using System;

using CommandLine;
using Yhsb.Util.Command;
using Yhsb.Util.Excel;
using Yhsb.Jb.Network;

namespace Yhsb.Jb.Payment
{
    class Program
    {
        [App(Name = "财务支付单生成程序")]
        static void Main(string[] args)
        {
            Command.Parse<Payment>(args);
        }

        public const string paymentXlsx = @"D:\支付管理\雨湖区居保个人账户返还表.xlsx";
    }

    class Payment : ICommand
    {        
        [Value(0, HelpText = "发放年月, 例如: 201904",
            Required = true, MetaName = "date")]
        public string Date { get; set; }

        [Value(1, HelpText = "业务状态: 0-待支付(默认), 1-已支付",
            MetaName = "state")]
        public string State { get; set; }

        public void Execute()
        {
            var workbook = ExcelExtension.LoadExcel(Program.paymentXlsx);
            var sheet = workbook.GetSheetAt(0);

            var (year, month, _) = Util.DateTime.SplitDate(Date);
            var title = $"{year}年{month}月个人账户返还表";
            sheet.Cell("A1").SetValue(title);

            var date = DateTime.Now.ToString("yyyyMMdd");
            var dateCH = DateTime.Now.ToString("yyyy年M月d日");
            var reportDate = $"制表时间：{dateCH}";
            sheet.Cell("H2").SetValue(reportDate);

            
        }
    }
}
