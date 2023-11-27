using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace PandaDotNet.Repo.Drivers.LiteDB
{
    /// <summary>
    /// Various extensions that make LiteDB quicker to use
    /// </summary>
    public static class LiteDbExtensions
    {
        /// <summary>
        /// Returns the given value as a <see cref="BsonValue"/>.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static BsonValue AsBsonValue(this object o)
        {
            return new(o);
        }

        /// <summary>
        /// Adds a singleton <see cref="ILiteDatabase"/> instance
        /// to the service collection which can be used to construct
        /// <see cref="IRepository{TEntity,TKey}"/> implementations
        /// using LiteDB.
        /// </summary>
        /// <param name="services">The service collection to add to</param>
        /// <param name="connectionString">
        /// The connection string to be used to connect to LiteDB
        /// </param>
        /// <returns></returns>
        public static IServiceCollection AddLiteDbDriver(
            this IServiceCollection services,
            string connectionString)
        {
            services.TryAddSingleton<ILiteDatabase>(
                s => new LiteDatabase(connectionString));
            return services;
        }
    }
}