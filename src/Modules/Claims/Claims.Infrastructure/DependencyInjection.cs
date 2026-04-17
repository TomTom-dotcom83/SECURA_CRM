using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Secura.DistributionCrm.BuildingBlocks.Application.Abstractions;
using Secura.DistributionCrm.Claims.Application.Abstractions;
using Secura.DistributionCrm.Claims.Infrastructure.Persistence;
using Secura.DistributionCrm.Claims.Infrastructure.Repositories;

namespace Secura.DistributionCrm.Claims.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddClaimsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SecuraCrm")
            ?? throw new InvalidOperationException("Connection string 'SecuraCrm' not found.");

        services.AddDbContext<ClaimsDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(ClaimsDbContext).Assembly.FullName);
                sql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
                sql.CommandTimeout(30);
            }));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ClaimsDbContext>());
        services.AddScoped<IClaimRepository, ClaimRepository>();

        return services;
    }
}
