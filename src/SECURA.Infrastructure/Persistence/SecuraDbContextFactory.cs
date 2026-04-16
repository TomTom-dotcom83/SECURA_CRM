using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SECURA.Infrastructure.Persistence;

/// <summary>
/// Used by EF Core CLI tools (dotnet ef migrations add).
/// </summary>
public sealed class SecuraDbContextFactory : IDesignTimeDbContextFactory<SecuraDbContext>
{
    public SecuraDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = config.GetConnectionString("SecuraCrm")
            ?? "Server=localhost,1433;Database=SecuraCrm;User Id=sa;Password=SecuraDev#2024;TrustServerCertificate=True";

        var optionsBuilder = new DbContextOptionsBuilder<SecuraDbContext>();
        optionsBuilder.UseSqlServer(connectionString, sql =>
        {
            sql.MigrationsAssembly(typeof(SecuraDbContext).Assembly.FullName);
            sql.EnableRetryOnFailure(3);
        });

        return new SecuraDbContext(optionsBuilder.Options);
    }
}
