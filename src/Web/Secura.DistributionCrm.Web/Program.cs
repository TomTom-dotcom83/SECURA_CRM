using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using MudBlazor.Services;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Serilog;
using Secura.DistributionCrm.Web.Auth;
using Secura.DistributionCrm.BuildingBlocks.Application.Abstractions;
using Secura.DistributionCrm.BuildingBlocks.Infrastructure.Auth;
using Secura.DistributionCrm.Host.CompositionRoot;
using Secura.DistributionCrm.Web.Components;
using Secura.DistributionCrm.Web.Middleware;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Secura.DistributionCrm.Web...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, config) =>
        config.ReadFrom.Configuration(context.Configuration)
              .ReadFrom.Services(services)
              .Enrich.FromLogContext());

    // All modules
    builder.Services.AddAllModules(builder.Configuration);

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUser, CurrentUserService>();

    // Authentication
    if (builder.Environment.IsDevelopment())
    {
        builder.Services.AddAuthentication(DevAuthHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, DevAuthHandler>(
                DevAuthHandler.SchemeName, _ => { });
    }
    else
    {
        builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));
    }

    builder.Services.AddAuthorization(SecuraPolicies.ConfigurePolicies);

    var otelEndpoint = builder.Configuration["OpenTelemetry:Endpoint"];
    var serviceName = builder.Configuration["OpenTelemetry:ServiceName"] ?? "Secura.DistributionCrm.Web";

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

    var connectionString = builder.Configuration.GetConnectionString("SecuraCrm");
    builder.Services.AddHealthChecks()
        .AddSqlServer(connectionString!, name: "sql-server", tags: ["db"]);

    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    builder.Services.AddMudServices();
    builder.Services.AddCascadingAuthenticationState();

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler();
        app.UseHsts();
        app.UseHttpsRedirection();
    }

    app.UseMiddleware<RequestContextMiddleware>();
    app.UseSerilogRequestLogging();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseAntiforgery();

    app.MapHealthChecks("/health");
    app.MapControllers();

    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Secura.DistributionCrm.Web terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
