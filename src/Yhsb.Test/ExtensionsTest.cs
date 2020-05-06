using Yhsb.Util;
using static System.Console;

public class ExtensionsTest
{
    public static void TestFill()
    {
        WriteLine("我们abc".FillLeft(8));
        WriteLine("ABCabc".FillLeft(8));
        WriteLine("我们abc".FillRight(8));
        WriteLine("ABCabc".FillRight(8));
    }
}