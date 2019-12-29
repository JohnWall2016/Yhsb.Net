using System;
using System.Linq;

public class DbTest
{
    public static void TestFPTable2019()
    {
        using var context = new Yhsb.Jb.Database.Jzfp2019.FpDbContext();
        var data = from fpData in context.FpData2019
                   where fpData.Idcard == "430311194610131520"
                   select fpData;
        if (data.Any())
        {
            var info = data.First();
            Console.WriteLine($"{info.Idcard} {info.Name} {info.Jbrdsf}");
        }
        
        var rdata = from fpData in context.FpRawData2019
                   where fpData.Idcard == "430311194610131520"
                   select fpData;
        if (rdata.Any())
        {
            var info = rdata.First();
            Console.WriteLine($"{info.Idcard} {info.Name} {info.Type}");
        }
    }

    public static void TestFPTable2020()
    {
        using var context = new Yhsb.Jb.Database.Jzfp2020.FpDbContext();
        var data = from fpData in context.FpHistoryData
            where fpData.Idcard == "1221323554333"
            select fpData;
        if (data.Any())
        {
            Console.WriteLine(data.First());
        }
        else
        {
            Console.WriteLine("no data");
        }
    }
}