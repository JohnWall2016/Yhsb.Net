using System;
using static System.Console;

class CsharpTest
{
    public static void TestString()
    {
        WriteLine(
                "序号".PadLeft(2) +
                "年度".PadLeft(3) +
                "个人缴费".PadLeft(6) +
                "省级补贴".PadLeft(5) +
                "市级补贴".PadLeft(5) +
                "县级补贴".PadLeft(5) +
                "政府代缴".PadLeft(5) +
                "集体补助".PadLeft(5) +
                "  社保经办机构 划拨时间");

        WriteLine(
            $"{"序号",2}{"年度",3}{"个人缴费",6}{"省级补贴",5}" +
            $"{"市级补贴",5}{"县级补贴",5}{"政府代缴",5}{"集体补助",5}" +
            "  社保经办机构 划拨时间");
    }

    public static void TestSwitch()
    {
        static void f(string s)
        {
            switch (s)
            {
                case "1":
                    WriteLine("One");
                    goto default;
                case "2":
                    WriteLine("Two");
                    goto case "1";

                default:
                    WriteLine("Def");
                    break;
            }
        }
        f("1");
        f("2");
    }
}