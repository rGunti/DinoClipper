using System;

namespace PandaDotNet.Time
{
    /// <summary>
    /// This is the default implementation of <see cref="IClock"/> which provides the current
    /// date and time from <see cref="DateTime.Now"/>.
    /// </summary>
    public class Clock : IClock
    {
        /// <inheritdoc />
        public DateTime GetCurrentDateTime()
        {
            return DateTime.Now;
        }

        /// <inheritdoc />
        public DateTime GetCurrentDateTimeUtc()
        {
            return DateTime.UtcNow;
        }
    }
}