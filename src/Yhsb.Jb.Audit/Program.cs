﻿using System.IO;
using System.Linq;
using System.Collections.Generic;

using CommandLine;
using Yhsb.Util.Command;
using Yhsb.Util.Excel;
using Yhsb.Jb.Network;
using Yhsb.Jb.Database;

using static Yhsb.Util.DateTime;
using static System.Console;

namespace Yhsb.Jb.Audit
{
    using Map = Dictionary<string, string>;

    class Program
    {
        static void Main(string[] args)
        {
            Command.Parse<Audit>(args);
        }
    }

    class Audit : ICommand
    {
        [Value(0, HelpText = "起始审核时间, 例如: 20190429",
            Required = true)]
        public string StartDate { get; set; }

        [Value(0, HelpText = "截至审核时间, 例如: 20190505")]
        public string EndDate { get; set; }

        readonly static Map _jbClassMap = new Map
        {
            ["贫困人口一级"] = "051",
            ["特困一级"] = "031",
            ["低保对象一级"] = "061",
            ["低保对象二级"] = "062",
            ["残一级"] = "021",
            ["残二级"] = "022"
        };

        public void Execute()
        {
            var startDate = StartDate != null ? ConvertToDashedDate(StartDate) : "";
            var endDate = EndDate != null ? ConvertToDashedDate(EndDate) : "";
            var timeSpan = "";
            if (startDate != "")
            {
                timeSpan += startDate;
                if (endDate != "")
                {
                    timeSpan += "_" + endDate;
                }
            }
            WriteLine(timeSpan);

            var dir = @"D:\精准扶贫\";
            var xlsx = "批量信息变更模板.xls";

            Result<Cbsh> result = null;
            Session.Use(session =>
            {
                session.SendService(new CbshQuery(startDate, endDate));
                result = session.GetResult<Cbsh>();
            });

            if (result != null)
            {
                WriteLine($"共计 {result.Count} 条");
                if (result.Count > 0)
                {
                    var workbook = ExcelExtension.LoadExcel(Path.Join(dir, xlsx));
                    var sheet = workbook.GetSheetAt(0);
                    int index = 1, copyIndex = 1;
                    var export = false;
                    foreach (var cbsh in result.Data)
                    {
                        using var context = new JzfpContext();
                        var data = from fpData in context.FPTable2019
                                   where fpData.IDCard == cbsh.idCard
                                   select fpData;
                        if (data.Any())
                        {
                            var info = data.First();
                            WriteLine(
                                $"{cbsh.idCard} {cbsh.name} {cbsh.birthDay} {info.JBClass} " +
                                $"{(info.Name != cbsh.name ? info.Name : "")}");
                            var row = sheet.GetOrCopyRow(index++, copyIndex, false);
                            row.Cell("A").SetValue(cbsh.idCard);
                            row.Cell("C").SetValue(cbsh.name);
                            row.Cell("H").SetValue(_jbClassMap[info.JBClass]);
                            export = true;
                        }
                        else
                        {
                            WriteLine($"{cbsh.idCard} {cbsh.name} {cbsh.birthDay}");
                        }
                    }
                    if (export)
                    {
                        workbook.Save(
                            Path.Join(dir, $"批量信息变更{timeSpan}.xls"), true);
                    }
                }
            }
        }
    }
}
