using Yhsb.Util;
using static System.Console;

public class ExtensionsTest
{
    public static void TestPad()
    {
        WriteLine("我们abc".PackLeft(8));
        WriteLine("ABCabc".PackLeft(8));
        WriteLine("我们abc".PackRight(8));
        WriteLine("ABCabc".PackRight(8));
    }
}