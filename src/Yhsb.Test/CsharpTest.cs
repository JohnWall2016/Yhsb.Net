using System;
using static System.Console;
using System.Reflection;
using System.Collections.Generic;

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

    public static void TestEntryPoint()
    {
        WriteLine(Assembly.GetEntryAssembly().EntryPoint.Name);
        WriteLine(Assembly.GetEntryAssembly().EntryPoint.ToString());
    }

    public class Coordinate : IEquatable<Coordinate>
    {
        readonly int _x, _y;

        public Coordinate(int x, int y)
        {
            _x = x; _y = y;
        }

        public bool Equals(Coordinate other)
        {
            if (other is null) return false;
            return (_x, _y).Equals((other._x, other._y));
        }

        public override bool Equals(object other)
        {
            return Equals(other as Coordinate);
        }

        public override int GetHashCode()
        {
            return (_x, _y).GetHashCode();
        }

        public static bool operator ==(Coordinate a, Coordinate b)
        {
            if (a is null)
            {
                if (b is null)
                    return true;
                else
                    return false;
            }
            else
            {
                return a.Equals(b);
            }
        }

        public static bool operator !=(Coordinate a, Coordinate b)
        {
            return !(a == b);
        }
    }

    public static void TestEqual()
    {
        var a = new Coordinate(10, 11);
        var b = a;
        var c = new Coordinate(10, 11);
        var d = new Coordinate(11, 11);

        WriteLine(a == b);
        WriteLine(a == c);
        WriteLine(a == d);
    }

    class DisposableData : IDisposable
    {
        public DisposableData()
        {
            WriteLine("creating DisposableData");
        }

        public void Dispose()
        {
            WriteLine("disposing DisposableData");
        }

        public static IEnumerable<int> Fetch()
        {
            using var data = new DisposableData();
            for (var i = 10; i < 20; i++)
                yield return i;
        }
    }

    public static void TestYield()
    {
        foreach (var i in DisposableData.Fetch())
            WriteLine(i);
    }

    public static void TestAnonymousClass()
    {
        var p1 = new { Name = "John", Age = 4 };
        var p2 = new { Name = "Rose", Age = 4 };

        WriteLine(p1);
        WriteLine(p2);
        WriteLine(p1.GetType());
        WriteLine(p2.GetType());
    }
}