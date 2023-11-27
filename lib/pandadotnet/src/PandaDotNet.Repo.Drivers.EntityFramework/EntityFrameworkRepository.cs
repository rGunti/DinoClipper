using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PandaDotNet.Utils;

namespace PandaDotNet.Repo.Drivers.EntityFramework
{
    public class EntityFrameworkRepository<TEntity, TKey, TContext> :
        IRepository<TEntity, TKey>
        where TEntity : class, IEntity<TKey>
        where TContext : DbContext
    {
        /// <summary>
        /// The database context used by Entity Framework
        /// </summary>
        protected readonly TContext _dbContext;

        /// <param name="dbContext"></param>
        public EntityFrameworkRepository(TContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Returns the <see cref="DbSet{TEntity}"/> instance.
        /// </summary>
        protected virtual DbSet<TEntity> Set => _dbContext.Set<TEntity>();

        /// <summary>
        /// </summary>
        public virtual IEnumerable<TEntity> All => Set;

        /// <inheritdoc />
        public virtual TEntity GetById(TKey id)
        {
            return Set.Find(id);
        }

        /// <inheritdoc />
        public virtual bool ExistsWithId(TKey id)
        {
            return Set.Find(id) != null;
        }

        /// <inheritdoc />
        public virtual TEntity this[TKey id] => GetById(id)
            .OrThrow(() => new ArgumentOutOfRangeException(nameof(id),
                $"No entity of type {typeof(TEntity)} with ID {id} exists"));

        /// <inheritdoc />
        public virtual TEntity Insert(TEntity e)
        {
            Set.Add(e);
            _dbContext.SaveChanges();
            return GetById(e.Id);
        }

        /// <inheritdoc />
        public virtual TEntity Update(TEntity e)
        {
            Set.Update(e);
            _dbContext.SaveChanges();
            return GetById(e.Id);
        }

        /// <inheritdoc />
        public virtual void Delete(TKey id)
        {
            Set.Remove(GetById(id));
            _dbContext.SaveChanges();
        }

        /// <inheritdoc />
        public virtual void Delete(TEntity e)
        {
            Delete(e.Id);
        }
    }
}