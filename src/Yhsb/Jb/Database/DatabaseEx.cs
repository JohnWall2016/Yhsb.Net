using System;
using System.Collections.Generic;
using static System.Console;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Yhsb.Util.Excel;

namespace Yhsb.Jb.Database
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
            this DbContext context, string sql, bool printSql = false)
        {
            if (printSql) WriteLine("SQL: " + sql);
            return context.Database.ExecuteSqlRaw(sql);
        }

        public static int LoadExcel<T>(
            this DbContext context, string fileName, int startRow,
            int endRow, List<string> fields, List<string> noQuotes = null,
            bool printSql = false)
            where T : class
        {
            var workbook = ExcelExtension.LoadExcel(fileName);
            var sheet = workbook.GetSheetAt(0);

            var builder = new StringBuilder();
            for (var index = startRow - 1; index < endRow; index++)
            {
                var values = new List<string>();
                foreach (var row in fields)
                {
                    var value = sheet.Row(index).Cell(row).Value();
                    if (noQuotes != null && noQuotes.Contains(row))
                        value = $"'{value}'";
                    values.Add(value);
                }
                builder.Append(string.Join(',', values));
                builder.Append("\n");
            }

            var tmpFileName = Path.GetTempFileName();
            File.AppendAllText(tmpFileName, builder.ToString());

            var cvsFileName = new Uri(tmpFileName).AbsolutePath;
            var tableName = context.GetTableName<T>();
            var sql = $@"load data infile '{cvsFileName}' into table `{tableName}` " +
                @"CHARACTER SET utf8 FIELDS TERMINATED BY ',' OPTIONALLY " +
                @"ENCLOSED BY '\'' LINES TERMINATED BY '\n';";
            
            var result = context.ExecuteSql(sql, printSql);

            if (File.Exists(tmpFileName))
                File.Delete(tmpFileName);

            return result;
        }

        public static int DeleteAll<T>(
            this DbContext context, bool printSql = false)
            where T : class
        {
            var sql = $"delete from {context.GetTableName<T>()};";
            return context.ExecuteSql(sql, printSql);
        }
    }
}
