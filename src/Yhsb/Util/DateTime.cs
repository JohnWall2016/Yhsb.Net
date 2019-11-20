using System;
using System.Text.RegularExpressions;

namespace Yhsb.Util
{
    public class DateTime
    {
        public static string ConvertToDashedDay(
            string date, string format = @"^(\d\d\d\d)(\d\d)(\d\d)$")
        {
            var match = Regex.Match(date, format);
            if (match.Success)
            {
                return match.Groups[1].Value + "-" +
                    match.Groups[2].Value + "-" +
                    match.Groups[3].Value;
            }
            else
            {
                throw new ArgumentException("Invalid date format (YYYYMMDD).");
            }
        }
    }
}