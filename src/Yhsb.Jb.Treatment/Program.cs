using CommandLine;
using Yhsb.Jb.Network;
using Yhsb.Jb.Database;
using Yhsb.Util;
using Yhsb.Util.Excel;
using Yhsb.Util.Command;
using System.Linq;

using static System.Console;

namespace Yhsb.Jb.Treatment
{
    class Program
    {
        public static string rootDir = @"D:\待遇核定";
        public static string infoXlsx = $@"{rootDir}\信息核对报告表模板.xlsx";
        public static string payInfoXlsx = $@"{rootDir}\养老金计算表模板.xlsx";
        public static string fphdXlsx = $@"{rootDir}\到龄贫困人员待遇核定情况表模板.xlsx";

        static void Main(string[] args)
        {
            Command.Parse<Fphd>(args);
        }
    }

    [Verb("fphd",
        HelpText = "从业务系统下载生成到龄贫困人员待遇核定情况表")]
    class Fphd : ICommand
    {
        [Value(0, HelpText = "截至到龄日期，格式：yyyymmdd",
            Required = true)]
        public string Date { get; set; }

        public void Execute()
        {
            var dlny = DateTime.ConvertToDashedDate(Date);
            var saveXlsx = $@"{Program.rootDir}\到龄贫困人员待遇核定情况表(截至{Date}).xlsx";

            var workbook = ExcelExtension.LoadExcel(Program.fphdXlsx);
            var sheet = workbook.GetSheetAt(0);
            int startRow = 3, currentRow = 3;

            Result<Dyry> result = null;
            Session.Use(session =>
            {
                session.SendService(new DyryQuery(dlny));
                result = session.GetResult<Dyry>();
            });

            if (result != null && result.Data.Count > 0)
            {
                using var context = new FpRawDataContext("2019年度扶贫办民政残联历史数据");
                foreach (var data in result.Data)
                {
                    var idcard = data.idcard;
                    var fpData = from e in context.Entity
                                 where e.IDCard == idcard &&
                                 (e.Type == "贫困人口" ||
                                 e.Type == "特困人员" ||
                                 e.Type == "全额低保人员" ||
                                 e.Type == "差额低保人员")
                                 select e;
                    if (fpData.Any())
                    {
                        WriteLine($"{currentRow - startRow + 1} {data.idcard} {data.name}");

                        var qjns = data.Yjnx - data.Sjnx;
                        if (qjns < 0) qjns = 0;
                        var record = fpData.First();

                        var row = sheet.GetOrCopyRow(currentRow++, startRow);
                        row.Cell("A").SetValue(currentRow - startRow);
                        row.Cell("B").SetValue(data.xzqh);
                        row.Cell("C").SetValue(data.name);
                        row.Cell("D").SetValue(data.idcard);
                        row.Cell("E").SetValue(data.birthDay);
                        row.Cell("F").SetValue(data.sex.ToString());
                        row.Cell("G").SetValue(data.hJClass.ToString());
                        row.Cell("H").SetValue(record.Name);
                        row.Cell("I").SetValue(record.Type);
                        row.Cell("J").SetValue(data.JBState);
                        row.Cell("K").SetValue(data.lqny);
                        row.Cell("L").SetValue(data.Yjnx);
                        row.Cell("M").SetValue(data.Sjnx);
                        row.Cell("N").SetValue(qjns);
                        row.Cell("O").SetValue(data.qbzt);
                        row.Cell("P").SetValue(data.bz);
                    }
                }
            }
            workbook.Save(saveXlsx);
        }
    }
}
