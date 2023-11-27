using System;
using System.Threading.Tasks;

namespace PandaDotNet.Utils
{
    /// <summary>
    /// An unordered set of various utilities
    /// </summary>
    public static class UtilityExtensions
    {
        /// <summary>
        /// Returns the provided object unless it is null.
        /// If the provided object is null, the alternative
        /// object will be returned.
        /// </summary>
        /// <param name="obj">Object to be tested</param>
        /// <param name="alternative">
        /// The alternative object to be returned in case
        /// <see cref="obj"/> is null.
        /// </param>
        /// <seealso cref="Or{T}(T,Func{T})"/>
        public static T Or<T>(this T obj, T alternative)
        {
            return obj == null ? alternative : obj;
        }

        /// <summary>
        /// Returns the provided object unless it is null.
        /// If the provided object is null, an alternative
        /// object will be constructed using the provided
        /// factory method and returned.
        /// </summary>
        /// <param name="obj">Object to be tested</param>
        /// <param name="alternativeFactory">
        /// A factory function that constructs the alternative
        /// value if <see cref="obj"/> is null.
        /// </param>
        /// <seealso cref="Or{T}(T,T)"/>
        public static T Or<T>(this T obj, Func<T> alternativeFactory)
        {
            return obj == null ? alternativeFactory() : obj;
        }

        /// <summary>
        /// Returns the provided object unless it is considered null.
        /// If the provided object is considered null, an alternative
        /// object will be constructed using the provided factory
        /// method and returned.
        /// </summary>
        /// <param name="obj">Object to be tested</param>
        /// <param name="checkFn">
        /// A function that shall return true if the value
        /// is "considered null".
        /// </param>
        /// <param name="alternativeFactory">
        /// A factory function that constructs the alternative
        /// value if <see cref="obj"/> is null.
        /// </param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Or<T>(this T obj, Func<T, bool> checkFn, Func<T> alternativeFactory)
        {
            return checkFn(obj) ? alternativeFactory() : obj;
        }

        /// <summary>
        /// Returns the object as is unless it is null.
        /// If the object is null, a custom exception will
        /// be thrown which is constructed using a provided
        /// <see cref="exceptionFactory"/>.
        /// </summary>
        /// <param name="obj">The object to be tested</param>
        /// <param name="exceptionFactory">
        /// A factory method for the exception to be thrown
        /// in case <see cref="obj"/> is null. The method provides
        /// no arguments and must return an <see cref="Exception"/>.
        /// </param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static T OrThrow<T>(this T obj, Func<Exception> exceptionFactory)
        {
            if (obj == null)
            {
                throw exceptionFactory();
            }
            return obj;
        }

        /// <summary>
        /// A specific implementation of <see cref="OrThrow{T}"/>
        /// that throws an <see cref="ArgumentNullException"/> if the
        /// provided object is null.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="paramName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T OrThrowNullArg<T>(this T obj, string paramName)
        {
            return obj.OrThrow(() => new ArgumentNullException(paramName));
        }

        /// <summary>
        /// Returns a human-readable file size as a string
        /// </summary>
        /// <param name="fileSize"></param>
        /// <returns></returns>
        public static string ToHumanReadableFileSize(
            this long fileSize)
        {
            // Get absolute value
            long absolute_i = (fileSize < 0 ? -fileSize : fileSize);
            // Determine the suffix and readable value
            string suffix;
            double readable;
            if (absolute_i >= 0x1000000000000000) // Exabyte
            {
                suffix = "EB";
                readable = (fileSize >> 50);
            }
            else if (absolute_i >= 0x4000000000000) // Petabyte
            {
                suffix = "PB";
                readable = (fileSize >> 40);
            }
            else if (absolute_i >= 0x10000000000) // Terabyte
            {
                suffix = "TB";
                readable = (fileSize >> 30);
            }
            else if (absolute_i >= 0x40000000) // Gigabyte
            {
                suffix = "GB";
                readable = (fileSize >> 20);
            }
            else if (absolute_i >= 0x100000) // Megabyte
            {
                suffix = "MB";
                readable = (fileSize >> 10);
            }
            else if (absolute_i >= 0x400) // Kilobyte
            {
                suffix = "KB";
                readable = fileSize;
            }
            else
            {
                return fileSize.ToString("0 B"); // Byte
            }
            // Divide by 1024 to get fractional value
            readable = (readable / 1024);
            // Return formatted number with suffix
            return readable.ToString("0.## ") + suffix;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskFn"></param>
        /// <typeparam name="T"></typeparam>
        public static T RunSync<T>(Func<Task<T>> taskFn)
        {
            T response = default;
            Task task = Task.Factory.StartNew(async () =>
            {
                response = await taskFn();
            }).Unwrap();
            task.Wait();
            return response;
        }
    }
}