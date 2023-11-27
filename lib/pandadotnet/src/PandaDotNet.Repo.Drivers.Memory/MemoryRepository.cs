using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using PandaDotNet.Utils;

namespace PandaDotNet.Repo.Drivers.Memory
{
    /// <summary>
    /// <para>
    /// An in-memory implementation of an <see cref="IRepository{TEntity,TKey}"/>
    /// which stores its data in a <see cref="ConcurrentDictionary{TKey,TValue}"/>.
    /// </para>
    /// <para>
    /// The main purpose of this implementation is for use in Unit Test environments
    /// where data doesn't have to be persisted.
    /// </para>
    /// </summary>
    /// <seealso cref="IRepository{TEntity,TKey}"/>
    /// <inheritdoc />
    public class MemoryRepository<TEntity, TKey> :
        IRepository<TEntity, TKey>
        where TEntity : IEntity<TKey>
    {
        /// <summary>
        /// The underlying object that stores all objects.
        /// </summary>
        protected readonly ConcurrentDictionary<TKey, TEntity> _database;

        /// <summary>
        /// Constructs a new in-memory repository.
        /// You can optionally provide an enumerable which can be used
        /// to initialize the underlying database.
        /// </summary>
        /// <param name="entities"></param>
        public MemoryRepository(IEnumerable<TEntity> entities = null)
        {
            _database = new ConcurrentDictionary<TKey, TEntity>(
                entities.Or(Array.Empty<TEntity>())
                    .ToDictionary(e => e.Id, e => e));
        }

        /// <summary>
        /// Exposes the underlying <see cref="ConcurrentDictionary{TKey,TValue}"/>
        /// for unit testing.
        /// </summary>
        public ConcurrentDictionary<TKey, TEntity> Database => _database;

        /// <inheritdoc />
        public IEnumerable<TEntity> All => _database.Values;

        /// <inheritdoc />
        public virtual TEntity GetById(TKey id)
        {
            return _database.GetValueOrDefault(id);
        }

        /// <inheritdoc />
        public virtual TEntity this[TKey id] => GetById(id)
            .OrThrow(() => new ArgumentOutOfRangeException(nameof(id),
                $"No entity of type {typeof(TEntity)} with ID {id} exists"));

        /// <inheritdoc />
        public virtual bool ExistsWithId(TKey id)
        {
            return _database.ContainsKey(id);
        }

        /// <inheritdoc />
        public virtual TEntity Insert(TEntity e)
        {
            _database.AddOrUpdate(e.Id, _ => e, (_, _) => e);
            return GetById(e.Id);
        }

        /// <inheritdoc />
        public virtual TEntity Update(TEntity e)
        {
            TEntity existing = this[e.Id];
            _database.TryUpdate(e.Id, e, existing);
            return GetById(e.Id);
        }

        /// <inheritdoc />
        public virtual void Delete(TKey id)
        {
            _database.TryRemove(id, out _);
        }

        /// <inheritdoc />
        public virtual void Delete(TEntity e)
        {
            Delete(e.Id);
        }
    }
}