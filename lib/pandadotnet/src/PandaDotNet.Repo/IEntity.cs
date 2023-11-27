namespace PandaDotNet.Repo
{
    /// <summary>
    /// <para>
    /// An entity is an object to be stored, managed and queried.
    /// It usually represents a table in a relational database or
    /// a collection in a document-based database and required a
    /// primary key of any kind.
    /// </para>
    /// <para>
    /// Note that an object implementing this interface does not
    /// necessarily have to be stored one-for-one in a data source.
    /// It may be converted into another kind of object before storage
    /// making it independent of storage medium. The only requisite is
    /// that the data remains accessible through the use of the
    /// <see cref="Id"/> property.
    /// </para>
    /// </summary>
    /// <typeparam name="TKey">The type of the primary key</typeparam>
    public interface IEntity<TKey>
    {
        /// <summary>
        /// The primary key of the object
        /// </summary>
        TKey Id { get; set; }
    }
}