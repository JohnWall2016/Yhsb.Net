using System;

using CommandLine;
using Yhsb.Util.Command;
using Yhsb.Util.Excel;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Yhsb.Jb.Network;

using static System.Console;

namespace Yhsb.Split
{
    class Program
    {
        [App(Name = "按单位分组生成数据表程序")]
        static void Main(string[] args)
        {
            Command.Parse<Split>(args);
        }
    }

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

        [Value(3, HelpText = "行政区划所在列，例如: B",
            Required = true, MetaName = "XzqhCol")]
        public string XzqhCol { get; set; }
        
        [Value(4, HelpText = "分组模板表路径",
            Required = true, MetaName = "TemplateExcel")]
        public string TemplateExcel { get; set; }
        
        [Value(5, HelpText = "分组模板表开始行，从1开始",
            Required = true)]
        public int TemplateBeginRow { get; set; }

        [Value(6, HelpText = "输出目录",
            Required = true, MetaName = "OutDir")]
        public string OutDir { get; set; }

        [Value(7, HelpText = "序号所在列",
            Required = false, MetaName = "NOCel")]
        public string NOCel { get; set; } = null;

        public void Execute()
        {
            var workbook = ExcelExtension.LoadExcel(SourceExcel);
            var sheet = workbook.GetSheetAt(0);

            WriteLine("生成分组映射表");
            var map = new Dictionary<string, List<int>>();
            for (var index = BeginRow - 1; index < EndRow; index++)
            {
                var xzqh = sheet.Cell(index, XzqhCol).Value();
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
                    if (!map.ContainsKey(xzj))
                        map[xzj] = new List<int>();
                    map[xzj].Add(index);
                }
            }

            WriteLine("生成分组数据表");
            /*if (Directory.Exists(OutDir))
                Directory.Move(OutDir, OutDir + ".orig");
            Directory.CreateDirectory(OutDir);*/

            foreach (var xzj in map.Keys)
            {
                var count = map[xzj].Count;
                WriteLine($"{xzj}: {count}");

                if (count <= 0) continue;

                var outWorkbook = ExcelExtension.LoadExcel(TemplateExcel);
                var outSheet = outWorkbook.GetSheetAt(0);

                int startRow = TemplateBeginRow - 1, currentRow = startRow;

                map[xzj].ForEach(rowIndex => {
                    var index = currentRow - startRow + 1;
                    var inRow = sheet.Row(rowIndex);
                    //WriteLine($"    {index} {currentRow} {startRow}");
                    var outRow = outSheet.GetOrCopyRow(currentRow++, startRow);
                    for (var cell = inRow.FirstCellNum; cell < inRow.LastCellNum; cell++) {
                        //WriteLine($"{cell}");
                        outRow.Cell(cell).SetValue(inRow.Cell(cell).Value());
                    }
                    if (NOCel != null) {
                        outRow.Cell(NOCel).SetValue(index);
                    }
                });
                outWorkbook.Save(
                    Path.Join(OutDir, $"{xzj}{map[xzj].Count}.xls"));
            }
        }
    }
}
