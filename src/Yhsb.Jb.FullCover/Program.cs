using CommandLine;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

using Yhsb.Database;
using Yhsb.Jb.Database.FullCover2020;
using Yhsb.Util.Command;
using Yhsb.Util.Excel;

using Microsoft.EntityFrameworkCore;

using static System.Console;

namespace Yhsb.Jb.FullCover
{
    class Program
    {
        [App(Name = "全覆盖处理程序")]
        static void Main(string[] args)
        {
            Command.Parse<Split, ImportDist>(args);
        }
    }

    [Verb("split", HelpText = "数据分发程序")]
    class Split : ICommand
    {
        [Value(0, HelpText = "源数据表路径",
            Required = true, MetaName = "SourceExcel")]
        public string SourceExcel { get; set; }

        [Value(1, HelpText = "开始行，从1开始",
            Required = true)]
        public int BeginRow { get; set; }

        [Value(2, HelpText = "结束行(包含)，从1开始",
            Required = true)]
        public int EndRow { get; set; }
        
        [Value(3, HelpText = "分组模板表路径",
            Required = true, MetaName = "TemplateExcel")]
        public string TemplateExcel { get; set; }

        [Value(4, HelpText = "输出目录",
            Required = true, MetaName = "OutDir")]
        public string OutDir { get; set; }

        public void Execute()
        {
            var workbook = ExcelExtension.LoadExcel(SourceExcel);
            var sheet = workbook.GetSheetAt(0);

            WriteLine("生成分组映射表");
            var map = new Dictionary<string, List<int>>();
            for (var index = BeginRow - 1; index < EndRow; index++)
            {
                var xzj = sheet.Row(index).Cell("F").Value();
                if (!map.ContainsKey(xzj))
                    map[xzj] = new List<int>();
                map[xzj].Add(index);
            }

            WriteLine("生成分组数据表");
            foreach (var xzj in map.Keys)
            {
                var count = map[xzj].Count;
                WriteLine($"{xzj}: {count}");

                if (count <= 0) continue;

                var outWorkbook = ExcelExtension.LoadExcel(TemplateExcel);
                var outSheet = outWorkbook.GetSheetAt(0);

                int startRow = 1, currentRow = startRow;
                (int Begin, int End) cols = (1, 7);

                map[xzj].ForEach(rowIndex => {
                    var index = currentRow - startRow + 1;
                    var inRow = sheet.Row(rowIndex);
                    var outRow = outSheet.GetOrCopyRow(currentRow++, startRow);
                    outRow.Cell("A").SetValue(index);
                    for (var col = cols.Begin; col <= cols.End; col++) {
                        outRow.Cell(col).SetValue(inRow.Cell(col).Value());
                    }
                });
                outWorkbook.Save(
                    Path.Join(OutDir, $"{xzj}{map[xzj].Count}.xls"));
                //break;
            }
        }
    }

    [Verb("importDist", HelpText = "导入已下发程序")]
    class ImportDist : ICommand
    {
        [Value(0, HelpText = "导入目录",
            Required = true, MetaName = "inputDir")]
        public string InputDir { get; set; }

        [Value(1, HelpText = "下发批次, 例如: 第一批, ...",
            Required = true, MetaName = "distNO")]
        public string DistNO { get; set; }

        [Option("clearAll", HelpText = "是否清除所有该批次数据")]
        public bool ClearAll { get; set; } = false;

        [Option("clear", HelpText = "是否清除导入单位该批次数据")]
        public bool Clear { get; set; } = false;

        public void Execute()
        {
            using var db = new Context();
            if (ClearAll)
            {
                WriteLine($"开始清除下发数据表: {DistNO}");
                db.Delete<Yxfsj>(where: $"xfpc = '{DistNO}'", printSql: true);
            }

            WriteLine($"开始导入已下发数据表: {DistNO}, {InputDir}");

            foreach (var file in Directory.GetFiles(InputDir))
            {
                ImportExcel(db, file);
            }
        }

        public void ImportExcel(DbContext db, string excel)
        {
            var ident = "  ";
            var fileName = Path.GetFileName(excel);
            WriteLine($"{ident}导入数据表: {fileName}");

            var m = Regex.Match(fileName, @"^(.*?)(\d+)\.xls[x]?$", RegexOptions.IgnoreCase);
            ident += ident;
            if (!m.Success)
            {
                WriteLine($"{ident}无法获得数据条数: {fileName}, {excel}");
            }
            else
            {
                var dwmc = m.Groups[1].Value;
                var count = m.Groups[2].Value;

                if (Clear)
                {
                    WriteLine($"{ident}开始清除下发批次: {DistNO}");
                    db.Delete<Yxfsj>(where: $"dwmc = '{dwmc}' and xfpc = '{DistNO}'", printSql: true, ident);
                }
                
                WriteLine($"{ident}导入 {dwmc}: {count}");
                var beginRow = 2;
                var endRow = int.Parse(count) + 1;

                db.LoadExcel<Yxfsj>(
                    excel, beginRow, endRow,
                    new List<string> { dwmc, DistNO, "A", "B", "C", "D", "E", "F", "G", "", "", "", "" },
                    new List<string> { "A" },
                    true, ident);
            }
        }
    }
}
