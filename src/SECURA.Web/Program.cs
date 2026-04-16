using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using MudBlazor.Services;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Serilog;
using SECURA.Application;
using SECURA.Infrastructure;
using SECURA.Web.Auth;
using SECURA.Web.Middleware;
using SECURA.Web.Components;

// ─────────────────────────────────────────────────────────────────────────────
// Bootstrap Serilog from configuration before the host is built
// ─────────────────────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting SECURA CRM...");

    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ────────────────────────────────────────────────────────────
    builder.Host.UseSerilog((context, services, config) =>
        config.ReadFrom.Configuration(context.Configuration)
              .ReadFrom.Services(services)
              .Enrich.FromLogContext());

    // ── Application + Infrastructure ───────────────────────────────────────
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // ── HttpContextAccessor (needed for CurrentUserService) ────────────────
    builder.Services.AddHttpContextAccessor();

    // ── Authentication ─────────────────────────────────────────────────────
    if (builder.Environment.IsDevelopment())
    {
        // Dev mode: auto-authenticate as CarrierAdmin; no Entra ID required.
        builder.Services.AddAuthentication(DevAuthHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, DevAuthHandler>(
                DevAuthHandler.SchemeName, _ => { });
    }
    else
    {
        builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));
    }

    // ── Authorization: policy-based RBAC ───────────────────────────────────
    builder.Services.AddAuthorization(SecuraPolicies.ConfigurePolicies);

    // ── OpenTelemetry ──────────────────────────────────────────────────────
    var otelEndpoint = builder.Configuration["OpenTelemetry:Endpoint"];
    var serviceName = builder.Configuration["OpenTelemetry:ServiceName"] ?? "SECURA.CRM";

    builder.Services.AddOpenTelemetry()
        .ConfigureResource(r => r.AddService(serviceName))
        .WithTracing(t =>
        {
            t.AddAspNetCoreInstrumentation()
             .AddHttpClientInstrumentation()
             .AddSqlClientInstrumentation();
            if (!string.IsNullOrEmpty(otelEndpoint))
                t.AddOtlpExporter(o => o.Endpoint = new Uri(otelEndpoint));
        })
        .WithMetrics(m =>
        {
            m.AddAspNetCoreInstrumentation();
            if (!string.IsNullOrEmpty(otelEndpoint))
                m.AddOtlpExporter(o => o.Endpoint = new Uri(otelEndpoint));
        });

    // ── Health Checks ──────────────────────────────────────────────────────
    var connectionString = builder.Configuration.GetConnectionString("SecuraCrm");
    builder.Services.AddHealthChecks()
        .AddSqlServer(connectionString!, name: "sql-server", tags: ["db"]);

    // ── Exception Handling ─────────────────────────────────────────────────
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    // ── Controllers (REST API) ─────────────────────────────────────────────
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // ── Blazor ─────────────────────────────────────────────────────────────
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents()
        .AddInteractiveWebAssemblyComponents();

    builder.Services.AddMudServices();

    // ── Cascade auth state to Blazor ───────────────────────────────────────
    builder.Services.AddCascadingAuthenticationState();

    var app = builder.Build();

    // ── Middleware pipeline ────────────────────────────────────────────────
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler();
        app.UseHsts();
    }

    if (!app.Environment.IsDevelopment())
        app.UseHttpsRedirection();
    app.UseMiddleware<RequestContextMiddleware>();
    app.UseSerilogRequestLogging();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseAntiforgery();

    // ── Health endpoint ────────────────────────────────────────────────────
    app.MapHealthChecks("/health");

    // ── REST API controllers ───────────────────────────────────────────────
    app.MapControllers();

    // ── Blazor ─────────────────────────────────────────────────────────────
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode()
        .AddInteractiveWebAssemblyRenderMode()
        .AddAdditionalAssemblies(typeof(SECURA.Web.Client._Imports).Assembly);

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "SECURA CRM terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
