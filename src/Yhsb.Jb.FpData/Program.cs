using CommandLine;
using Yhsb.Jb.Network;
using Yhsb.Jb.Database;
using Yhsb;
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
            Command.Parse<Pkrk, Dcsj>(args);
        }

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

        public static void ExportFpData(string tableName, string tmplXlsx, string saveXlsx)
        {
            using var db = new FpDbContextWithFpData(tableName);

            WriteLine($"开始导出扶贫底册: {tableName}=>{saveXlsx}");

            var workbook = ExcelExtension.LoadExcel(tmplXlsx);
            var sheet = workbook.GetSheetAt(0);
            int startRow = 2, currentRow = 2;

            var data = from e in db.Entity select e;
            foreach (var d in data)
            {
                var index = currentRow - startRow + 1;

                WriteLine($"{index} {d.IDCard} {d.Name}");

                var row = sheet.GetOrCopyRow(currentRow++, startRow);

                row.Cell("A").SetValue(index);
                row.Cell("B").SetValue(d.NO);
                row.Cell("C").SetValue(d.Xzj);
                row.Cell("D").SetValue(d.Csq);
                row.Cell("E").SetValue(d.Address);
                row.Cell("F").SetValue(d.Name);
                row.Cell("G").SetValue(d.IDCard);
                row.Cell("H").SetValue(d.BirthDay);
                row.Cell("I").SetValue(d.Pkrk);
                row.Cell("J").SetValue(d.PkrkDate);
                row.Cell("K").SetValue(d.Tkry);
                row.Cell("L").SetValue(d.TkryDate);
                row.Cell("M").SetValue(d.Qedb);
                row.Cell("N").SetValue(d.QedbDate);
                row.Cell("O").SetValue(d.Cedb);
                row.Cell("P").SetValue(d.CedbDate);
                row.Cell("Q").SetValue(d.Yejc);
                row.Cell("R").SetValue(d.YejcDate);
                row.Cell("S").SetValue(d.Ssjc);
                row.Cell("T").SetValue(d.SsjcDate);
                row.Cell("U").SetValue(d.Sypkry);
                row.Cell("V").SetValue(d.Jbrdsf);
                row.Cell("W").SetValue(d.JbrdsfFirstDate);
                row.Cell("X").SetValue(d.JbrdsfLastDate);
                row.Cell("Y").SetValue(d.Jbcbqk);
                row.Cell("Z").SetValue(d.JbcbqkDate);
            }

            workbook.Save(saveXlsx);

            WriteLine($"结束导出扶贫底册: {tableName}=>{saveXlsx}");
        }
    }

    [Verb("pkrk", HelpText = "导入贫困人口数据")]
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

    [Verb("dcsj", HelpText = "导出扶贫底册数据")]
    class Dcsj : ICommand
    {
        [Value(0, HelpText = "表名称，例如：2019年度扶贫历史数据底册, 201905扶贫数据底册",
            Required = true, MetaName = "tabeName")]
        public string TableName { get; set; }

        public void Execute()
        {
            Program.ExportFpData(
                TableName, 
                @"D:\精准扶贫\雨湖区精准扶贫底册模板.xlsx",
                $@"D:\精准扶贫\{TableName}{Util.DateTime.FormatedDate()}.xlsx");
        }
    }
}
