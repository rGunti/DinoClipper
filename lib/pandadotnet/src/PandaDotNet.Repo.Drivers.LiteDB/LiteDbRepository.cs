using System;
using System.Collections.Generic;
using LiteDB;
using PandaDotNet.Utils;

namespace PandaDotNet.Repo.Drivers.LiteDB
{
    /// <summary>
    /// <para>
    /// A LiteDB implementation of an <see cref="IRepository{TEntity,TKey}"/>.
    /// LiteDB is a NoSQL document store which stores its data in a single
    /// file on the file system.
    /// </para>
    /// </summary>
    /// <inheritdoc />
    public class LiteDbRepository<TEntity, TKey> :
        IRepository<TEntity, TKey>
        where TEntity : IEntity<TKey>
    {
        /// <summary>
        /// The <see cref="ILiteDatabase"/> instance used to interact with
        /// the underlying database.
        /// </summary>
        protected readonly ILiteDatabase _database;

        /// <summary>
        /// Constructs a new LiteDB repository using the provided database instance.
        /// </summary>
        /// <param name="database">
        /// An instance of <see cref="ILiteDatabase"/> to read/write data from/to
        /// </param>
        public LiteDbRepository(ILiteDatabase database)
        {
            _database = database;
        }

        /// <summary>
        /// Returns the corresponding collection in the <see cref="ILiteDatabase"/> instance.
        /// If you want to use a custom name for your collection, you can override this property.
        /// It is recommended to use this property every time you want to interact with your collection.
        /// </summary>
        protected virtual ILiteCollection<TEntity> Collection => _database.GetCollection<TEntity>();

        /// <inheritdoc />
        public IEnumerable<TEntity> All => Collection.Query()
            .ToEnumerable();

        /// <inheritdoc />
        public virtual TEntity GetById(TKey id)
        {
            return GetById(id.AsBsonValue());
        }

        /// <summary>
        /// Returns an object with the given primary key (as a <see cref="BsonValue"/>).
        /// </summary>
        /// <param name="bsonId"></param>
        /// <returns></returns>
        protected virtual TEntity GetById(BsonValue bsonId)
        {
            return Collection.FindById(bsonId);
        }

        /// <inheritdoc />
        public virtual bool ExistsWithId(TKey id)
        {
            return Collection.Exists(Query.EQ("_id", id.AsBsonValue()));
        }

        /// <inheritdoc />
        public virtual TEntity this[TKey id] => GetById(id)
            .OrThrow(() => new ArgumentOutOfRangeException(nameof(id),
                $"No entity of type {typeof(TEntity)} with ID {id} exists"));

        /// <inheritdoc />
        public TEntity Insert(TEntity e)
        {
            BsonValue newId = Collection.Insert(e);
            return GetById(newId);
        }

        /// <inheritdoc />
        public TEntity Update(TEntity e)
        {
            Collection.Update(e.Id.AsBsonValue(), e);
            return GetById(e.Id);
        }

        /// <inheritdoc />
        public void Delete(TKey id)
        {
            Delete(id.AsBsonValue());
        }

        /// <summary>
        /// Deletes the object in LiteDb using a <see cref="BsonValue"/>.
        /// </summary>
        /// <param name="bsonId"></param>
        protected virtual void Delete(BsonValue bsonId)
        {
            Collection.Delete(bsonId);
        }

        /// <inheritdoc />
        public void Delete(TEntity e)
        {
            Delete(e.Id);
        }
    }
}