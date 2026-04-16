#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
# SECURA CRM — Codespace startup script
# Usage: ./start.sh
# Starts SQL Server, applies migrations, and runs the app on http://localhost:5280
# ─────────────────────────────────────────────────────────────────────────────
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
APP_URL="http://localhost:5280"
LOG_FILE="/tmp/secura_app.log"

# ── Colours ────────────────────────────────────────────────────────────────
GREEN='\033[0;32m'; YELLOW='\033[1;33m'; RED='\033[0;31m'; NC='\033[0m'
info()    { echo -e "${GREEN}[SECURA]${NC} $*"; }
warn()    { echo -e "${YELLOW}[SECURA]${NC} $*"; }
die()     { echo -e "${RED}[SECURA] ERROR:${NC} $*" >&2; exit 1; }

# ── 1. Kill any stale app processes ────────────────────────────────────────
info "Checking for stale processes on port 5280..."
STALE=$(lsof -ti :5280 2>/dev/null || true)
if [[ -n "$STALE" ]]; then
    warn "Killing stale process(es) on port 5280: $STALE"
    kill "$STALE" 2>/dev/null || true
    sleep 1
fi

# ── 2. Start SQL Server via docker-compose ─────────────────────────────────
info "Starting SQL Server (docker-compose)..."
cd "$REPO_ROOT"

# If the named container is in a broken state, remove it so compose can recreate it
if docker inspect secura_sqlserver &>/dev/null; then
    STATUS=$(docker inspect -f '{{.State.Status}}' secura_sqlserver 2>/dev/null || echo "missing")
    if [[ "$STATUS" != "running" ]]; then
        warn "Container is in state '$STATUS' — removing and recreating..."
        docker rm -f secura_sqlserver 2>/dev/null || true
    fi
fi

docker compose up -d sqlserver

# ── 3. Wait for SQL Server to accept connections ───────────────────────────
info "Waiting for SQL Server to be ready..."
RETRIES=30
until docker exec secura_sqlserver \
        /opt/mssql-tools18/bin/sqlcmd -S localhost -U SA -P "SecuraDev#2024" \
        -Q "SELECT 1" -C &>/dev/null; do
    RETRIES=$((RETRIES - 1))
    if [[ $RETRIES -le 0 ]]; then
        die "SQL Server did not become ready in time. Check: docker logs secura_sqlserver"
    fi
    sleep 3
done
info "SQL Server is ready."

# ── 4. Apply EF Core migrations ────────────────────────────────────────────
info "Applying EF Core migrations..."
dotnet ef database update \
    --project "$REPO_ROOT/src/SECURA.Infrastructure/SECURA.Infrastructure.csproj" \
    --startup-project "$REPO_ROOT/src/SECURA.Web/SECURA.Web.csproj" \
    2>&1 | grep -v "^Build" | grep -v "^$" || die "Migration failed."
info "Migrations applied."

# ── 5. Start the app in the background ────────────────────────────────────
info "Starting SECURA CRM at $APP_URL ..."
ASPNETCORE_ENVIRONMENT=Development \
ASPNETCORE_URLS="$APP_URL" \
dotnet run \
    --project "$REPO_ROOT/src/SECURA.Web/SECURA.Web.csproj" \
    --no-launch-profile \
    > "$LOG_FILE" 2>&1 &
APP_PID=$!
echo "$APP_PID" > /tmp/secura_app.pid

# ── 6. Wait for the app to start ──────────────────────────────────────────
info "Waiting for app to start (PID $APP_PID)..."
RETRIES=30
until grep -q "Now listening\|Hosting failed\|Unhandled exception" "$LOG_FILE" 2>/dev/null; do
    if ! kill -0 "$APP_PID" 2>/dev/null; then
        die "App process exited unexpectedly. Check logs: $LOG_FILE"
    fi
    RETRIES=$((RETRIES - 1))
    if [[ $RETRIES -le 0 ]]; then
        die "App did not start in time. Check logs: $LOG_FILE"
    fi
    sleep 2
done

if grep -q "Hosting failed\|Unhandled exception" "$LOG_FILE"; then
    die "App failed to start. Check logs: $LOG_FILE"
fi

# ── 7. Health check ────────────────────────────────────────────────────────
HEALTH=$(curl -s --max-time 5 "$APP_URL/health" || echo "unreachable")
if [[ "$HEALTH" == "Healthy" ]]; then
    echo ""
    info "SECURA CRM is running at ${GREEN}$APP_URL${NC}"
    info "Logs: tail -f $LOG_FILE"
    info "Stop: kill \$(cat /tmp/secura_app.pid)"
else
    die "Health check returned: $HEALTH. Check logs: $LOG_FILE"
fi
