using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using static System.Console;

using Yhsb.Util.Excel;

namespace Yhsb.Gb.Excel
{
    class Program
    {
        static void Main(string[] args)
        {
            var srcDir = @"E:\机关养老保险\原数据";
            var outDir = @"E:\机关养老保险\新数据";
            var tmpXls = @"E:\机关养老保险\（模板）试点期间参保人员缴费确认表.xls";

            foreach (var xls in Directory.EnumerateFiles(srcDir))
            {
                WriteLine($"{xls}");

                var workbook = ExcelExtension.LoadExcel(xls);
                var sheet = workbook.GetSheetAt(0);

                var outWorkbook = ExcelExtension.LoadExcel(tmpXls);
                var outSheet = outWorkbook.GetSheetAt(0);

                (int start, int end) copyRange = (1, 11);
                int startRow = 4, currentRow = 4;

                var code = sheet.Cell(2, 2).Value();
                var name = sheet.Cell(2, 6).Value();
                outSheet.Cell(2, 2).SetValue(code);
                outSheet.Cell(2, 6).SetValue(name);

                WriteLine($"{name} {code}");
                
                for (var i = 4; i < sheet.LastRowNum; i++)
                {
                    var r = copyRange.start;
                    var row = sheet.GetRow(i);
                    var id = row.Cell(r)?.Value();
                    if (id == null) continue;
                    if (Regex.IsMatch(id, @"^\d+$"))
                    {
                        WriteLine($"{currentRow} {id}");

                        var outRow = outSheet.GetOrCopyRow(currentRow++, startRow);
                        outRow.Cell(r).SetValue(id);
                        for (r +=1; r < copyRange.end; r++)
                        {
                            if (r == 8 || r == 9) 
                            {
                                outRow.Cell(r).SetCellValue(row.Cell(r).NumericCellValue);
                            }
                            else
                            {
                                outRow.Cell(r).SetValue(row.Cell(r).Value());
                            }
                        }
                    }
                    else if (id == "说明：")
                    {
                        var total = sheet.GetRow(i - 2).Cell("B").Value();
                        var hj = sheet.GetRow(i - 1).Cell("I").NumericCellValue;
                        var lx = sheet.GetRow(i - 1).Cell("J").NumericCellValue;
                        WriteLine($"{total} 合计 {hj} {lx}");
                        
                        var outRow = outSheet.GetOrCopyRow(currentRow++, startRow);
                        outRow.Cell("B").SetValue(total);
                        outRow.Cell("H").SetValue("合计");
                        outRow.Cell("I").SetCellValue(hj);
                        outRow.Cell("J").SetCellValue(lx);
                        break;
                    }
                }

                outWorkbook.Save(Path.Join(outDir, $"试点期间参保人员缴费确认表_{code}_{name}.xls"));
                //break;
            }
        }
    }
}
