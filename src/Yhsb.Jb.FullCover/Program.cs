﻿using CommandLine;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

using Yhsb.Database;
using Yhsb.Jb.Database.FullCover2020;
using Yhsb.Util.Command;
using Yhsb.Util.Excel;

using Yhsb.Util;

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
                UpdateBooks, UpdateJB, UpdateYY, exportDC, GenCompareData,
                ImportFC2, ImportBdjg, ImportZxxs, ExportXfsj>(args);
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
                    $"update {yxfsj}\n" +
                    $"   set {yxfsj}.wcbyy=''\n" +
                    $" where {yxfsj}.Sfycb='是'\n";
            db.ExecuteSql(sql, printSql: true);
            sql =
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

    [Verb("updateBooks", HelpText = "更新落实总台账")]
    class UpdateBooks : ICommand
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

        public void Execute()
        {
            using var db = new Context();

            WriteLine("开始更新落实总台账");

            var workbook = ExcelExtension.LoadExcel(Xlsx);
            var sheet = workbook.GetSheetAt(0);

            for (var index = BeginRow - 1; index < EndRow; index++)
            {
                var row = sheet.Row(index);
                var no = row.Cell("A").Value();
                var book = new Books
                {
                    Dwmc = row.Cell("B").Value(),
                    Name = row.Cell("C").Value(),
                    Idcard = row.Cell("D").Value(),
                    Address = row.Cell("E").Value(),
                    Hsqk = row.Cell("F").Value(),
                };

                WriteLine($"{no} {book.Idcard} {book.Name.FillRight(6)}");
                
                var fcbook = from b in db.Books
                             where b.Idcard == book.Idcard
                             select b;
                if (fcbook.Any())
                {
                    foreach (var fb in fcbook)
                    {
                        db.Entry(fb).CurrentValues.SetValues(book);
                    }
                }
                else
                {
                    db.Add(book);
                }
                db.SaveChanges();
            }

            WriteLine("结束更新落实总台账");
        }
    }

    [Verb("genCData", HelpText = "生成比对上传数据程序")]
    class GenCompareData : ICommand
    {
        public void Execute()
        {
            const string tmplXlsx = @"D:\参保管理\参保全覆盖2\243030220200701000001Q020310.xls";
            string[] files = new string[] 
            {
                @"D:\参保管理\参保全覆盖2\原始下发数据\雨湖区未参加城乡居保.xls", 
                @"D:\参保管理\参保全覆盖2\原始下发数据\雨湖区未参加城乡居保１.xls"
            };

            const string dir = @"D:\参保管理\参保全覆盖2\上传比对数据\";

            int serialNO = 1;

            void genCData(string file, string tmpl) 
            {
                var workbook = ExcelExtension.LoadExcel(file);
                for (int i = 0; i < workbook.NumberOfSheets; i++)
                {
                    var sheetName = workbook.GetSheetName(i);
                    WriteLine($"生成比对上传数据: {sheetName}");

                    var outWorkbook = ExcelExtension.LoadExcel(tmpl);
                    var outSheet = outWorkbook.GetSheetAt(0);

                    int startRow = 3, currentRow = 3;

                    var sheet = workbook.GetSheetAt(i);
                    for (int r = 0; r <= sheet.LastRowNum; r++)
                    {
                        var row = sheet.Row(r);
                        var idcard = row.Cell("D").Value();
                        var name = row.Cell("C").Value();
                        var type = "170";
                        var xzqh = "430302";

                        if (string.IsNullOrEmpty(idcard)) break;
                        WriteLine($"{currentRow - startRow + 1} {idcard} {name}");

                        var outRow = outSheet.GetOrCopyRow(currentRow++, startRow);
                        outRow.Cell("A").SetValue(idcard);
                        outRow.Cell("B").SetValue(name);
                        outRow.Cell("C").SetValue(type);
                        outRow.Cell("D").SetValue(xzqh);
                    }

                    var NO = Util.StringEx.FillLeft($"{serialNO++}", 6, '0');
                    var outFile = $"{2}{430302}{Util.DateTime.FormatedDate()}{NO}{"Q02031"}{0}.xls";
                    var path = Path.Join(dir, outFile);

                    WriteLine($"保存至: {path}");
                    outWorkbook.Save(path);
                }
            }

            foreach (var file in files)
            {
                genCData(file, tmplXlsx);
            }
        }
    }

    [Verb("importFC2", HelpText = "导入全覆盖2省厅下发数据")]
    class ImportFC2 : ICommand
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

        [Value(3, HelpText = "在xlsx中表格序号, 默认为: 0", 
            MetaName = "tableIndex")]
        public int tableIndex { get; set; } = 0;

        [Option("clear", HelpText = "是否清除数据表")]
        public bool Clear { get; set; } = false;

        public void Execute()
        {
            using var db = new Context();

            if (Clear)
            {
                WriteLine("开始清除数据表: 全覆盖2省厅下发数据");
                db.DeleteAll<FC2Stxfsj>(printSql: true);
                WriteLine("结束清除数据表: 全覆盖2省厅下发数据");
            }

            WriteLine("开始导入下发数据表");
            db.LoadExcel<FC2Stxfsj>(Xlsx, BeginRow, EndRow,
                new List<string>
                { "A", "E", "D", "F", "G", "H", "0", "0", "0", "0", "", "", "" },
                new List<string> { "A" },
                printSql: true,
                tableIndex: tableIndex);
            WriteLine("结束导入下发数据表");
        }
    }

    [Verb("importBdjg", HelpText = "导入全覆盖2全国信息比对结果")]
    class ImportBdjg : ICommand
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

        [Value(3, HelpText = "在xlsx中表格序号, 默认为: 0", 
            MetaName = "tableIndex")]
        public int tableIndex { get; set; } = 0;

        [Option("clear", HelpText = "是否清除数据表")]
        public bool Clear { get; set; } = false;

        public void Execute()
        {
            using var db = new Context();

            if (Clear)
            {
                WriteLine("开始清除数据表: 全覆盖2全国信息比对结果");
                db.DeleteAll<FC2Qgbdjg>(printSql: true);
                WriteLine("结束清除数据表: 全覆盖2全国信息比对结果");
            }

            WriteLine("开始导入全国信息比对结果表");
            db.LoadExcel<FC2Qgbdjg>(Xlsx, BeginRow, EndRow,
                new List<string>
                { @"\N", "A", "E", "F", "G", "H", "I", "J", "K", "L", "" },
                //new List<string> { "A" },
                printSql: true,
                tableIndex: tableIndex);
            WriteLine("结束导入全国信息比对结果表");
        }
    }

    
    [Verb("importZxxs", HelpText = "导入在校学生数据")]
    class ImportZxxs : ICommand
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

        [Value(3, HelpText = "在xlsx中表格序号, 默认为: 0", 
            MetaName = "tableIndex")]
        public int tableIndex { get; set; } = 0;

        [Option("clear", HelpText = "是否清除数据表")]
        public bool Clear { get; set; } = false;

        public void Execute()
        {
            using var db = new Context();

            if (Clear)
            {
                WriteLine("开始清除数据表: 在校学生数据");
                db.DeleteAll<Zxxssj>(printSql: true);
                WriteLine("结束清除数据表: 在校学生数据");
            }

            WriteLine("开始导入在校学生数据");
            db.LoadExcel<Zxxssj>(Xlsx, BeginRow, EndRow,
                new List<string>
                { "A", "B", "C", "D", "E", "F", "G" },
                new List<string> { "A" },
                printSql: true,
                tableIndex: tableIndex);
            WriteLine("结束导入在校学生数据");
        }
    }

    [Verb("exportXfsj", HelpText = "导出全覆盖2下发数据清册")]
    class ExportXfsj : ICommand
    {
        const string tmplXlsx = @"D:\参保管理\参保全覆盖2\雨湖区全覆盖下发数据清册模板.xlsx";

        const int countPerExcel = 60000;

        [Value(0, HelpText = "导出目录", Required = true)]
        public string outputDir { get; set; } = null;

        public void Execute()
        {
            if (Directory.Exists(outputDir))
                Directory.Move(outputDir, outputDir + ".orig");
            Directory.CreateDirectory(outputDir);

            using var db = new Context();

            var sql = "SELECT * FROM fc2_stxfsj ORDER BY id";

            IQueryable<FC2Stxfsj> data = db.FC2Stxfsj.FromSqlRaw(sql);
            var dataCount = data.Count();

            var excelCount = dataCount / countPerExcel;
            if (dataCount - excelCount * countPerExcel > 0) excelCount += 1;

            WriteLine($"全覆盖下发数据共计: {dataCount}, 共分为 {excelCount} 个excel文件");

            for (var excelNO = 1; excelNO <= excelCount; excelNO ++)
            {
                sql = $"SELECT * FROM fc2_stxfsj ORDER BY id LIMIT {(excelNO - 1) * countPerExcel}, {countPerExcel}";

                data = db.FC2Stxfsj.FromSqlRaw(sql);

                var fileName = $"雨湖区全覆盖下发数据清册{excelNO}.xlsx";
                var filePath = Path.Join(outputDir, fileName);

                WriteLine($"开始导出第 {excelNO} 个文件: => {fileName}");

                var workbook = ExcelExtension.LoadExcel(tmplXlsx);
                var sheet = workbook.GetSheetAt(0);
                int startRow = 2, currentRow = 2;

                foreach (var d in data)
                {
                    var index = currentRow - startRow + 1;

                    WriteLine($"{index} {d.Id} {d.Idcard} {d.Name}");

                    var row = sheet.GetOrCopyRow(currentRow++, startRow);

                    row.Cell("A").SetValue(d.Id);
                    row.Cell("B").SetValue(d.Name);
                    row.Cell("C").SetValue(d.Idcard);
                    row.Cell("D").SetValue(d.Address);
                    row.Cell("E").SetValue(d.ManageName);
                    row.Cell("F").SetValue(d.Hsqk);
                    row.Cell("G").SetValue(d.Slcb);
                    row.Cell("H").SetValue(d.Swcb);
                    row.Cell("I").SetValue(d.InZxxssj == "1" ? "是" : "");
                    row.Cell("J").SetValue(d.InSfwqjb == "1" ? "是" : "");

                    if (index >= countPerExcel)
                        break;
                }

                workbook.Save(filePath);

                WriteLine($"结束导出第 {excelNO} 个文件: => {fileName}");
            }
        }
    }
}
