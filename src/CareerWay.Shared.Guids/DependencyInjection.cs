using CareerWay.Shared.Core.Guards;
using CareerWay.Shared.Guids;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddCustomGuidGenerator(
        this IServiceCollection services)
    {
        Guard.Against.Null(services, nameof(services));
        return services.AddSingleton<IGuidGenerator, CustomGuidGenerator>();
    }

    public static IServiceCollection AddSequentialGuidGenerator(
       this IServiceCollection services)
    {
        Guard.Against.Null(services, nameof(services));
        return services.AddSingleton<IGuidGenerator, SequentialGuidGenerator>();
    }
}
