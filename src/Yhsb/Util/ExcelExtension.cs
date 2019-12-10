using System;
using System.IO;
using System.Text.RegularExpressions;

using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;

namespace Yhsb.Util.Excel
{
    public class CellRef
    {
        public int Row { get; set; }
        public int Column { get; set; }

        public string ColumnName { get; set; }

        public bool RowAnchored { get; set; }
        public bool ColumnAnchored { get; set; }

        static readonly string _cellRegex = @"(\$?)([A-Z]+)(\$?)(\d+)";

        public CellRef(
            int row, int column, bool anchored = false,
            bool rowAnchored = false, bool columnAnchored = false)
        {
            Row = row; 
            Column = column;
            ColumnName = ColumnNumberToName(column);
            RowAnchored = anchored || rowAnchored;
            ColumnAnchored = anchored || columnAnchored;
        }

        public CellRef() {}

        public static CellRef FromAddress(string address)
        {
            var match = Regex.Match(address, _cellRegex);
            if (match.Success)
            {
                return new CellRef
                {
                    ColumnAnchored = match.Groups[1].Length > 0,
                    ColumnName = match.Groups[2].Value,
                    Column = ColumnNameToNumber(match.Groups[2].Value),
                    RowAnchored = match.Groups[3].Length > 0,
                    Row = int.Parse(match.Groups[4].Value)
                };
            }
            return null;
        }

        public string ToAddress()
        {
            string address = "";
            if (ColumnAnchored) address += "$";
            address += ColumnName;
            if (RowAnchored) address += "$";
            address += Row.ToString();
            return address;
        }

        public static string ColumnNumberToName(int number)
        {
            var dividend = number;
            var name = "";

            while (dividend > 0)
            {
                var modulo = (dividend - 1) % 26;
                name = Convert.ToChar(65 + modulo).ToString() + name;
                dividend = (dividend - modulo) / 26;
            }

            return name;
        }

        public static int ColumnNameToNumber(string name)
        {
            name = name.ToUpper();
            var sum = 0;
            for (var i = 0; i < name.Length; i++)
            {
                sum *= 26;
                sum += name[i] - 64;
            }
            return sum;
        }
    }
    
    public static class ExcelExtension
    {
        public enum Type
        {
            XLS, XLSX, AUTO
        }

        public static IWorkbook LoadExcel(
            string fileName, Type type = Type.AUTO)
        {
            Stream stream = new MemoryStream();
            using (var file = new FileStream(
                fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                file.CopyTo(stream);
                stream.Seek(0, SeekOrigin.Begin);
            }
            if (type == Type.AUTO)
            {
                var ext = Path.GetExtension(fileName).ToLower();
                type = ext switch
                {
                    ".xls" => Type.XLS,
                    ".xlsx" => Type.XLSX,
                    _ => throw new ArgumentException("Unknown excel type"),
                };
            }
            return type switch
            {
                Type.XLS => new HSSFWorkbook(stream),
                Type.XLSX => new XSSFWorkbook(stream),
                _ => throw new ArgumentException("Unknown excel type")
            };
        }

        public static void Save(
            this IWorkbook wb, string fileName, bool overwrite = false)
        {
            if (File.Exists(fileName) && overwrite) File.Delete(fileName);
            using var stream = new FileStream(fileName, FileMode.CreateNew);
            wb.Write(stream);
        }

        public static IRow Row(
            this ISheet sheet, int row) => sheet.GetRow(row);

        public static ICell Cell(
            this ISheet sheet, int row, int col) =>
                sheet.GetRow(row).Cell(col);

        public static ICell Cell(
            this ISheet sheet, int row, string columnName) =>
                sheet.GetRow(row).Cell(columnName);

        public static ICell Cell(this ISheet sheet, string address)
        {
            var cell = CellRef.FromAddress(address);
            return sheet.GetRow(cell.Row - 1).Cell(cell.Column - 1);
        }

        public static ICell Cell(this IRow row, string columnName) =>
            row.Cell(CellRef.ColumnNameToNumber(columnName) - 1);
        
        public static ICell Cell(this IRow row, int col)
        {
            var cell = row.GetCell(col);
            if (cell == null)
            {
                cell = row.CreateCell(col);
            }
            return cell;
        }

        public static void SetValue(
            this ICell cell, string value) => cell.SetCellValue(value);

        public static void SetValue(
            this ICell cell, int value) => cell.SetCellValue(value);

        public static void SetValue(
            this ICell cell, double value) => cell.SetCellValue(value);

        public static void SetValue(
            this ICell cell, decimal value) => cell.SetCellValue((double)value);

        public static string Value(this ICell cell) =>
            cell.CellType switch
            {
                CellType.String => cell.StringCellValue,
                CellType.Numeric => cell.NumericCellValue.ToString(),
                CellType.Blank => "",
                CellType.Boolean => cell.BooleanCellValue.ToString(),
                _ => throw new InvalidOperationException(),
            };

        public static IRow GetOrCopyRow(
            this ISheet sheet, int index, int copyIndex, bool clearValue = true)
        {
            IRow dstRow = null;
            if (copyIndex == index)
            {
                dstRow = sheet.GetRow(copyIndex);
            }
            else
            {
                if (sheet.LastRowNum >= index)
                    sheet.ShiftRows(
                        index, sheet.LastRowNum, 1, true, false);
                dstRow = sheet.CopyRow(copyIndex, index);
                var srcRow = sheet.GetRow(copyIndex);
                dstRow.Height = srcRow.Height;
                var merged = new CellRangeAddressList();
                for (var i = 0; i < sheet.NumMergedRegions; i++)
                {
                    var address = sheet.GetMergedRegion(i);
                    if (copyIndex == address?.FirstRow 
                        && copyIndex == address?.LastRow)
                    {
                        merged.AddCellRangeAddress(
                            index, address.FirstColumn, index, address.LastColumn);
                    }
                }
                for (var i = 0; i < merged.CellRangeAddresses.Length; i++)
                    sheet.AddMergedRegion(merged.CellRangeAddresses[i]);
            }
            if (clearValue)
            {
                dstRow.Cells.ForEach(c => c.SetValue(null));
            }
            return dstRow;
        }
    }
}