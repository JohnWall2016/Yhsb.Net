using System;
using System.Globalization;
using System.Collections.Generic;
using CommandLine;
using Yhsb.Util.Command;
using Yhsb.Util.Excel;
using Yhsb.Jb.Network;

using static System.Console;

using JbOtherPayment = Yhsb.Jb.Network.OtherPayment;

namespace Yhsb.Jb.OtherPayment
{
    class Program
    {
        static void Main(string[] args)
        {
            Command.Parse<PersonList, PayList>(args);
        }

        public const string payListXlsx =
            @"D:\代发管理\雨湖区城乡居民基本养老保险代发人员支付明细.xlsx";
    }

    [Verb("payList", HelpText = "代发支付明细导出")]
    class PayList : ICommand
    {
        [Value(0, HelpText =
            "业务类型: DF0001 - 独生子女, DF0002 - 乡村教师, " +
            "DF0003 - 乡村医生, DF0007 - 电影放映员",
            Required = true, MetaName = "type")]
        public string Type { get; set; }

        [Value(1, HelpText =
            "支付年月: 格式 YYYYMM, 如 201901",
            Required = true, MetaName = "date")]
        public string Date { get; set; }

        class Item
        {
            public string region, name, idCard, type;
            public int? yearMonth, startDate, endDate;
            public decimal amount;
        }

        public void Execute()
        {
            var items = new List<Item>();

            decimal total = 0;
            var typeCH = JbOtherPayment.Name(Type);

            Session.Use(session =>
            {
                session.SendService(new OtherPaymentQuery(Type, Date));
                var result = session.GetResult<JbOtherPayment>();
                result.Data.ForEach(payment =>
                {
                    if (!string.IsNullOrEmpty(payment.typeCH))
                    {
                        session.SendService(new OtherPaymentDetailQuery(payment.payList));
                        var result = session.GetResult<OtherPaymentDetail>();
                        result.Data.ForEach(paymentDetail =>
                        {
                            if (!string.IsNullOrEmpty(paymentDetail.region)
                                && paymentDetail.flag == "0")
                            {
                                session.SendService(new OtherPaymentPersonalDetailQuery(
                                    paymentDetail.grbh, paymentDetail.payList, paymentDetail.personalPayList));
                                var result = session.GetResult<OtherPaymentPersonalDetail>();
                                int? startDate = null, endDate = null;
                                var count = result.Count;
                                if (count > 0)
                                {
                                    startDate = result[0].date;
                                    if (count > 2)
                                        endDate = result[count - 2].date;
                                    else
                                        endDate = startDate;
                                }
                                total += paymentDetail.amount;
                                items.Add(new Item
                                {
                                    region = paymentDetail.region,
                                    name = paymentDetail.name,
                                    idCard = paymentDetail.idCard,
                                    type = typeCH,
                                    yearMonth = paymentDetail.yearMonth,
                                    startDate = startDate,
                                    endDate = endDate,
                                    amount = paymentDetail.amount
                                });
                            }
                        });
                    }
                });
            });

            var culture = new CultureInfo("zh-CN");
            items.Sort((x, y) =>
                culture.CompareInfo.Compare(x.region, y.region));

            var workbook = ExcelExtension.LoadExcel(Program.payListXlsx);
            var sheet = workbook.GetSheetAt(0);
            int startRow = 3, currentRow = 3;

            var date = DateTime.Now.ToString("yyyyMMdd");
            var dateCH = DateTime.Now.ToString("yyyy年M月d日");
            var reportDate = $"制表时间：{dateCH}";
            sheet.Cell("G2").SetValue(reportDate);

            foreach (var item in items)
            {
                var row = sheet.GetOrCopyRow(currentRow++, startRow);
                row.Cell("A").SetValue(currentRow - startRow);
                row.Cell("B").SetValue(item.region);
                row.Cell("C").SetValue(item.name);
                row.Cell("D").SetValue(item.idCard);
                row.Cell("E").SetValue(item.type);
                row.Cell("F").SetValue(item.yearMonth?.ToString());
                row.Cell("G").SetValue(item.startDate?.ToString());
                row.Cell("H").SetValue(item.endDate?.ToString());
                row.Cell("I").SetValue(item.amount); ;
            }
            var trow = sheet.GetOrCopyRow(currentRow, startRow);
            trow.Cell("C").SetValue("共计");
            trow.Cell("D").SetValue(currentRow - startRow);
            trow.Cell("H").SetValue("合计");
            trow.Cell("I").SetValue(total);
            workbook.Save(Util.StringEx.AppendToFileName(
                Program.payListXlsx, $"({typeCH}){date}"));
        }
    }

    [Verb("personList", HelpText = "正常代发人员名单导出")]
    class PersonList : ICommand
    {
        public void Execute()
        {

        }
    }
}
