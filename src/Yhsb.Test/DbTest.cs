using System;
using Yhsb.Jb.Database;
using System.Linq;

public class DbTest
{
    public static void TestFPTable()
    {
        using var context = new JzfpContext();
        var data = from fpData in context.FPTable2019
                   where fpData.IDCard == "430311194610131520"
                   select fpData;
        if (data.Any())
        {
            var info = data.First();
            Console.WriteLine($"{info.IDCard} {info.Name} {info.JBClass}");
        }
    }
}