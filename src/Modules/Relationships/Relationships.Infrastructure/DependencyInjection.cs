using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Secura.DistributionCrm.BuildingBlocks.Application.Abstractions;
using Secura.DistributionCrm.Relationships.Application.Abstractions;
using Secura.DistributionCrm.Relationships.Infrastructure.Persistence;
using Secura.DistributionCrm.Relationships.Infrastructure.Repositories;

namespace Secura.DistributionCrm.Relationships.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddRelationshipsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SecuraCrm")
            ?? throw new InvalidOperationException("Connection string 'SecuraCrm' not found.");

        services.AddDbContext<RelationshipsDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(RelationshipsDbContext).Assembly.FullName);
                sql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
                sql.CommandTimeout(30);
            }));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<RelationshipsDbContext>());
        services.AddScoped<IInteractionRepository, InteractionRepository>();

        return services;
    }
}
