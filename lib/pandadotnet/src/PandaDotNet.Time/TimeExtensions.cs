using System;

namespace PandaDotNet.Time
{
    /// <summary>
    /// A set of extension methods to get TimeSpans
    /// </summary>
    public static class TimeExtensions
    {
        public static TimeSpan Millisecond(this int ms) => TimeSpan.FromMilliseconds(ms);
        public static TimeSpan Millisecond(this double ms) => TimeSpan.FromMilliseconds(ms);
        public static TimeSpan Milliseconds(this int ms) => TimeSpan.FromMilliseconds(ms);
        public static TimeSpan Milliseconds(this double ms) => TimeSpan.FromMilliseconds(ms);
        
        public static TimeSpan Second(this int sec) => TimeSpan.FromSeconds(sec);
        public static TimeSpan Second(this double sec) => TimeSpan.FromSeconds(sec);
        public static TimeSpan Seconds(this int sec) => TimeSpan.FromSeconds(sec);
        public static TimeSpan Seconds(this double sec) => TimeSpan.FromSeconds(sec);

        public static TimeSpan Minute(this int min) => TimeSpan.FromMinutes(min);
        public static TimeSpan Minute(this double min) => TimeSpan.FromMinutes(min);
        public static TimeSpan Minutes(this int min) => TimeSpan.FromMinutes(min);
        public static TimeSpan Minutes(this double min) => TimeSpan.FromMinutes(min);

        public static TimeSpan Hour(this int hrs) => TimeSpan.FromHours(hrs);
        public static TimeSpan Hour(this double hrs) => TimeSpan.FromHours(hrs);
        public static TimeSpan Hours(this int hrs) => TimeSpan.FromHours(hrs);
        public static TimeSpan Hours(this double hrs) => TimeSpan.FromHours(hrs);

        public static TimeSpan Day(this int days) => TimeSpan.FromDays(days);
        public static TimeSpan Day(this double days) => TimeSpan.FromDays(days);
        public static TimeSpan Days(this int days) => TimeSpan.FromDays(days);
        public static TimeSpan Days(this double days) => TimeSpan.FromDays(days);
    }
}