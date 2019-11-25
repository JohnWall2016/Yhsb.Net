using System;
using Yhsb.Jb.Database;
using System.Linq;

public class DbTest
{
    public static void TestFPTable()
    {
        using var context = new FpDataContext("2019年度扶贫历史数据底册");
        var data = from fpData in context.Entity
                   where fpData.IDCard == "430311194610131520"
                   select fpData;
        if (data.Any())
        {
            var info = data.First();
            Console.WriteLine($"{info.IDCard} {info.Name} {info.Jbrdsf}");
        }
    }
}