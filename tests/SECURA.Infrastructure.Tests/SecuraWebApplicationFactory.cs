using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SECURA.Infrastructure.Persistence;

namespace SECURA.Infrastructure.Tests;

public sealed class SecuraWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove all SqlServer DbContext registrations.
            // EF Core 9 uses IDbContextOptionsConfiguration<T> to compose options —
            // we must remove those too, otherwise both SqlServer and InMemory end up
            // in the built DbContextOptions, which EF Core rejects.
            var toRemove = services
                .Where(d => d.ServiceType == typeof(IDbContextOptionsConfiguration<SecuraDbContext>)
                         || d.ServiceType == typeof(DbContextOptions<SecuraDbContext>)
                         || d.ServiceType == typeof(SecuraDbContext))
                .ToList();
            foreach (var d in toRemove)
                services.Remove(d);

            services.AddDbContext<SecuraDbContext>(opts =>
                opts.UseInMemoryDatabase(_dbName));

            // Replace authentication with a test scheme that auto-authenticates
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
        });
    }
}
