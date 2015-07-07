using System;

namespace WendySharp
{
    static class DateTimeParser
    {
        /// <exception cref="ArgumentException">Invalid input.</exception>
        public static DateTime Parse(string input, string inputUnit)
        {
            int i;

            if (!int.TryParse(input, out i) || i <= 0)
            {
                throw new ArgumentException("Duration can not be zero.");
            }

            var time = DateTime.UtcNow;

            switch (inputUnit[0])
            {
                case 'h':
                    time = time.AddHours(i);
                    break;

                case 'w':
                    time = time.AddDays(i * 7);
                    break;

                case 'd':
                    time = time.AddDays(i);
                    break;

                case 'm':
                    if (inputUnit.Length < 2)
                    {
                        throw new ArgumentException("Ambiguous unit. Specify if it's [mi]nutes or [mo]nths.");
                    }

                    switch (inputUnit[1])
                    {
                        case 'o':
                            time = time.AddMonths(i);
                            break;

                        case 'i':
                            time = time.AddMinutes(i);
                            break;

                        default:
                            throw new ArgumentException("Invalid unit.");
                    }

                    break;

                default:
                    throw new ArgumentException("Invalid unit.");
            }

            return time;
        }
    }
}
