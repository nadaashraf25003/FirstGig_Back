using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using FirstGIG.BuildingBlocks.Application.Behaviors;

namespace FirstGIG.Identity.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        services.AddAutoMapper(assembly);

        return services;
    }
}
