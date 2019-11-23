using System;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Linq;

using static System.Console;

namespace Yhsb.Test
{
    public class TestRunner
    {
        public static void Main(string[] args)
        {
            if (args.Length <= 0)
            {
                WriteLine(
                    "dotnet run --project src/Yhsb.Test/ [-l|-a] <method>");
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
                else if (a == "-la" || a == "-al")
                {
                    list = true;
                    matchAll = true;
                }
                else if (a != "")
                {
                    pattern = a;
                    break;
                }
            }

            WriteLine(
                $"Arguments: [List: {list}, MatchAll: {matchAll}, Pattern: {pattern}]");

            var type = typeof(TestRunner);
            foreach (var t in type.Assembly.GetTypes())
            {
                if (t.IsClass)
                {
                    string ns = t.Namespace != null ? t.Namespace + "." : "";
                    if (list) WriteLine(ns + t.Name + ":");
                    foreach (var m in t.GetMethods(
                        BindingFlags.Public | BindingFlags.Static))
                    {
                        if (list) WriteLine("  " + m.Name);
                        if (pattern != null)
                        {
                            try
                            {
                                var match = Regex.Match(
                                    ns + t.Name + "." + m.Name, pattern, 
                                    RegexOptions.IgnoreCase);
                                if (match.Success)
                                {
                                    if (!list)
                                    {
                                        WriteLine(ns + t.Name + ":");
                                        WriteLine("  " + m.Name);
                                    }
                                    WriteLine(
                                        "".PadLeft(27, '-') + "OUTPUT" + "".PadLeft(27, '-'));
                                    var parameters = m.GetParameters().Select(info =>
                                        info.DefaultValue);
                                    m.Invoke(null, parameters.ToArray());
                                    WriteLine("".PadLeft(60, '-'));
                                    
                                    if (!matchAll) return;
                                }
                            }
                            catch (Exception ex)
                            {
                                WriteLine(
                                    "".PadLeft(27, '-') + "ERROR" + "".PadLeft(28, '-'));
                                WriteLine($"{ex}");
                                WriteLine("".PadLeft(60, '-'));
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}
