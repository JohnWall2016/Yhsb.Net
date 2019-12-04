using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace Yhsb.Util
{
    public static class StreamExtension
    {
        public static void Write(this Stream stream, byte[] buffer)
        {
            stream.Write(buffer, 0, buffer.Length);
        }
    }

    public static class PrintExtension
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

    public static class EnumExtension
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

    public static class EnumerateExtension
    {
        public static string ToLiteral<T>(this IEnumerable<T> enumerator)
        {
            var list = enumerator.Select(t => t.ToString());
            return "[" + string.Join(',', list) + "]";
        }
    }

}