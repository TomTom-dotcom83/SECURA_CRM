using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Secura.DistributionCrm.Administration.Application.AppetiteTags;
using Secura.DistributionCrm.Administration.Infrastructure.Persistence;
using Secura.DistributionCrm.Administration.Infrastructure.Repositories;
using Secura.DistributionCrm.BuildingBlocks.Application.Abstractions;

namespace Secura.DistributionCrm.Administration.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAdministrationModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SecuraCrm")
            ?? throw new InvalidOperationException("Connection string 'SecuraCrm' not found.");

        services.AddDbContext<AdministrationDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(AdministrationDbContext).Assembly.FullName);
                sql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
                sql.CommandTimeout(30);
            }));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AdministrationDbContext>());
        services.AddScoped<IAppetiteTagRepository, AppetiteTagRepository>();

        return services;
    }
}
