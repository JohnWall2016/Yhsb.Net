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

    public static void TestParameters(string s = "abc")
    {
        WriteLine(s);
        s = "efg";
        WriteLine(s);
    }

    struct Angle
    {
        public int degrees;
        public int minutes;
        public int seconds;

        public override string ToString() => 
            $"Angle: {degrees}, {minutes}, {seconds}";
    }

    public static void TestStruct()
    {
        var angle = new Angle 
        {
            degrees = 1,
            minutes = 2,
            seconds = 3,
        };

        object obj = angle; // Box: copy angle's value to obj's.
        Angle angle2 = (Angle)obj; // Unbox: copy obj's value to angle2's.
        angle2.degrees = 4;
        WriteLine(angle);
        WriteLine(angle2);

        WriteLine(typeof(Angle).BaseType);
        WriteLine(typeof(int).BaseType);
        WriteLine(typeof(ValueType).BaseType);
    }
}