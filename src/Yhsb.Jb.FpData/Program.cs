using CommandLine;
using Yhsb.Jb.Network;
using Yhsb.Jb.Database;
using Yhsb.Util;
using Yhsb.Util.Excel;
using Yhsb.Util.Command;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using static System.Console;

namespace Yhsb.Jb.FpData
{
    class Program
    {
        [App(Name = "扶贫数据导库比对程序")]
        static void Main(string[] args)
        {
            Command.Parse<Pkrk>(args);
        }
    }

    class Util
    {
        public static void ImportFpHistoryData(IEnumerable<FpRawData> records)
        {
            var index = 1;
            using var context = new FpDbContext();
            foreach (var record in records)
            {
                WriteLine($"{index++} {record.IDCard} ${record.Name} ${record.Type}");
                if (!string.IsNullOrEmpty(record.IDCard))
                {
                    var fpData = from e in context.FpRawData2019
                                 where e.IDCard == record.IDCard &&
                                 e.Type == record.Type &&
                                 e.Date == record.Date
                                 select e;
                    if (fpData.Any())
                        context.Update(record);
                    context.Add(record);
                    context.SaveChanges();
                }
            }
        }
    }

    class Pkrk : ICommand
    {
        [Value(0, HelpText = "数据月份, 例如: 201912",
            Required = true, MetaName = "date")]
        public string Date { get; set; }

        [Value(1, HelpText = "xlsx文件",
            Required = true, MetaName = "xslx")]
        public string Xlsx { get; set; }

        [Value(2, HelpText = "数据开始行, 从1开始",
            Required = true, MetaName = "beginRow")]
        public int BeginRow { get; set; }

        [Value(3, HelpText = "数据结束行(包含), 从1开始",
            Required = true, MetaName = "endRow")]
        public int EndRow { get; set; }

        public void Execute()
        {
            
        }
    }
}
