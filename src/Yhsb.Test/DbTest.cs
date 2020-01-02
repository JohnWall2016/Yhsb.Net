using System;
using System.Linq;
using System.Collections.Generic;
using Yhsb.Database;
using Yhsb.Jb.Database.Jzfp2020;

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
            where fpData.Idcard == "430311197209271512"
            select fpData;
        if (data.Any())
        {
            var d = data.First();
            Console.WriteLine($"{d.NO} {d.Idcard} {d.Name} {d.Jbrdsf}");
        }
        else
        {
            Console.WriteLine("no data");
        }
    }

    public static void TestDatabaseEx()
    {
        using var db = new FpDbContext();
        Console.WriteLine(db.GetTableName<FpHistoryData>());
        Console.WriteLine(db.GetTableName<FpMonthData>());
        Console.WriteLine(db.GetTableName<Jbrymx>());

        db.DeleteAll<Jbrymx>();
        /*
        db.LoadExcel<Jbrymx>(
            @"D:\精准扶贫\2019\参保人员明细表\居保参保人员明细表20191203A.xlsx",
            2, 101, 
            new List<string> {"D", "A", "B", "C", "E", "F", "H", "J", "K", "N"});
        */
    }
}