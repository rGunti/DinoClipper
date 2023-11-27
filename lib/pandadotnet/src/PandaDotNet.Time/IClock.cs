using System;

namespace PandaDotNet.Time
{
    /// <summary>
    /// The Clock interface provides an abstraction for retrieving the current time
    /// </summary>
    public interface IClock
    {
        /// <summary>
        /// Returns the current date and time
        /// </summary>
        /// <returns></returns>
        DateTime GetCurrentDateTime();

        /// <summary>
        /// Returns the current date and time in UTC
        /// </summary>
        /// <returns></returns>
        DateTime GetCurrentDateTimeUtc();
    }
}