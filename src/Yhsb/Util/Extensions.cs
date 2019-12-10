using System;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace Yhsb.Util
{
    public static class StreamEx
    {
        public static void Write(this Stream stream, byte[] buffer)
        {
            stream.Write(buffer, 0, buffer.Length);
        }
    }

    public static class PrintEx
    {
        public static void Print<T>(this T obj)
        {
            Console.WriteLine($"{obj}");
        }
    }

    public sealed class DescriptionAttribute : Attribute
    {
        public string Description { get; }

        public DescriptionAttribute(string description) =>
            Description = description;
    }

    public static class EnumEx
    {
        public static string GetDescription(this Enum This)
        {
            Type type = This.GetType();

            string name = Enum.GetName(type, This);

            MemberInfo member = type.GetMembers()
                .Where(w => w.Name == name)
                .FirstOrDefault();

            DescriptionAttribute attribute = member != null
                ? member.GetCustomAttributes(true)
                    .Where(w => w.GetType() == typeof(DescriptionAttribute))
                    .FirstOrDefault() as DescriptionAttribute
                : null;

            return attribute != null ? attribute.Description : name;
        }
    }

    public static class EnumerateEx
    {
        public static string ToLiteral<T>(this IEnumerable<T> enumerator)
        {
            var list = enumerator.Select(t => t.ToString());
            return "[" + string.Join(',', list) + "]";
        }
    }

    public static class StringEx
    {
        public static string AppendToFileName(string fileName, string appendString)
        {
            var index = fileName.LastIndexOf(".");
            if (index >= 0)
                return fileName.Substring(0, index) +
                    appendString + fileName.Substring(index);
            else
                return fileName + appendString;
        }

        static readonly List<string> bigN = new List<string>
        {
            "零", "壹", "贰", "叁", "肆",
            "伍", "陆", "柒", "捌", "玖",
        };

        static readonly List<string> places = new List<string>
        {
            "", "拾", "佰", "仟", "万", "亿",
        };

        static readonly List<string> units = new List<string>
        {
            "元", "角", "分",
        };

        const string whole = "整";

        public static string ConvertToChineseMoney(decimal money)
        {
            var n = new BigInteger(Math.Round(money, 2) * 100);
            var integer = n / 100;
            var fraction = n % 100;

            int length = integer.ToString().Length;
            var ret = "";
            var zero = false;
            for (var i = length; i >= 0; i--)
            {
                var bas = (BigInteger)Math.Pow(10, i);
                if (integer / bas > 0)
                {
                    if (zero) ret += bigN[0];
                    ret += bigN[(int)(integer / bas)] + places[i % 4];
                    zero = false;
                }
                else if (integer / bas == 0 && ret != "")
                {
                    zero = true;
                }
                if (i >= 4)
                {
                    if (i % 8 == 0 && ret != "")
                        ret += places[5];
                    else if (i % 4 == 0 && ret != "")
                        ret += places[4];
                }
                integer %= bas;
                if (integer == 0 && i != 0)
                {
                    zero = true;
                    break;
                }
            }
            ret += units[0];

            if (fraction == 0) // .00
            {
                ret += whole;
            }
            else if (fraction % 10 == 0) // .D0
            {
                if (zero) ret += bigN[0];
                ret += bigN[(int)(fraction / 10)] + units[1] + whole;
            }
            else
            {
                if (zero || fraction / 10 == 0) // .0D or .DD
                    ret += bigN[0];
                if (fraction / 10 != 0) // .0D
                    ret += bigN[(int)(fraction / 10)] + units[1];
                ret += bigN[(int)(fraction % 10)] + units[2];
            }
            return ret;
        }
    }
}