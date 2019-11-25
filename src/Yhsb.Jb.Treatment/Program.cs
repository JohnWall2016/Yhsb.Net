using CommandLine;
using Yhsb.Jb.Network;
using Yhsb.Jb.Database;
using Yhsb.Util;
using Yhsb.Util.Excel;
using Yhsb.Util.Command;

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
            Session.Use(session => {
                session.SendService(new DyryQuery(dlny));
                result = session.GetResult<Dyry>();
            });
            
            if (result != null && result.Data.Count > 0)
            {
                using var context = new FpDataContext("2019年度扶贫历史数据底册");
            }
        }
    }
}
