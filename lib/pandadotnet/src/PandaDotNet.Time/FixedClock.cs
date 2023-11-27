using System;

namespace PandaDotNet.Time
{
    /// <summary>
    /// An implementation of <see cref="IClock"/> that has a fixed time set to it.
    /// Adjust the current time using the <see cref="CurrentDateTime"/> property.
    /// This implementation is designed for unit testing.
    /// </summary>
    public class FixedClock : IClock
    {
        /// <summary>
        /// Gets or sets the time this clock is set to.
        /// </summary>
        public DateTime CurrentDateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// Updates the current time forward by the given <see cref="TimeSpan"/>
        /// </summary>
        /// <param name="timeSpan"></param>
        public void AdvanceTimeBy(TimeSpan timeSpan)
        {
            CurrentDateTime += timeSpan;
        }

        /// <summary>
        /// Rolls back the current time by the given <see cref="TimeSpan"/>
        /// </summary>
        /// <param name="timeSpan"></param>
        public void RollBackTimeBy(TimeSpan timeSpan)
        {
            CurrentDateTime -= timeSpan;
        }

        /// <inheritdoc />
        public DateTime GetCurrentDateTime()
        {
            return CurrentDateTime;
        }

        /// <inheritdoc />
        public DateTime GetCurrentDateTimeUtc()
        {
            return CurrentDateTime.ToUniversalTime();
        }
    }
}