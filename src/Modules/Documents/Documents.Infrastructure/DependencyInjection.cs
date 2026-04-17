using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Secura.DistributionCrm.BuildingBlocks.Application.Abstractions;
using Secura.DistributionCrm.Documents.Application.Abstractions;
using Secura.DistributionCrm.Documents.Infrastructure.Persistence;
using Secura.DistributionCrm.Documents.Infrastructure.Repositories;

namespace Secura.DistributionCrm.Documents.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDocumentsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SecuraCrm")
            ?? throw new InvalidOperationException("Connection string 'SecuraCrm' not found.");

        services.AddDbContext<DocumentsDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(DocumentsDbContext).Assembly.FullName);
                sql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
                sql.CommandTimeout(30);
            }));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<DocumentsDbContext>());
        services.AddScoped<IDocumentRepository, DocumentRepository>();

        return services;
    }
}
