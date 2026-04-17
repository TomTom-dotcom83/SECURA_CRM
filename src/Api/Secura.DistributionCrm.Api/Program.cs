using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Serilog;
using Secura.DistributionCrm.Api.Auth;
using Secura.DistributionCrm.Api.Middleware;
using Secura.DistributionCrm.BuildingBlocks.Infrastructure.Auth;
using Secura.DistributionCrm.BuildingBlocks.Application.Abstractions;
using Secura.DistributionCrm.Host.CompositionRoot;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Secura.DistributionCrm.Api...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, config) =>
        config.ReadFrom.Configuration(context.Configuration)
              .ReadFrom.Services(services)
              .Enrich.FromLogContext());

    // Modules
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

    // OpenTelemetry
    var otelEndpoint = builder.Configuration["OpenTelemetry:Endpoint"];
    var serviceName = builder.Configuration["OpenTelemetry:ServiceName"] ?? "Secura.DistributionCrm";

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

    // Health Checks
    var connectionString = builder.Configuration.GetConnectionString("SecuraCrm");
    builder.Services.AddHealthChecks()
        .AddSqlServer(connectionString!, name: "sql-server", tags: ["db"]);

    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler();
        app.UseHsts();
        app.UseHttpsRedirection();
    }

    app.UseMiddleware<RequestContextMiddleware>();
    app.UseSerilogRequestLogging();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapHealthChecks("/health");
    app.MapControllers();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Secura.DistributionCrm.Api terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
