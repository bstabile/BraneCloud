using System;

namespace BraneCloud.Evolution.EC.Randomization
{
    public static class DateTimeHelper
    {
        /// <summary>
        /// Used to offset total milliseconds to match what Java uses.
        /// </summary>
        private const long TicksOffset = 621355968000000000;

        /// <summary>
        /// This is useful for Randimization seeds to match what values are used in ECJ (i.e. Java).
        /// It is also used to standardize times in many of the algorithms that need to compare "now" and "then".
        /// </summary>
        public static long CurrentTimeMilliseconds => (DateTime.Now.Ticks - TicksOffset) / 10000;
    }
}
