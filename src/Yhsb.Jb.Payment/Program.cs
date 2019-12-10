using System;

using CommandLine;
using Yhsb;
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

        public const string paymentXlsx = 
            @"D:\支付管理\雨湖区居保个人账户返还表.xlsx";
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

            Session.Use(session =>
            {
                int startRow = 4, currentRow = 4;
                decimal sum = 0;

                session.SendService(new PaymentQuery(Date, State));
                var result = session.GetResult<Network.Payment>();

                foreach (var data in result.Data)
                {
                    if (data.payType == "3")
                    {
                        session.SendService(new PaymentDetailQuery(
                            NO: $"{data.NO}", yearMonth: $"{data.yearMonth}",
                            state: $"{data.state}", type: $"{data.type}"));
                        var detailResult = session.GetResult<PaymentDetail>();
                        var payment = detailResult[0];

                        string reason = null, bankName = null;
                        session.SendService(new DyzzfhQuery(payment.idCard));
                        var dyzzResult = session.GetResult<Dyzzfh>();
                        if (!dyzzResult.IsEmpty)
                        {
                            session.SendService(new DyzzfhDetailQuery(dyzzResult[0]));
                            var dyzzDetailResult = session.GetResult<DyzzfhDetail>();
                            if (!dyzzDetailResult.IsEmpty)
                            {
                                var info = dyzzDetailResult[0];
                                reason = info.reason.Name;
                                bankName = info.BankName;
                            }
                        }
                        else
                        {
                            session.SendService(new CbzzfhQuery(payment.idCard));
                            var cbzzResult = session.GetResult<Cbzzfh>();
                            if (!cbzzResult.IsEmpty)
                            {
                                session.SendService(new CbzzfhDetailQuery(cbzzResult[0]));
                                var cbzzDetailResult = session.GetResult<CbzzfhDetail>();
                                if (!cbzzDetailResult.IsEmpty)
                                {
                                    var info = cbzzDetailResult[0];
                                    reason = info.reason.Name;
                                    bankName = info.BankName;
                                }
                            }
                        }

                        var row = sheet.GetOrCopyRow(currentRow++, startRow);
                        row.Cell("A").SetValue(currentRow - startRow);
                        row.Cell("B").SetValue(payment.name);
                        row.Cell("C").SetValue(payment.idCard);

                        var type = payment.TypeCH;
                        if (reason != null)
                            type = $"{type}({reason})";

                        var amount = payment.amount;
                        row.Cell("D").SetValue(type);
                        row.Cell("E").SetValue(payment.payList);
                        row.Cell("F").SetValue(amount);
                        row.Cell("G").SetValue(
                            Util.StringEx.ConvertToChineseMoney(amount));
                        row.Cell("H").SetValue(data.name);
                        row.Cell("I").SetValue(data.account);
                        row.Cell("J").SetValue(bankName);

                        sum += amount;
                    }
                }
                var trow = sheet.GetOrCopyRow(currentRow, startRow);
                trow.Cell("A").SetValue("合计");
                trow.Cell("F").SetValue(sum);

                workbook.Save(Util.StringEx.AppendToFileName(
                    Program.paymentXlsx, date));
            });
        }
    }
}
