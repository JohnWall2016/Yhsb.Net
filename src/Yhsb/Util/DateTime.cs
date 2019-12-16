using System;
using System.Text.RegularExpressions;

namespace Yhsb.Util
{
    public class DateTime
    {
        public static string ConvertToDashedDate(
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

        public static (string year, string month, string day) SplitDate(string date)
        {
            var match = Regex.Match(date,  @"^(\d\d\d\d)(\d\d)(\d\d)?$");
            if (match.Success)
            {
                return (match.Groups[1].Value,
                        match.Groups[2].Value,
                        match.Groups[3].Value);
            }
            else
            {
                throw new ArgumentException("Invalid date format (YYYYMMDD).");
            }
        }

        public static string FormatedDate() => System.DateTime.Now.ToString("yyyyMMdd");
    }
}