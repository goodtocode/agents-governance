using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Goodtocode.Agents.Governance.Application;

/// <summary>
/// DI registration for repeatability governance.
/// </summary>
public static class GovernanceRepeatabilityServiceCollectionExtensions
{
    /// <summary>
    /// Registers the default <see cref="IRepeatPolicy"/> and <see cref="IRepeatRecorder"/> with validated options.
    /// Apps register their own <see cref="IRepeatSourceResolver"/>.
    /// </summary>
    public static IServiceCollection AddGovernanceRepeatability(
        this IServiceCollection services,
        Action<RepeatabilityOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var builder = services.AddOptions<RepeatabilityOptions>().ValidateDataAnnotations().ValidateOnStart();
        if (configure is not null)
        {
            builder.Configure(configure);
        }

        services.TryAddSingleton<IRepeatPolicy, DefaultRepeatPolicy>();
        services.TryAddSingleton<IRepeatRecorder, DefaultRepeatRecorder>();
        return services;
    }
}
