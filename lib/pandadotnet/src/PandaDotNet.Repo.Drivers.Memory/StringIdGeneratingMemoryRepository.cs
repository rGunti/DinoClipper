using System;
using System.Collections.Generic;
using PandaDotNet.Utils;

namespace PandaDotNet.Repo.Drivers.Memory
{
    /// <summary>
    /// <para>
    /// An extension of <see cref="MemoryRepository{TEntity,TKey}"/> for entities
    /// with string IDs. This implementation will generate a random ID for every
    /// object inserted.
    /// </para>
    /// <para>
    /// This implementation uses <see cref="Guid.NewGuid"/> to generate a random
    /// string ID and ensures that an existing object is not overwritten.
    /// If you want to use a different way of generating IDs, you can override
    /// <see cref="GenerateNewId"/> with your own logic.
    /// </para>
    /// <para>
    /// Note that ID generation does not occur when initializing the repository.
    /// </para>
    /// </summary>
    /// <seealso cref="MemoryRepository{TEntity,TKey}"/>
    /// <inheritdoc cref="MemoryRepository{TEntity,TKey}"/>
    public class StringIdGeneratingMemoryRepository<TEntity> :
        MemoryRepository<TEntity, string>
        where TEntity : class, IEntity<string>
    {
        /// <inheritdoc cref="MemoryRepository{TEntity,TKey}(System.Collections.Generic.IEnumerable{TEntity})"/>
        public StringIdGeneratingMemoryRepository(IEnumerable<TEntity> entities = null)
            : base(entities)
        {
        }

        /// <summary>
        /// Generates a new, unique ID and returns it.
        /// </summary>
        /// <returns></returns>
        protected virtual string GenerateNewId()
        {
            string newId;
            do
            {
                newId = Guid.NewGuid().ToString();
            } while (ExistsWithId(newId));
            return newId;
        }

        /// <inheritdoc />
        public override TEntity Insert(TEntity e)
        {
            e.Id = e.Id.Or(GenerateNewId);
            return base.Insert(e);
        }
    }
}