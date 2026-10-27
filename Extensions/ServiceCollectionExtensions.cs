namespace Furrow.Payments.Extensions;

using Furrow.Payments;
using Furrow.Payments.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

/// <summary>
/// Service collection extensions for registering Furrow.Payments services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Paynow client with the dependency injection container using an action delegate.
    /// </summary>
    public static IServiceCollection AddFurrowPayments(
        this IServiceCollection services,
        Action<PaynowOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.Configure(configureOptions);

        services.AddHttpClient<IPaynowClient, PaynowClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<PaynowOptions>>().Value;
            if (options.TimeoutSeconds > 0)
            {
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            }
        });

        return services;
    }

    /// <summary>
    /// Registers the Paynow client binding configuration from an <see cref="IConfigurationSection"/>.
    /// </summary>
    public static IServiceCollection AddFurrowPayments(
        this IServiceCollection services,
        IConfiguration configurationSection)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configurationSection);

        services.Configure<PaynowOptions>(configurationSection);

        services.AddHttpClient<IPaynowClient, PaynowClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<PaynowOptions>>().Value;
            if (options.TimeoutSeconds > 0)
            {
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            }
        });

        return services;
    }

    /// <summary>
    /// Backwards-compatible alias for <see cref="AddFurrowPayments(IServiceCollection, Action{PaynowOptions})"/>.
    /// </summary>
    public static IServiceCollection AddHavanoPayments(
        this IServiceCollection services,
        Action<PaynowOptions> configureOptions) =>
        services.AddFurrowPayments(configureOptions);

    /// <summary>
    /// Backwards-compatible alias for <see cref="AddFurrowPayments(IServiceCollection, IConfiguration)"/>.
    /// </summary>
    public static IServiceCollection AddHavanoPayments(
        this IServiceCollection services,
        IConfiguration configurationSection) =>
        services.AddFurrowPayments(configurationSection);
}
