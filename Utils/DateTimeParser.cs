using System;
using Chronic;

namespace WendySharp
{
    static class DateTimeParser
    {
        public static DateTime Parse(string input)
        {
            var options = new Options
            {
                EndianPrecedence = EndianPrecedence.Little,
                FirstDayOfWeek = DayOfWeek.Monday,
                Clock = () => DateTime.UtcNow,
            };
            var parser = new Parser(options);
            var span = parser.Parse(input);

            if (span == null)
            {
                throw new Exception("Failed to parse specified duration.");
            }

            return span.ToTime();
        }
    }
}
