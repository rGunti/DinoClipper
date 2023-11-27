using System;
using System.Collections.Generic;
using MongoDB.Driver;
using PandaDotNet.Utils;

namespace PandaDotNet.Repo.Drivers.MongoDB
{
    /// <summary>
    /// <para>
    /// A MongoDB implementation of an <see cref="IRepository{TEntity,TKey}"/>.
    /// MongoDB is a document-oriented database using a server to store
    /// documents in collections. 
    /// </para>
    /// </summary>
    /// <inheritdoc/>
    public class MongoDbRepository<TEntity, TKey> :
        IRepository<TEntity, TKey>
        where TEntity : IEntity<TKey>
    {
        /// <summary>
        /// The <see cref="IMongoDatabase"/> instance used to interact with
        /// the underlying database.
        /// </summary>
        protected readonly IMongoDatabase _database;

        /// <summary>
        /// Constructs a new MongoDB repository using the provided database instance.
        /// </summary>
        /// <param name="database">
        /// An instance of <see cref="IMongoDatabase"/> to read/write data from/to
        /// </param>
        public MongoDbRepository(IMongoDatabase database)
        {
            _database = database;
            // ReSharper disable once VirtualMemberCallInConstructor
            CollectionName = typeof(TEntity).Name;
        }

        /// <summary>
        /// The name of the MongoDB collection.
        /// Will be auto-filled on construction using <see cref="TEntity"/>.
        /// </summary>
        public virtual string CollectionName { get; }

        /// <summary>
        /// Returns a new instance of <see cref="IMongoCollection{T}"/> to interact with the underlying
        /// database collection.
        /// </summary>
        public virtual IMongoCollection<TEntity> Collection => _database.GetCollection<TEntity>(CollectionName);

        /// <inheritdoc />
        public virtual IEnumerable<TEntity> All => Collection.AsQueryable();

        /// <summary>
        /// Provides quick access to <see cref="Builders{TDocument}.Filter"/>
        /// </summary>
        protected virtual FilterDefinitionBuilder<TEntity> Filter => Builders<TEntity>.Filter;

        /// <summary>
        /// Returns a <see cref="FilterDefinition{TDocument}"/> to filter for an ID.
        /// </summary>
        /// <param name="id">The ID to be filtered for</param>
        /// <returns></returns>
        protected FilterDefinition<TEntity> GetIdFilter(TKey id) => Filter.Eq(i => i.Id, id);

        /// <inheritdoc />
        public virtual TEntity GetById(TKey id)
        {
            return Collection.Find(GetIdFilter(id)).FirstOrDefault();
        }

        /// <inheritdoc />
        public virtual bool ExistsWithId(TKey id)
        {
            return Collection.CountDocuments(GetIdFilter(id)) > 0;
        }

        /// <inheritdoc />
        public virtual TEntity this[TKey id] => GetById(id)
            .OrThrow(() => new ArgumentOutOfRangeException(nameof(id),
                $"No entity of type {typeof(TEntity)} with ID {id} exists"));

        /// <inheritdoc />
        public virtual TEntity Insert(TEntity e)
        {
            Collection.InsertOne(e);
            return GetById(e.Id);
        }

        /// <inheritdoc />
        public virtual TEntity Update(TEntity e)
        {
            Collection.FindOneAndReplace(GetIdFilter(e.Id), e);
            return GetById(e.Id);
        }

        /// <inheritdoc />
        public virtual void Delete(TKey id)
        {
            Collection.FindOneAndDelete(GetIdFilter(id));
        }

        /// <inheritdoc />
        public virtual void Delete(TEntity e)
        {
            Delete(e.Id);
        }
    }
}