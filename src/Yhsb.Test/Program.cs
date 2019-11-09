using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Yhsb.Test
{
    public class TestRunner
    {
        public static void Main(string[] args)
        {
            if (args.Length <= 0)
            {
                Console.WriteLine("dotnet run --project src/Yhsb.Test/ [-l|-a] <method>");
                return;
            }
            var list = false;
            var matchAll = false;
            string pattern = null;

            foreach (var arg in args)
            {
                var a = arg.Trim();
                if (a == "-l") list = true;
                else if (a == "-a") matchAll = true;
                else if (a != "")
                {
                    pattern = a;
                    break;
                }
            }

            Console.WriteLine($"Arguments: [List: {list}, MatchAll: {matchAll}, Pattern: {pattern}]");

            var type = typeof(TestRunner);
            foreach (var t in type.Assembly.GetTypes())
            {
                if (t.IsClass)
                {
                    if (list) Console.WriteLine(t.Name + ":");
                    foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.Static))
                    {
                        if (list) Console.WriteLine("  " + m.Name);
                        if (pattern != null)
                        {
                            try
                            {
                                var match = Regex.Match(t.Name + "." + m.Name, pattern, RegexOptions.IgnoreCase);
                                if (match.Success)
                                {
                                    if (!list)
                                    {
                                        Console.WriteLine(t.Name + ":");
                                        Console.WriteLine("  " + m.Name);
                                    }
                                    Console.WriteLine("".PadLeft(27, '-') + "OUTPUT" + "".PadLeft(27, '-'));
                                    m.Invoke(null, null);
                                    Console.WriteLine("".PadLeft(60, '-'));
                                }
                                if (!matchAll) return;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("".PadLeft(27, '-') + "ERROR" + "".PadLeft(28, '-'));
                                Console.WriteLine($"{ex}");
                                Console.WriteLine("".PadLeft(60, '-'));
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}
