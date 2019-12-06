using System;
using System.Linq;
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
            Command.Parse<PayList>(args);
        }
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
            public int yearMonth;
            public int? startDate, endDate;
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
                                items.Add(new Item {
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
            

        }
    }
}
