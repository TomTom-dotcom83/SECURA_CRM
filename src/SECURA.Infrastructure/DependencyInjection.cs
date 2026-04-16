using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SECURA.Application.Common.Interfaces;
using SECURA.Domain.Entities;
using SECURA.Infrastructure.BackgroundServices;
using SECURA.Infrastructure.Persistence;
using SECURA.Infrastructure.Repositories;
using SECURA.Infrastructure.Services;
using SECURA.Infrastructure.Webhooks;

namespace SECURA.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SecuraCrm")
            ?? throw new InvalidOperationException("Connection string 'SecuraCrm' not found.");

        services.AddDbContext<SecuraDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(SecuraDbContext).Assembly.FullName);
                sql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
                sql.CommandTimeout(30);
            });
        });

        // UnitOfWork = DbContext
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<SecuraDbContext>());

        // Repositories
        services.AddScoped<IAgencyRepository, AgencyRepository>();
        services.AddScoped<IProducerRepository, ProducerRepository>();
        services.AddScoped<ISubmissionRepository, SubmissionRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IChecklistRepository, ChecklistRepository>();
        services.AddScoped<IInteractionRepository, InteractionRepository>();
        services.AddScoped<IRepository<Interaction, Guid>>(
            sp => sp.GetRequiredService<IInteractionRepository>());
        services.AddScoped<IClaimRepository, ClaimRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();

        // Services
        services.AddScoped<ICurrentUser, CurrentUserService>();
        services.AddScoped<INiprService, NiprIntegrationService>();

        // Register Infrastructure-layer MediatR handlers (webhook + event handlers)
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        // Webhooks
        services.AddHttpClient("webhook");
        services.AddScoped<WebhookDispatcher>();

        // Background Services
        services.AddHostedService<OutboxDispatcher>();
        services.AddHostedService<ComplianceCheckService>();

        return services;
    }
}
