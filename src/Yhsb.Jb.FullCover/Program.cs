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
            Command.Parse<Split, ImportDist, ImportBooks, ImportJB, 
                UpdateJB, UpdateYY, exportDC>(args);
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

        [Value(5, HelpText = "用于分组的数据列",
            Required = true)]
        public string DistCol { get; set; }

        [Value(6, HelpText = "用于分组的匹配语句")]
        public string DistPattern { get; set; } = ".*";
        
        public void Execute()
        {
            var workbook = ExcelExtension.LoadExcel(SourceExcel);
            var sheet = workbook.GetSheetAt(0);

            WriteLine("生成分组映射表");
            var map = new Dictionary<string, List<int>>();
            for (var index = BeginRow - 1; index < EndRow; index++)
            {
                var xzj = sheet.Row(index).Cell(DistCol).Value();
                if (Regex.IsMatch(xzj, DistPattern))
                {
                    if (!map.ContainsKey(xzj))
                        map[xzj] = new List<int>();
                    map[xzj].Add(index);
                }
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

    [Verb("importBooks", HelpText = "导入落实总台账")]
    class ImportBooks : ICommand
    {
        [Value(0, HelpText = "导入目录",
            Required = true, MetaName = "inputDir")]
        public string InputDir { get; set; }

        [Option("clearAll", HelpText = "是否清除所有数据")]
        public bool ClearAll { get; set; } = false;

        [Option("clear", HelpText = "是否清除导入单位数据")]
        public bool Clear { get; set; } = false;

        public void Execute()
        {
            using var db = new Context();
            if (ClearAll)
            {
                WriteLine($"开始清除数据表");
                db.Delete<Books>(printSql: true);
            }

            WriteLine($"开始导入落实总台账数据表: {InputDir}");

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
                    WriteLine($"{ident}开始清除落实总台账: {dwmc}");
                    db.Delete<Books>(where: $"dwmc = '{dwmc}'", printSql: true, ident);
                }
                
                WriteLine($"{ident}导入 {dwmc}: {count}");
                var beginRow = 2;
                var endRow = int.Parse(count) + 1;

                db.LoadExcel<Books>(
                    excel, beginRow, endRow,
                    new List<string> { dwmc, "C", "B", "D", "E" },
                    printSql: true, ident: ident);
            }
        }
    }

    [Verb("importJB", HelpText = "导入居保参保人员明细表")]
    class ImportJB : ICommand
    {
        [Value(0, HelpText = "xlsx文件",
            Required = true, MetaName = "xslx")]
        public string Xlsx { get; set; }

        [Value(1, HelpText = "数据开始行, 从1开始",
            Required = true, MetaName = "beginRow")]
        public int BeginRow { get; set; }

        [Value(2, HelpText = "数据结束行(包含), 从1开始",
            Required = true, MetaName = "endRow")]
        public int EndRow { get; set; }

        [Option("clear", HelpText = "是否清除数据表")]
        public bool Clear { get; set; } = false;

        public void Execute()
        {
            using var db = new Context();
            // WriteLine($"{Xlsx} {BeginRow} {EndRow} {Clear}");

            if (Clear)
            {
                WriteLine("开始清除数据表: 居保参保人员明细表");
                db.DeleteAll<Jbrymx>(printSql: true);
                WriteLine("结束清除数据表: 居保参保人员明细表");
            }

            WriteLine("开始导入居保参保人员明细表");
            db.LoadExcel<Jbrymx>(Xlsx, BeginRow, EndRow,
                new List<string>
                { "E", "A", "B", "C", "F", "G", "I", "K", "L", "O" },
                printSql: true);
            WriteLine("结束导入居保参保人员明细表");
        }
    }

    [Verb("updateJB", HelpText = "更新居保参保状态")]
    class UpdateJB : ICommand
    {
        public void Execute()
        {
            using var db = new Context();

            WriteLine($"开始更新居保参保状态");

            var yxfsj = db.GetTableName<Yxfsj>();
            var jbrymx = db.GetTableName<Jbrymx>();

            var sql =
                    $"update {yxfsj}, {jbrymx}\n" +
                    $"   set {yxfsj}.Sfycb='是',\n" +
                    $"       {yxfsj}.Cbsj={jbrymx}.Cbsj\n" +
                    $" where {yxfsj}.Idcard={jbrymx}.Idcard\n";
            db.ExecuteSql(sql, printSql: true);

            WriteLine($"结束更新居保状态");
        }
    }

    [Verb("updateYY", HelpText = "更新未参保原因")]
    class UpdateYY : ICommand
    {
        public void Execute()
        {
            using var db = new Context();

            WriteLine($"开始未参保原因");

            var yxfsj = db.GetTableName<Yxfsj>();
            var books = db.GetTableName<Books>();

            var sql =
                    $"update {yxfsj}, {books}\n" +
                    $"   set {yxfsj}.wcbyy={books}.hsqk\n" +
                    $" where {yxfsj}.Idcard={books}.Idcard\n" +
                    $"   and {yxfsj}.Sfycb<>'是'\n";
            db.ExecuteSql(sql, printSql: true);

            WriteLine($"结束未参保原因");
        }
    }

    [Verb("exportDC", HelpText = "导出未参保落实台账")]
    class exportDC : ICommand
    {
        const string tmplXlsx = @"D:\参保管理\参保全覆盖\雨湖区未参保落实台账模板.xlsx";

        [Option("where", HelpText = "导出条件语句")]
        public string Where { get; set; } = null;

        [Option("out", HelpText = "导出文件名")]
        public string FileName { get; set; } = null;

        public void Execute()
        {
            using var db = new Context();

            var saveXlsx = $@"D:\参保管理\参保全覆盖\雨湖区未参保落实台账{Util.DateTime.FormatedDate()}.xlsx";

            if (!string.IsNullOrEmpty(FileName))
            {
                saveXlsx = $@"D:\参保管理\参保全覆盖\{FileName}";
            }

            var sql = "SELECT * FROM fc_yxfsj ";
            if (!string.IsNullOrEmpty(Where))
            {
                sql += $" where {Where} ";
            }
            sql += " ORDER BY CONVERT(dwmc USING gbk), " +
                "FIELD(SUBSTRING(xfpc,2,1),'一','二','三','四','五','六','七','八','九'), no";

            // WriteLine($"{sql}, ${saveXlsx}"); return;

            IQueryable<Yxfsj> data = db.Yxfsjs.FromSqlRaw(sql);
            
            WriteLine($"开始导出未参保落实台账: =>{saveXlsx}");

            var workbook = ExcelExtension.LoadExcel(tmplXlsx);
            var sheet = workbook.GetSheetAt(0);
            int startRow = 2, currentRow = 2;

            foreach (var d in data)
            {
                var index = currentRow - startRow + 1;

                WriteLine($"{index} {d.Idcard} {d.Name}");

                var row = sheet.GetOrCopyRow(currentRow++, startRow);

                row.Cell("A").SetValue(index);
                row.Cell("B").SetValue(d.Dwmc);
                row.Cell("C").SetValue(d.Xfpc);
                row.Cell("D").SetValue(d.No);
                row.Cell("E").SetValue(d.Name);
                row.Cell("F").SetValue(d.Idcard);
                row.Cell("G").SetValue(d.Tcq);
                row.Cell("H").SetValue(d.Xzj);
                row.Cell("I").SetValue(d.Csq);
                row.Cell("J").SetValue(d.Sfycb);
                row.Cell("K").SetValue(d.Cbsj);
                row.Cell("L").SetValue(d.Wcbyy);
            }

            workbook.Save(saveXlsx);

            WriteLine($"结束导出未参保落实台账: =>{saveXlsx}");
        }
    }
}
