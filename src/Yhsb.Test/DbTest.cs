using System;
using Yhsb.Jb.Database;
using System.Linq;

public class DbTest
{
    public static void TestFPTable()
    {
        using var context = new FpDbContext();
        var data = from fpData in context.FpData2019
                   where fpData.IDCard == "430311194610131520"
                   select fpData;
        if (data.Any())
        {
            var info = data.First();
            Console.WriteLine($"{info.IDCard} {info.Name} {info.Jbrdsf}");
        }
        
        var rdata = from fpData in context.FpRawData2019
                   where fpData.IDCard == "430311194610131520"
                   select fpData;
        if (rdata.Any())
        {
            var info = rdata.First();
            Console.WriteLine($"{info.IDCard} {info.Name} {info.Type}");
        }
    }
}