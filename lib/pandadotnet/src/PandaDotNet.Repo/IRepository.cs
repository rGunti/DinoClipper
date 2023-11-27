using System.Collections.Generic;

namespace PandaDotNet.Repo
{
    /// <summary>
    /// <para>
    /// A repository is a service class that provides
    /// access to the underlying storage facility, like
    /// a database.
    /// </para>
    /// <para>
    /// A repository provides basic operations to access
    /// and modify the stored data.
    /// </para>
    /// <para>
    /// The implementation does not have to guarantee
    /// any kind of state. Once an object leaves the
    /// repository if is no longer attached to the
    /// data source and any modification may be lost
    /// when the object is disposed of without invoking
    /// an <see cref="Insert"/> or <see cref="Update"/>
    /// command. 
    /// </para>
    /// </summary>
    public interface IRepository<TEntity, in TKey>
        where TEntity : IEntity<TKey>
    {
        /// <summary>
        /// This property exposes the underlying data source
        /// using the IEnumerable interface. It shall provide
        /// a quick way to query for data from the source.
        /// However its use is discouraged for more complex
        /// operations, which warrant a custom implementation.
        /// </summary>
        IEnumerable<TEntity> All { get; }

        /// <summary>
        /// <para>
        /// Requests the data source to lookup and return an
        /// object with the given primary key.
        /// If the object cannot be found, null/default
        /// is returned.
        /// </para>
        ///
        /// <para>
        /// If you prefer to have an exception thrown if the
        /// requested record doesn't exist, use the indexer
        /// <see cref="this"/>.
        /// </para>
        /// </summary>
        /// <param name="id">The primary key to look up</param>
        /// <returns>An object having the provided primary key, or null/default</returns>
        /// <seealso cref="this"/>
        TEntity GetById(TKey id);

        /// <summary>
        /// <para>
        /// Requests the data source to check if an object
        /// with the given primary key exists.
        /// </para>
        /// </summary>
        /// <param name="id">The primary key to look up</param>
        /// <returns>
        /// Returns true, if an object with the given primary key exists.
        /// Returns false otherwise.
        /// </returns>
        bool ExistsWithId(TKey id);
        
        /// <summary>
        /// <para>
        /// Requests the data source to lookup and return
        /// an object with the given primary key.
        /// If the object cannot be found, an ArgumentOutOfRangeException
        /// may be thrown.
        /// </para>
        ///
        /// <para>
        /// If you prefer to have a default value returned if the record
        /// is missing, use <see cref="GetById"/>.
        /// </para>
        ///
        /// <para>
        /// This accessor cannot be used to store data.
        /// </para>
        /// </summary>
        /// <param name="id">The primary key to look up</param>
        /// <seealso cref="GetById"/>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// when no object with the given primary key exists
        /// </exception>
        TEntity this[TKey id] { get; }

        /// <summary>
        /// Requests the data source to store the given object
        /// as a new entry and then returning it. The returned
        /// object also contains the new primary key if it
        /// is to be set by the underlying data source.
        /// </summary>
        /// <param name="e">Tne object to insert</param>
        /// <returns></returns>
        TEntity Insert(TEntity e);

        /// <summary>
        /// Requests the data source to update the record for
        /// the given object. For this, the ID property has
        /// to be set to an ID of an existing record.
        /// If the record doesn't exist, an exception may
        /// be thrown but doesn't have to.
        /// The object is then to be read back again and returned,
        /// providing an updated copy.
        /// </summary>
        /// <param name="e">The updated object to store</param>
        /// <returns></returns>
        TEntity Update(TEntity e);

        /// <summary>
        /// Requests the data source to delete the record with
        /// the given ID. If the record doesn't exist, no
        /// action will be taken and no exception will be thrown.
        /// </summary>
        /// <param name="id">The key of the object to be deleted</param>
        void Delete(TKey id);

        /// <summary>
        /// Requests the data source to delete the given record.
        /// This method will not inspect the object for equality but
        /// will only use the stored primary key to lookup and delete
        /// the record from the data source.
        /// </summary>
        /// <param name="e">The object to be deleted</param>
        /// <seealso cref="Delete(TKey)"/>
        void Delete(TEntity e);
    }
}