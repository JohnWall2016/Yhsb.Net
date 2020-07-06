using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using static System.Console;

using Microsoft.EntityFrameworkCore;

using Yhsb.Util.Excel;

namespace Yhsb.Database
{
    public static class DatabaseEx
    {
        public static string GetTableName<T>(this DbContext context)
            where T : class
        {
            /*
            var type = typeof(T);
            var attrs = type.GetCustomAttributes(
                typeof(TableAttribute), false);
            return attrs.Any() ? 
                (attrs[0] as TableAttribute).Name : type.Name;
            */
            var entityType = context.Model.FindEntityType(typeof(T));
            // var schema = entityType.GetSchema();
            return entityType.GetTableName();
        }

        public static int ExecuteSql(
            this DbContext context, string sql, bool printSql = false, string ident = "")
        {
            if (printSql) WriteLine(ident + "SQL: " + sql);
            return context.Database.ExecuteSqlRaw(sql);
        }

        /// <summary>将Excel表格导入数据库</summary>
        ///
        /// <param name="startRow">开始行(从1开始)</param>
        /// <param name="endRow">结束行(包含)</param>
        ///
        public static int LoadExcel<T>(
            this DbContext context, string fileName, int startRow,
            int endRow, List<string> fields, List<string> noQuotes = null,
            bool printSql = false, string ident = "", int tableIndex = 0)
            where T : class
        {
            var workbook = ExcelExtension.LoadExcel(fileName);
            var sheet = workbook.GetSheetAt(tableIndex);
            var regex = new Regex("^[A-Z]+$", RegexOptions.IgnoreCase);

            var builder = new StringBuilder();
            for (var index = startRow - 1; index < endRow; index++)
            {
                try
                {
                    var values = new List<string>();
                    foreach (var row in fields)
                    {
                        string value = row;
                        if (regex.IsMatch(row))
                        {
                            value = sheet.Row(index).Cell(row).Value();
                            if (noQuotes == null || !noQuotes.Contains(row))
                                value = $"'{value}'";
                        }
                        values.Add(value);
                    }
                    builder.Append(string.Join(',', values));
                    builder.Append("\n");
                }
                catch (Exception ex)
                {
                    throw new Exception($"LoadExcel error at row {index + 1}", ex);
                }
            }

            var tmpFileName = Path.GetTempFileName();
            File.AppendAllText(tmpFileName, builder.ToString());

            var cvsFileName = new Uri(tmpFileName).AbsolutePath;
            var tableName = context.GetTableName<T>();
            var sql = $@"load data infile '{cvsFileName}' into table `{tableName}` " +
                @"CHARACTER SET utf8 FIELDS TERMINATED BY ',' OPTIONALLY " +
                @"ENCLOSED BY '\'' LINES TERMINATED BY '\n';";
            
            var result = context.ExecuteSql(sql, printSql, ident);

            if (File.Exists(tmpFileName))
                File.Delete(tmpFileName);

            return result;
        }

        public static int DeleteAll<T>(
            this DbContext context, bool printSql = false, string ident = "")
            where T : class
        {
            var sql = $"delete from {context.GetTableName<T>()};";
            return context.ExecuteSql(sql, printSql, ident);
        }

        public static int Delete<T>(
            this DbContext context, string where = null, bool printSql = false, string ident = "")
            where T : class
        {
            if (!string.IsNullOrWhiteSpace(where))
                where = $" where {where}";
            else
                where = "";

            var sql = $"delete from {context.GetTableName<T>()}{where};";
            return context.ExecuteSql(sql, printSql, ident);
        }
    }
}
