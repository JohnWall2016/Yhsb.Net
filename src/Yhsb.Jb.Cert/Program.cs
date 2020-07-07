using CommandLine;
using Yhsb.Jb.Network;
using Yhsb.Util;
using Yhsb.Util.Excel;
using Yhsb.Util.Command;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using NPOI.SS.UserModel;

using static System.Console;

namespace Yhsb.Jb.Cert
{
    class Program
    {
        public static string CertTemplate = @"D:\待遇认证\2020年\城乡居民基本养老保险待遇领取人员资格认证表（表二）.xls";

        [App(Name = "待遇认证数据统计和表格生成程序")]
        static void Main(string[] args)
        {
            Command.Parse<Statics, Split>(args);
        }

        public class Group
        {
            public int Total;
            public Dictionary<string, List<int>> Data;
        }

        public static Dictionary<string, Group>
            GenerateGroupData(ISheet sheet, int beginRow, int endRow)
        {
            // WriteLine("生成分组映射表");
            var map = new Dictionary<string, Group>();
            for (var index = beginRow - 1; index < endRow; index++)
            {
                var xzqh = sheet.Cell(index, "A").Value();
                Match match = null;
                foreach (var regex in Xzqh.regex)
                {
                    match = Regex.Match(xzqh, regex);
                    if (match.Success) break;
                }
                if (match == null || !match.Success)
                    throw new ApplicationException($"未匹配行政区划: {xzqh}");
                else
                {
                    var xzj = match.Groups[2].Value;
                    var csq = match.Groups[3].Value;
                    if (!map.ContainsKey(xzj))
                        map[xzj] = new Group 
                        {
                            Total = 0, 
                            Data = new Dictionary<string, List<int>>()
                        };

                    if (!map[xzj].Data.ContainsKey(csq))
                    {
                        map[xzj].Data[csq] = new List<int>{index};
                        map[xzj].Total += 1;
                    }
                    else
                    {
                        map[xzj].Data[csq].Add(index);
                        map[xzj].Total += 1;
                    }
                }
            }
            return map;
        }
    }

    [Verb("statics", HelpText = "待遇认证数据统计")]
    class Statics : ICommand
    {
        [Value(0, HelpText = "认证人员EXCEL文件", Required = true)]
        public string CertExcel { get; set; }

        [Value(1, HelpText = "开始行，从1开始", Required = true)]
        public int BeginRow { get; set; }

        [Value(2, HelpText = "结束行(包含)，从1开始", Required = true)]
        public int EndRow { get; set; }

        [Option("full", HelpText = "显现所有数据")]
        public bool Full { get; set; }

        public void Execute()
        {
            var workbook = ExcelExtension.LoadExcel(CertExcel);
            var sheet = workbook.GetSheetAt(0);
            var map = Program.GenerateGroupData(sheet, BeginRow, EndRow);
            var total = 0;

            foreach (var (xzj, group) in map)
            {
                WriteLine($"{(xzj+":").FillRight(11)} {group.Total}");
                total += group.Total;

                if (Full)
                {
                    foreach (var (csq, list) in group.Data)
                    {
                        WriteLine($"    {(csq+":").FillRight(11)} {list.Count}");
                    }
                }
            }
            WriteLine($"{"合计:".FillRight(11)} {total}");
        }
    }

    [Verb("split", HelpText = "待遇认证表格生成")]
    class Split : ICommand
    {
        [Value(0, HelpText = "认证人员EXCEL文件", Required = true)]
        public string CertExcel { get; set; }

        [Value(1, HelpText = "开始行，从1开始", Required = true)]
        public int BeginRow { get; set; }

        [Value(2, HelpText = "结束行(包含)，从1开始", Required = true)]
        public int EndRow { get; set; }

        [Value(3, HelpText = "导出表格目录", Required = true)]
        public string OutputDir { get; set; }

        public void Execute()
        {
            var workbook = ExcelExtension.LoadExcel(CertExcel);
            var sheet = workbook.GetSheetAt(0);
            var map = Program.GenerateGroupData(sheet, BeginRow, EndRow);

            if (Directory.Exists(OutputDir))
                Directory.Move(OutputDir, OutputDir + ".orig");
            Directory.CreateDirectory(OutputDir);

            var total = 0;
            foreach (var (xzj, group) in map)
            {
                WriteLine($"{(xzj+":").FillRight(11)} {group.Total}");

                total += group.Total;

                Directory.CreateDirectory(Path.Join(OutputDir, xzj));

                foreach (var (csq, list) in group.Data)
                {
                    WriteLine($"    {(csq+":").FillRight(11)} {list.Count}");

                    var outWorkbook = ExcelExtension.LoadExcel(Program.CertTemplate);
                    var outSheet = outWorkbook.GetSheetAt(0);
                    int startRow = 4, currentRow = 4;

                    list.ForEach(rowIndex =>
                    {
                        var index = currentRow - startRow + 1;
                        var inRow = sheet.Row(rowIndex);
                        
                        var row = outSheet.GetOrCopyRow(currentRow++, startRow);
                        row.Cell("A").SetValue(index);
                        row.Cell("B").SetValue(inRow.Cell("C").Value());
                        row.Cell("C").SetValue((inRow.Cell("E").Value() == "1") ? "男" : "女");
                        row.Cell("D").SetValue(inRow.Cell("D").Value());
                        row.Cell("E").SetValue(inRow.Cell("A").Value());
                        row.Cell("M").SetValue(inRow.Cell("I").Value());
                    });

                    outWorkbook.Save(
                        Path.Join(OutputDir, xzj, $"{csq}.xls"));
                }
            }
            WriteLine($"{"合计:".FillRight(11)} {total}");
        }
    }
}
