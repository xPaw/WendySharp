using System;

namespace WendySharp
{
    public static class DateTimeExensions
    {
        /// <summary>
        /// Returns a string representation of this date/time. If the
        /// value is close to now, a relative description is returned.
        /// </summary>
        public static string ToRelativeString(this DateTime dt)
        {
            var span = DateTime.UtcNow - dt.ToUniversalTime();

            // Normalize time span
            var future = false;
            if (span.TotalSeconds < 0)
            {
                // In the future
                span = -span;
                future = true;
            }

            // Test for Now
            var totalSeconds = span.TotalSeconds;
            if (totalSeconds < 0.9)
            {
                return "now";
            }

            // Date/time near current date/time
            var format = future ? "in {0} {1}" : "{0} {1} ago";
            if (totalSeconds < 55)
            {
                // Seconds
                var seconds = Math.Max(1, span.Seconds);
                return string.Format(format, seconds,
                    seconds == 1 ? "second" : "seconds");
            }

            if (totalSeconds < 55 * 60)
            {
                // Minutes
                var minutes = Math.Max(1, span.Minutes);
                return string.Format(format, minutes,
                    minutes == 1 ? "minute" : "minutes");
            }
            if (totalSeconds < 24 * 60 * 60)
            {
                // Hours
                var hours = Math.Max(1, span.Hours);
                return string.Format(format, hours,
                    hours == 1 ? "hour" : "hours");
            }

            // Format both date and time
            if (totalSeconds < 48 * 60 * 60)
            {
                // 1 Day
                format = future ? "tomorrow" : "yesterday";
            }
            else if (totalSeconds < 3 * 24 * 60 * 60)
            {
                // 2 Days
                format = string.Format(format, 2, "days");
            }
            else if (dt.Year == DateTime.Now.Year)
            {
                // Absolute date
                format = dt.ToString(@"\o\n MMM d");
            }
            else
            {
                format = dt.ToString(@"\o\n MMM d, yyyy");
            }

            // Add time
            return string.Format("{0} at {1:HH:mm}", format, dt);
        }
    }
}
