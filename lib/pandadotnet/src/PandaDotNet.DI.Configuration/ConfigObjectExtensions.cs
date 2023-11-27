using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PandaDotNet.DI.Configuration
{
    /// <summary>
    /// A set of extensions for adding configuration objects to DI containers
    /// </summary>
    public static class ConfigObjectExtensions
    {
        /// <summary>
        /// Adds a TConfig singleton to the service collection which holds configuration
        /// data from IConfiguration. The section to be used has to be provided as a
        /// parameter.
        /// Note: This uses the Configuration Binder from the Microsoft Extensions.
        /// If the section is not defined in the configuration file, this will error out,
        /// at latest when TConfig is requested.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="sourceSection">The section to be read</param>
        /// <typeparam name="TConfig">A POCO class containing fields for all sub values</typeparam>
        /// <returns></returns>
        public static IServiceCollection AddConfigObject<TConfig>(
            this IServiceCollection services,
            string sourceSection)
            where TConfig : class
        {
            return services
                .AddSingleton<TConfig>(s => s.GetRequiredService<IConfiguration>()
                    .GetSection(sourceSection)
                    .Get<TConfig>());
        }
    }
}