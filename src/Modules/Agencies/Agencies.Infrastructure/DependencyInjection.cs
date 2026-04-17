using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Secura.DistributionCrm.Agencies.Application.Abstractions;
using Secura.DistributionCrm.Agencies.Infrastructure.BackgroundServices;
using Secura.DistributionCrm.Agencies.Infrastructure.Persistence;
using Secura.DistributionCrm.Agencies.Infrastructure.Repositories;
using Secura.DistributionCrm.Agencies.Infrastructure.Services;
using Secura.DistributionCrm.Agencies.Infrastructure.Webhooks;
using Secura.DistributionCrm.BuildingBlocks.Application.Abstractions;

namespace Secura.DistributionCrm.Agencies.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAgenciesModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SecuraCrm")
            ?? throw new InvalidOperationException("Connection string 'SecuraCrm' not found.");

        services.AddDbContext<AgenciesDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(AgenciesDbContext).Assembly.FullName);
                sql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
                sql.CommandTimeout(30);
            }));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AgenciesDbContext>());
        services.AddScoped<IAgencyRepository, AgencyRepository>();
        services.AddScoped<IProducerRepository, ProducerRepository>();
        services.AddScoped<IChecklistRepository, ChecklistRepository>();
        services.AddScoped<INiprService, NiprClient>();

        services.AddHttpClient("webhook");
        services.AddScoped<WebhookDispatcher>();

        // MediatR handlers in this assembly (webhook handlers, etc.)
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddHostedService<ComplianceScanJob>();

        return services;
    }
}
