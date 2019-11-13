using System;
using Yhsb.Util;

public class UtilTest
{
    public enum Test
    {
        [Description("Abc description")]
        Abc,
        Efg,
    }

    public static void TestEnum()
    {
        var t = Test.Abc;
        Console.WriteLine(t.GetDescription());
        t = Test.Efg;
        Console.WriteLine(t.GetDescription());
    }
}