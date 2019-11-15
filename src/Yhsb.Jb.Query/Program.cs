using System;
using System.Linq;
using System.Collections.Generic;
using CommandLine;
using Yhsb.Util.Command;

namespace Yhsb.Jb.Query
{
    class Program
    {
        static void Main(string[] args)
        {
            Command.Parse<Jfxx, Doc>(args);
        }
    }

    [Verb("jfxx", HelpText = "缴费信息查询")]
    class Jfxx : ICommand
    {
        [Option('e', "export", HelpText = "导出信息表")]
        public bool Export { get; set; }

        [Value(0, HelpText = "身份证号码",
            Required = true)]
        public string IDCard { get; set; }

        class JfxxRecord
        {
            internal int year;
            internal decimal grjf, sjbt, sqbt, xjbt, zfdj, jtbz;
            internal HashSet<string> hbrq = new HashSet<string>();
            internal HashSet<string> sbjg = new HashSet<string>();
        }

        class JfxxTotalRecord : JfxxRecord
        {
            internal decimal tatal = 0;
        }

        void GetJfxxRecords(
            Result<Jb.Jfxx> jfxx,
            Dictionary<int, JfxxRecord> payedRecords,
            Dictionary<int, JfxxRecord> unpayedRecords)
        {
            foreach (var data in jfxx.Data)
            {
                if (data.year != null)
                {
                    var records = data.IsPayedOff ? payedRecords : unpayedRecords;
                    if (!records.ContainsKey(data.year.Value))
                    {
                        records[data.year.Value] =
                            new JfxxRecord { year = data.year.Value };
                    }
                    var record = records[data.year.Value];
                    switch (data.item.Value)
                    {
                        case "1":
                            record.grjf += data.amount;
                            break;
                        case "3":
                            record.sjbt += data.amount;
                            break;
                        case "4":
                            record.sqbt += data.amount;
                            break;
                        case "5":
                            record.xjbt += data.amount;
                            break;
                        case "6":
                            record.jtbz += data.amount;
                            break;
                        case "11":
                            record.zfdj += data.amount;
                            break;
                        default:
                            Console.WriteLine(
                                $"未知缴费类型{data.item.Value}, 金额{data.amount}");
                            break;
                    };
                    record.sbjg.Add(data.agency ?? "");
                    record.hbrq.Add(data.payedOffDay ?? "");
                }
            }
        }

        List<JfxxRecord> OrderAndTotal(Dictionary<int, JfxxRecord> records)
        {
            var results = records.Values.ToList();
            results.Sort((p, n) => p.year - n.year);
            var total = new JfxxTotalRecord();
            results.ForEach((r) =>
            {
                total.grjf += r.grjf;
                total.sjbt += r.sjbt;
                total.sqbt += r.sqbt;
                total.xjbt += r.xjbt;
                total.zfdj += r.zfdj;
                total.jtbz += r.jtbz;
            });
            total.tatal =
                total.grjf + total.sjbt + total.sqbt +
                total.xjbt + total.zfdj + total.jtbz;
            results.Add(total);
            return results;
        }

        void PrintInfo(Cbxx info)
        {
            Console.WriteLine("个人信息:");
            Console.WriteLine(
                $"{info.name} {info.idCard} {info.JBState} " +
                $"{info.JBClass} {info.agency} {info.czName} " +
                $"{info.dealDate}\n"
            );
        }

        void PrintJfxxRecords(List<JfxxRecord> records, String message)
        {
            Console.WriteLine(message);

            Console.WriteLine(
                "序号".PadLeft(2) +
                "年度".PadLeft(3) +
                "个人缴费".PadLeft(6) +
                "省级补贴".PadLeft(5) +
                "市级补贴".PadLeft(5) +
                "县级补贴".PadLeft(5) +
                "政府代缴".PadLeft(5) +
                "集体补助".PadLeft(5) +
                "  社保经办机构 划拨时间");

            static string format(JfxxRecord r)
            {
                return (r is JfxxTotalRecord ? "合计" : $"{r.year}".PadLeft(4)) +
                    $"{r.grjf}".PadLeft(9) +
                    $"{r.sjbt}".PadLeft(9) +
                    $"{r.sqbt}".PadLeft(9) +
                    $"{r.xjbt}".PadLeft(9) +
                    $"{r.zfdj}".PadLeft(9) +
                    $"{r.jtbz}".PadLeft(9) +
                    (r is JfxxTotalRecord
                        ? $"   总计: {(r as JfxxTotalRecord).tatal}".PadLeft(9)
                        : $"   {string.Join('|', r.sbjg)} {string.Join('|', r.hbrq)}");
            }

            var i = 1;
            foreach (var r in records)
            {
                var title = r is JfxxTotalRecord ? "" : $"{i++}";
                Console.WriteLine($"{title}".PadLeft(3) + "  " + format(r));
            }
        }

        public void Execute()
        {
            Cbxx info = null;
            Result<Jb.Jfxx> jfxx = null;
            Session.Use(session =>
            {
                session.SendService(new CbxxQuery(IDCard));
                var infos = session.GetResult<Cbxx>();
                if (infos.IsEmpty || infos[0].Invalid) return;
                info = infos[0];
                session.SendService(new JfxxQuery(IDCard));
                var result = session.GetResult<Jb.Jfxx>();
                if (result.IsEmpty ||
                    (result.Count == 1 && result[0].year == null)) return;
                jfxx = result;
            });

            if (info == null)
            {
                Console.WriteLine("未查到参保记录");
                return;
            }

            PrintInfo(info);

            if (jfxx == null)
            {
                Console.WriteLine("未查询到缴费信息");
                return;
            }

            var payedRecords = new Dictionary<int, JfxxRecord>();
            var unpayedRecords = new Dictionary<int, JfxxRecord>();

            GetJfxxRecords(jfxx, payedRecords, unpayedRecords);

            var records = OrderAndTotal(payedRecords);
            var unrecords = OrderAndTotal(unpayedRecords);

            PrintJfxxRecords(records, "已拨付缴费历史记录:");
            if (unpayedRecords.Count > 0)
            {
                PrintJfxxRecords(unrecords, "\n未拨付补录入记录:");
            }
        }
    }

    [Verb("doc", HelpText = "档案目录生成")]
    class Doc : ICommand
    {
        [Value(0, HelpText = "xlsx文件",
            Required = true)]
        public string Xlsx { get; set; }

        public void Execute()
        {
            Console.WriteLine(Xlsx);
        }
    }
}
