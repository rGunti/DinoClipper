using System;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
using PandaDotNet.Utils;

namespace PandaDotNet.Repo.Drivers.MongoDB
{
    /// <summary>
    /// Various extensions that make LiteDB quicker to use
    /// </summary>
    public static class MongoDbExtensions
    {
        /// <summary>
        /// <para>
        /// Adds all required dependencies to the service collection
        /// to construct an <see cref="IMongoClient"/> and
        /// <see cref="IMongoDatabase"/> instance using the provided
        /// <see cref="connectionString"/>.
        /// </para>
        /// <para>
        /// All objects added to the service collection are added
        /// as singletons.
        /// </para>
        /// <para>
        /// Additionally a <see cref="StringObjectIdGenerator"/>
        /// is installed to automatically generate String IDs upon insertion
        /// since this is not the default behaviour of the MongoDB driver.
        /// </para>
        /// </summary>
        /// <param name="services">The service collection to add to</param>
        /// <param name="connectionString">
        /// A URL used to connect to a MongoDB instance.
        /// </param>
        /// <returns></returns>
        public static IServiceCollection AddMongoDbDriver(
            this IServiceCollection services,
            string connectionString)
        {
            connectionString.OrThrow(() => new ArgumentNullException(nameof(connectionString)));
            
            // By default, MongoDB uses ObjectIds as primary keys and if string IDs are used
            // they won't automatically be generated. Adding the StringObjectIdGenerator resolves
            // this problem, allowing the use of simple strings as IDs.
            BsonSerializer.RegisterIdGenerator(typeof(string), StringObjectIdGenerator.Instance);

            return services
                .AddSingleton<MongoUrl>(MongoUrl.Create(connectionString))
                .AddSingleton<IMongoClient>(s => new MongoClient(s.GetMongoDbUrl()))
                .AddSingleton<IMongoDatabase>(s => s.GetMongoClient().GetDatabase(s.GetMongoDbUrl().DatabaseName));
        }

        /// <summary>
        /// Returns a registered instance of <see cref="MongoUrl"/>.
        /// </summary>
        public static MongoUrl GetMongoDbUrl(this IServiceProvider services)
            => services.GetRequiredService<MongoUrl>();

        /// <summary>
        /// Returns a registered instance of <see cref="IMongoClient"/>.
        /// </summary>
        public static IMongoClient GetMongoClient(this IServiceProvider services)
            => services.GetRequiredService<IMongoClient>();
    }
}