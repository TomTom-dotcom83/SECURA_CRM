using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Secura.DistributionCrm.Administration.Infrastructure;
using Secura.DistributionCrm.Agencies.Infrastructure;
using Secura.DistributionCrm.BuildingBlocks.Application.Behaviors;
using Secura.DistributionCrm.Claims.Infrastructure;
using Secura.DistributionCrm.Documents.Infrastructure;
using Secura.DistributionCrm.Relationships.Infrastructure;
using Secura.DistributionCrm.Reporting.Infrastructure;
using Secura.DistributionCrm.Submissions.Infrastructure;

namespace Secura.DistributionCrm.Host.CompositionRoot;

public static class ModuleRegistration
{
    public static IServiceCollection AddAllModules(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register MediatR from all application assemblies
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(
                typeof(Agencies.Application.DependencyInjection).Assembly,
                typeof(Relationships.Application.DependencyInjection).Assembly,
                typeof(Submissions.Application.DependencyInjection).Assembly,
                typeof(Claims.Application.DependencyInjection).Assembly,
                typeof(Documents.Application.DependencyInjection).Assembly,
                typeof(Reporting.Application.DependencyInjection).Assembly,
                typeof(Administration.Application.DependencyInjection).Assembly
            );

            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        });

        // Register FluentValidation validators from all application assemblies
        services.AddValidatorsFromAssemblies(
        [
            typeof(Agencies.Application.DependencyInjection).Assembly,
            typeof(Relationships.Application.DependencyInjection).Assembly,
            typeof(Submissions.Application.DependencyInjection).Assembly,
            typeof(Claims.Application.DependencyInjection).Assembly,
            typeof(Documents.Application.DependencyInjection).Assembly,
            typeof(Reporting.Application.DependencyInjection).Assembly,
            typeof(Administration.Application.DependencyInjection).Assembly,
        ]);

        // Register all infrastructure modules
        services
            .AddAgenciesModule(configuration)
            .AddRelationshipsModule(configuration)
            .AddSubmissionsModule(configuration)
            .AddClaimsModule(configuration)
            .AddDocumentsModule(configuration)
            .AddReportingModule(configuration)
            .AddAdministrationModule(configuration);

        return services;
    }
}
