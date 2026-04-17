using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Secura.DistributionCrm.BuildingBlocks.Application.Abstractions;
using Secura.DistributionCrm.Submissions.Application.Abstractions;
using Secura.DistributionCrm.Submissions.Infrastructure.Persistence;
using Secura.DistributionCrm.Submissions.Infrastructure.Repositories;

namespace Secura.DistributionCrm.Submissions.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSubmissionsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SecuraCrm")
            ?? throw new InvalidOperationException("Connection string 'SecuraCrm' not found.");

        services.AddDbContext<SubmissionsDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(SubmissionsDbContext).Assembly.FullName);
                sql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
                sql.CommandTimeout(30);
            }));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<SubmissionsDbContext>());
        services.AddScoped<ISubmissionRepository, SubmissionRepository>();

        return services;
    }
}
