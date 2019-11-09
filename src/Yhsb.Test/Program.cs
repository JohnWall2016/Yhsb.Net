using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Yhsb.Test
{

    public class TestRunner
    {
        public static void Main(string[] args)
        {
            if (args.Length < 0) return;
            var list = false;
            string pattern = null;

            foreach (var arg in args)
            {
                var a = arg.Trim();
                if (a == "-l")
                {
                    list = true;
                }
                else if (a != "")
                {
                    pattern = a;
                    break;
                }
            }

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
                            var match = Regex.Match(m.Name, pattern, RegexOptions.IgnoreCase);
                            if (match.Success)
                            {
                                if (!list)
                                {
                                    Console.WriteLine(t.Name + ":");
                                    Console.WriteLine("  " + m.Name);
                                }
                                Console.WriteLine("".PadLeft(60, '-'));
                                m.Invoke(null, null);
                                Console.WriteLine("".PadLeft(60, '-'));
                            }
                        }
                    }
                }
            }
        }
    }
}
