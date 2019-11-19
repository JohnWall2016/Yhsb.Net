using System;
using System.IO;
using System.Reflection;
using System.Linq;

using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;

namespace Yhsb.Util
{
    public static class StreamExtension
    {
        public static void Write(this Stream stream, byte[] buffer)
        {
            stream.Write(buffer, 0, buffer.Length);
        }
    }

    public static class PrintExtension
    {
        public static void Print<T>(this T obj)
        {
            Console.WriteLine($"{obj}");
        }
    }

    public sealed class DescriptionAttribute : Attribute
    {
        public string Description { get; }

        public DescriptionAttribute(string description)
            => Description = description;
    }

    public static class EnumExtension
    {
        public static string GetDescription(this Enum This)
        {
            Type type = This.GetType();

            string name = Enum.GetName(type, This);

            MemberInfo member = type.GetMembers()
                .Where(w => w.Name == name)
                .FirstOrDefault();

            DescriptionAttribute attribute = member != null
                ? member.GetCustomAttributes(true)
                    .Where(w => w.GetType() == typeof(DescriptionAttribute))
                    .FirstOrDefault() as DescriptionAttribute
                : null;

            return attribute != null ? attribute.Description : name;
        }
    }

    public static class ExcelExtension
    {
        public enum ExcelType
        {
            XLS, XLSX, AUTO
        }

        public static IWorkbook LoadExcel(
            string fileName, ExcelType type = ExcelType.AUTO)
        {
            Stream stream = new MemoryStream();
            using (var file = new FileStream(
                fileName, FileMode.Open, FileAccess.Read))
            {
                file.CopyTo(stream);
                stream.Seek(0, SeekOrigin.Begin);
            }
            if (type == ExcelType.AUTO)
            {
                var ext = Path.GetExtension(fileName).ToLower();
                type = ext switch
                {
                    ".xls" => ExcelType.XLS,
                    ".xlsx" => ExcelType.XLSX,
                    _ => throw new ArgumentException("Unknown excel type"),
                };
            }
            return type switch
            {
                ExcelType.XLS => new HSSFWorkbook(stream),
                ExcelType.XLSX => new XSSFWorkbook(stream),
                _ => throw new ArgumentException("Unknown excel type")
            };
        }

        public static void Save(this IWorkbook wb, string fileName)
        {
            using var stream = new FileStream(fileName, FileMode.CreateNew);
            wb.Write(stream);
        }

        public static IRow Row(this ISheet sheet, int row)
            => sheet.GetRow(row);

        public static ICell Cell(this ISheet sheet, int row, int col)
            => sheet.GetRow(row).GetCell(col);

        public static ICell Cell(this IRow row, int col)
            => row.GetCell(col);

        public static void SetValue(this ICell cell, string value)
            => cell.SetCellValue(value);

        public static void SetValue(this ICell cell, double value)
            => cell.SetCellValue(value);

        public static string CellValue(this ICell cell)
            => cell.CellType switch
            {
                CellType.String => cell.StringCellValue,
                CellType.Numeric => cell.NumericCellValue.ToString(),
                CellType.Blank => "",
                CellType.Boolean => cell.BooleanCellValue.ToString(),
                _ => throw new InvalidOperationException(),
            };

        public static IRow GetOrCopyRowFrom(
            this ISheet sheet, int dstRowIdx, int srcRowIdx)
        {
            if (dstRowIdx <= srcRowIdx)
                return sheet.GetRow(srcRowIdx);
            else
            {
                if (sheet.LastRowNum >= dstRowIdx)
                    sheet.ShiftRows(
                        dstRowIdx, sheet.LastRowNum, 1, true, false);
                var dstRow = sheet.CreateRow(dstRowIdx);
                var srcRow = sheet.GetRow(srcRowIdx);
                dstRow.Height = srcRow.Height;
                for (var idx = (int)srcRow.FirstCellNum; 
                    idx < srcRow.PhysicalNumberOfCells; idx++)
                {
                    var dstCell = dstRow.CreateCell(idx);
                    var srcCell = srcRow.GetCell(idx);
                    dstCell.SetCellType(srcCell.CellType);
                    dstCell.CellStyle = srcCell.CellStyle;
                    dstCell.SetValue("");
                }
                return dstRow;
            }
        }
    }
}