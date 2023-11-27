using System.IO;
using System.IO.Abstractions;

namespace PandaDotNet.Utils
{
    /// <summary>
    /// A set of extensions for File System interactions.
    /// All extensions use the System.IO.Abstractions package to
    /// abstract away the file system interaction. If not provided,
    /// the default implementation will be used, which interacts
    /// with your real file system.
    /// </summary>
    public static class FileSystemExtensions
    {
        /// <summary>
        /// Checks if the given directory exists on the file system.
        /// If it doesn't exist, it will be created.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fs"></param>
        /// <returns>true, if the directory has been created, false otherwise</returns>
        /// <seealso cref="FileSystemExtensions"/>
        public static bool EnsureDirectoryExists(
            this string path,
            IFileSystem fs = null)
        {
            path.OrThrowNullArg(nameof(path));
            fs = fs.Or(() => new FileSystem());
            if (!fs.Directory.Exists(path))
            {
                fs.Directory.CreateDirectory(path);
                return true;
            }

            return false;
        }

        /// <summary>
        /// <para>
        /// Checks that all directories exists for the given
        /// file path. If any directory doesn't exist, it will
        /// be created.
        /// </para>
        /// <para>
        /// The file itself will not be created.
        /// </para>
        /// <para>
        /// This extension calls <see cref="EnsureDirectoryExists"/>
        /// internally.
        /// </para>
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fs"></param>
        /// <returns>true, if the directory has been created, false otherwise</returns>
        /// <seealso cref="FileSystemExtensions"/>
        public static bool EnsureDirectoryExistsForFile(
            this string path,
            IFileSystem fs = null)
        {
            path.OrThrowNullArg(nameof(path));
            return Path.GetDirectoryName(path)
                .EnsureDirectoryExists(fs);
        }
    }
}