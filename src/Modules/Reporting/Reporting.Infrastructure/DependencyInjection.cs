using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Secura.DistributionCrm.Reporting.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddReportingModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Reporting module uses repositories from Agencies and Submissions modules.
        // No dedicated persistence required for this module.
        return services;
    }
}
