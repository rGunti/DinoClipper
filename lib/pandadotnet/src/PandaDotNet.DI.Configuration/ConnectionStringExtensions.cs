using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace PandaDotNet.DI.Configuration
{
    /// <summary>
    /// A set of extension methods for handling connection strings
    /// </summary>
    public static class ConnectionStringExtensions
    {
        private static readonly Regex SchemeRegex =
            new Regex("^(([^:\\/]*):\\/\\/)?(.*)$", RegexOptions.Compiled);

        /// <summary>
        /// Returns a connection string (like the standard .GetConnectionString() method,
        /// but optionally strips the scheme from it.
        /// This requires that the connection string starts with a URI scheme (i.e. file://, mongodb://, ...).
        /// </summary>
        /// <seealso cref="GetScheme"/>
        /// <seealso cref="StripScheme"/>
        /// <param name="config"></param>
        /// <param name="connectionStringName"></param>
        /// <param name="stripScheme"></param>
        /// <returns></returns>
        public static string GetConnectionString(
            this IConfiguration config,
            string connectionStringName,
            bool stripScheme)
        {
            string cs = config.GetConnectionString(connectionStringName);
            return stripScheme ? cs.StripScheme() : cs;
        }

        /// <summary>
        /// Returns the scheme of the provided URI.
        /// If no scheme can be detected in the URI, an empty string
        /// will be returned.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static string GetScheme(this string uri)
        {
            Match protoMatch = SchemeRegex.Match(uri);
            return protoMatch.Groups[2].Value;
        }

        /// <summary>
        /// Strips the scheme from a URI.
        /// If no scheme is detected, the input and output string
        /// are the same.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static string StripScheme(this string uri)
        {
            Match result = SchemeRegex.Match(uri);
            return result.Groups[3].Value;
        }
    }
}