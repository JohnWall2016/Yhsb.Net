using System;
using System.Linq;
using System.Collections.Generic;
using CommandLine;
using Yhsb.Util.Command;
using Yhsb.Util.Excel;
using Yhsb.Jb.Network;
using JbJfxx = Yhsb.Jb.Network.Jfxx;

using static System.Console;

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
            internal decimal total = 0;
        }

        void GetJfxxRecords(
            Result<JbJfxx> jfxx,
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
                            WriteLine(
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
            total.total =
                total.grjf + total.sjbt + total.sqbt +
                total.xjbt + total.zfdj + total.jtbz;
            results.Add(total);
            return results;
        }

        void PrintInfo(Cbxx info)
        {
            WriteLine("个人信息:");
            WriteLine(
                $"{info.name} {info.idCard} {info.JBState} " +
                $"{info.JBClass} {info.agency} {info.czName} " +
                $"{info.dealDate}\n"
            );
        }

        void PrintJfxxRecords(List<JfxxRecord> records, String message)
        {
            WriteLine(message);

            WriteLine(
                $"{"序号",2}{"年度",3}{"个人缴费",6}{"省级补贴",5}" +
                $"{"市级补贴",5}{"县级补贴",5}{"政府代缴",5}{"集体补助",5}" +
                "  社保经办机构 划拨时间");

            static string format(JfxxRecord r)
            {
                return (r is JfxxTotalRecord ? "合计" : $"{r.year,4}") +
                    $"{r.grjf,9}{r.sjbt,9}{r.sqbt,9}{r.xjbt,9}{r.zfdj,9}{r.jtbz,9}   " +
                    (r is JfxxTotalRecord ? $"总计: {(r as JfxxTotalRecord).total,9}" 
                        : $"{string.Join('|', r.sbjg)} {string.Join('|', r.hbrq)}");
            }

            var i = 1;
            foreach (var r in records)
            {
                WriteLine(
                    $"{(r is JfxxTotalRecord ? "" : $"{i++}"),3}  {format(r)}");
            }
        }

        public void Execute()
        {
            Cbxx info = null;
            Result<JbJfxx> jfxx = null;
            Session.Use(session =>
            {
                session.SendService(new CbxxQuery(IDCard));
                var infos = session.GetResult<Cbxx>();
                if (infos.IsEmpty || infos[0].Invalid) return;
                info = infos[0];
                session.SendService(new JfxxQuery(IDCard));
                var result = session.GetResult<JbJfxx>();
                if (result.IsEmpty ||
                    (result.Count == 1 && result[0].year == null)) return;
                jfxx = result;
            });

            if (info == null)
            {
                WriteLine("未查到参保记录");
                return;
            }

            PrintInfo(info);

            List<JfxxRecord> records = null;
            List<JfxxRecord> unrecords = null;

            if (jfxx == null)
            {
                WriteLine("未查询到缴费信息");
            }
            else
            {
                var payedRecords = new Dictionary<int, JfxxRecord>();
                var unpayedRecords = new Dictionary<int, JfxxRecord>();

                GetJfxxRecords(jfxx, payedRecords, unpayedRecords);

                records = OrderAndTotal(payedRecords);
                unrecords = OrderAndTotal(unpayedRecords);

                PrintJfxxRecords(records, "已拨付缴费历史记录:");
                if (unpayedRecords.Count > 0)
                {
                    PrintJfxxRecords(unrecords, "\n未拨付补录入记录:");
                }
            }

            if (Export)
            {
                var path = @"D:\征缴管理";
                var xlsx = $@"{path}\雨湖区城乡居民基本养老保险缴费查询单模板.xlsx";
                var workbook = ExcelExtension.LoadExcel(xlsx);
                var sheet = workbook.GetSheetAt(0);
                sheet.Cell("A5").SetValue(info.name);
                sheet.Cell("C5").SetValue(info.idCard);
                sheet.Cell("E5").SetValue(info.agency);
                sheet.Cell("G5").SetValue(info.czName);
                sheet.Cell("K5").SetValue(info.dealDate);

                if (records != null)
                {
                    int index = 8, copyIndex = 8;
                    foreach (var r in records)
                    {
                        var row = sheet.GetOrCopyRow(index++, copyIndex);
                        row.Cell("A").SetValue(
                            r is JfxxTotalRecord ? "" : $"{index - copyIndex}");
                        row.Cell("B").SetValue(
                            r is JfxxTotalRecord ? "合计" : $"{r.year}");
                        row.Cell("C").SetValue(r.grjf);
                        row.Cell("D").SetValue(r.sjbt);
                        row.Cell("E").SetValue(r.sqbt);
                        row.Cell("F").SetValue(r.xjbt);
                        row.Cell("G").SetValue(r.zfdj);
                        row.Cell("H").SetValue(r.jtbz);
                        row.Cell("I").SetValue(
                            r is JfxxTotalRecord ? "总计" : string.Join('|', r.sbjg));
                        row.Cell("K").SetValue(
                            r is JfxxTotalRecord ? (r as JfxxTotalRecord).total.ToString() 
                                : string.Join('|', r.hbrq));
                    }
                }
                workbook.Save($@"{path}\{info.name}缴费查询单.xlsx");
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
            WriteLine(Xlsx);
        }
    }
}
