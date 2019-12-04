using CommandLine;
using Yhsb.Jb.Network;
using Yhsb.Jb.Database;
using Yhsb.Util.Excel;
using Yhsb.Util.Command;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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
            Command.Parse<Fphd, Download>(args);
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
            var dlny = Util.DateTime.ConvertToDashedDate(Date);
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
                    var idcard = data.idCard;
                    var fpData = from e in context.Entity
                                 where e.IDCard == idcard &&
                                 (e.Type == "贫困人口" ||
                                 e.Type == "特困人员" ||
                                 e.Type == "全额低保人员" ||
                                 e.Type == "差额低保人员")
                                 select e;
                    if (fpData.Any())
                    {
                        WriteLine($"{currentRow - startRow + 1} {data.idCard} {data.name}");

                        var qjns = data.Yjnx - data.Sjnx;
                        if (qjns < 0) qjns = 0;
                        var record = fpData.First();

                        var row = sheet.GetOrCopyRow(currentRow++, startRow);
                        row.Cell("A").SetValue(currentRow - startRow);
                        row.Cell("B").SetValue(data.xzqh);
                        row.Cell("C").SetValue(data.name);
                        row.Cell("D").SetValue(data.idCard);
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

    [Verb("download",
        HelpText = "从业务系统下载信息核对报告表")]
    class Download : ICommand
    {
        
        [Value(0, HelpText = "报表生成日期，格式：yyyymmdd",
            Required = true)]
        public string Date { get; set; }

        public void Execute()
        {
            var saveXlsx = $@"{Program.rootDir}\信息核对报告表{Date}.xlsx";
            
            Result<Dyfh> result = null;
            Session.Use(session =>
            {
                session.SendService(new DyfhQuery());
                result = session.GetResult<Dyfh>();
            });

            if (!result.IsEmpty)
            {
                using var context = new FpRawDataContext("2019年度扶贫办民政残联历史数据");
                foreach (var data in result.Data)
                {
                    var idcard = data.idCard;
                    var fpData = from e in context.Entity
                                 where e.IDCard == idcard &&
                                 (e.Type == "贫困人口" ||
                                 e.Type == "特困人员" ||
                                 e.Type == "全额低保人员" ||
                                 e.Type == "差额低保人员")
                                 select e;
                    if (fpData.Any())
                    {
                        var record = fpData.First();
                        data.bz = "按人社厅发〔2018〕111号文办理";
                        data.fpName = record.Name;
                        data.fpType = record.Type;
                        data.fpDate = record.Date;
                    }
                }
            }

            var workbook = ExcelExtension.LoadExcel(Program.infoXlsx);
            var sheet = workbook.GetSheetAt(0);
            int startRow = 3, currentRow = 3;

            foreach (var data in result.Data)
            {
                var index = currentRow - startRow + 1;
                WriteLine($"{index} {data.idCard} {data.name} {data.bz} {data.fpType}");
                var row = sheet.GetOrCopyRow(currentRow++, startRow);
                row.Cell("A").SetValue(index);
                row.Cell("B").SetValue(data.name);
                row.Cell("C").SetValue(data.idCard);
                row.Cell("D").SetValue(data.xzqh);
                row.Cell("E").SetValue(data.payAmount);
                row.Cell("F").SetValue(data.payMonth);
                row.Cell("G").SetValue("是 [ ]");
                row.Cell("H").SetValue("否 [ ]");
                row.Cell("I").SetValue("是 [ ]");
                row.Cell("J").SetValue("否 [ ]");
                row.Cell("L").SetValue(data.bz);
                row.Cell("M").SetValue(data.fpType);
                row.Cell("N").SetValue(data.fpDate);
                row.Cell("O").SetValue(data.fpName);
            }
            workbook.Save(saveXlsx);
        }
    }

    [Verb("split",
        HelpText = "对下载的信息表分组并生成养老金计算表")]
    class Split : ICommand
    {
        [Value(0, HelpText = "报表生成日期，格式：yyyymmdd",
            Required = true)]
        public string Date { get; set; }

        [Value(1, HelpText = "开始行，从1开始",
            Required = true)]
        public int BeginRow { get; set; }

        [Value(2, HelpText = "结束行(包含)，从1开始",
            Required = true)]
        public int EndRow { get; set; }

        public void Execute()
        {
            var (year, month, _) = Util.DateTime.SplitDate(Date);

            var inXlsx = $@"{Program.rootDir}\信息核对报告表{Date}.xlsx";
            var outputDir = $@"{Program.rootDir}\{year}年{month}月待遇核定数据";

            var workbook = ExcelExtension.LoadExcel(inXlsx);
            var sheet = workbook.GetSheetAt(0);

            WriteLine("生成分组映射表");
            var map = new Dictionary<string, Dictionary<string, List<int>>>();
            for (var index = BeginRow - 1; index < EndRow; index++)
            {
                var xzqh = sheet.Cell(index, "D").Value();
                Match match = null;
                foreach (var regex in Xzqh.regex)
                {
                    match = Regex.Match(xzqh, regex);
                    if (match.Success) break;
                }
                if (match == null || !match.Success)
                {
                    throw new ApplicationException($"未匹配行政区划: {xzqh}");
                }
                else
                {
                    var xzj = match.Groups[0].Value;
                    var csq = match.Groups[1].Value;
                    if (!map.ContainsKey(xzj))
                    {
                        map[xzj] = new Dictionary<string, List<int>>();
                    }
                    if (!map[xzj].ContainsKey(csq))
                    {
                        map[xzj][csq] = new List<int>{index};
                    }
                    else
                    {
                        map[xzj][csq].Add(index);
                    }
                }
            }

            WriteLine("生成分组目录并分别生成信息核对报告表");
            if (Directory.Exists(outputDir))
            {
                Directory.Move(outputDir, outputDir + ".orig");
            }
            Directory.CreateDirectory(outputDir);

            foreach (var xzj in map.Keys)
            {
                WriteLine($"{xzj}:");
                Directory.CreateDirectory(Path.Join(outputDir, xzj));

                foreach (var csq in map[xzj].Keys)
                {
                    WriteLine($"  {csq}: {map[xzj][csq]}");
                    Directory.CreateDirectory(Path.Join(outputDir, xzj, csq));

                    var outWorkbook = ExcelExtension.LoadExcel(Program.infoXlsx);
                    var outSheet = outWorkbook.GetSheetAt(0);
                    int startRow = 3, currentRow = 3;

                    map[xzj][csq].ForEach(rowIndex =>
                    {
                        var index = currentRow - startRow + 1;
                        var inRow = sheet.Row(rowIndex);
                        
                        WriteLine(
                            $"    {index} {inRow.Cell("C").Value()} {inRow.Cell("B").Value()}");

                        var row = outSheet.GetOrCopyRow(currentRow++, startRow);
                        row.Cell("A").SetValue(index);
                        row.Cell("B").SetValue(inRow.Cell("B").Value());
                        row.Cell("C").SetValue(inRow.Cell("C").Value());
                        row.Cell("D").SetValue(inRow.Cell("D").Value());
                        row.Cell("E").SetValue(inRow.Cell("E").Value());
                        row.Cell("F").SetValue(inRow.Cell("F").Value());
                        row.Cell("G").SetValue("是 [ ]");
                        row.Cell("H").SetValue("否 [ ]");
                        row.Cell("I").SetValue("是 [ ]");
                        row.Cell("J").SetValue("否 [ ]");
                        row.Cell("L").SetValue(inRow.Cell("L").Value());
                    });

                    outWorkbook.Save(
                        Path.Join(outputDir, xzj, csq, $"{csq}信息核对报告表.xlsx"));
                }
            }

            WriteLine("\n按分组生成养老金养老金计算表");
            Session.Use(session =>
            {
                foreach (var xzj in map.Keys)
                {
                    foreach (var csq in map[xzj].Keys)
                    {
                        map[xzj][csq].ForEach(index =>
                        {
                            var row = sheet.Row(index);
                            var name = row.Cell("B").Value();
                            var idcard = row.Cell("C").Value();
                            WriteLine($"  {idcard} {name}");
                            try
                            {
                                GetPaymentReport(
                                    session, name, idcard, Path.Join(outputDir, xzj, csq));
                            }
                            catch (Exception e)
                            {
                                WriteLine($"  {idcard} {name} 获得养老金计算表岀错: {e}");
                            }
                        });
                    }
                }
            });
        }

        void GetPaymentReport(
            Session session, string name, string idcard, 
            string outdir, int retry = 3)
        {
            session.SendService(new DyfhQuery(idcard, "0"));
            var result = session.GetResult<Dyfh>();
            if (!result.IsEmpty)
            {
                session.SendService(new BankInfoQuery(idcard));
                var bankInfoResult = session.GetResult<BankInfo>();

                var payInfo = result[0].PaymentInfo;
                while (!payInfo.Success)
                {
                    if (--retry > 1)
                    {
                        payInfo = result[0].PaymentInfo;
                    }
                    else
                    {
                        throw new ApplicationException("养老金计算信息无效");
                    }
                }
                var workbook = ExcelExtension.LoadExcel(Program.payInfoXlsx);
                var sheet = workbook.GetSheetAt(0);
                sheet.Cell("A5").SetValue(payInfo.Groups[1].Value);
                sheet.Cell("B5").SetValue(payInfo.Groups[2].Value);
                sheet.Cell("C5").SetValue(payInfo.Groups[3].Value);
                sheet.Cell("F5").SetValue(payInfo.Groups[4].Value);
                sheet.Cell("H5").SetValue(payInfo.Groups[5].Value);
                sheet.Cell("K5").SetValue(payInfo.Groups[6].Value);
                sheet.Cell("A8").SetValue(payInfo.Groups[7].Value);
                sheet.Cell("B8").SetValue(payInfo.Groups[8].Value);
                sheet.Cell("C8").SetValue(payInfo.Groups[9].Value);
                sheet.Cell("E8").SetValue(payInfo.Groups[10].Value);
                sheet.Cell("F8").SetValue(payInfo.Groups[11].Value);
                sheet.Cell("G8").SetValue(payInfo.Groups[12].Value);
                sheet.Cell("H8").SetValue(payInfo.Groups[13].Value);
                sheet.Cell("I8").SetValue(payInfo.Groups[14].Value);
                sheet.Cell("J8").SetValue(payInfo.Groups[15].Value);
                sheet.Cell("K8").SetValue(payInfo.Groups[16].Value);
                sheet.Cell("L8").SetValue(payInfo.Groups[17].Value);
                sheet.Cell("A11").SetValue(payInfo.Groups[18].Value);
                sheet.Cell("B11").SetValue(payInfo.Groups[19].Value);
                sheet.Cell("C11").SetValue(payInfo.Groups[20].Value);
                sheet.Cell("D11").SetValue(payInfo.Groups[21].Value);
                sheet.Cell("E11").SetValue(payInfo.Groups[22].Value);
                sheet.Cell("F11").SetValue(payInfo.Groups[23].Value);
                sheet.Cell("G11").SetValue(payInfo.Groups[24].Value);
                sheet.Cell("H11").SetValue(payInfo.Groups[25].Value);
                sheet.Cell("I11").SetValue(payInfo.Groups[26].Value);
                sheet.Cell("J11").SetValue(payInfo.Groups[27].Value);
                sheet.Cell("K11").SetValue(payInfo.Groups[28].Value);
                sheet.Cell("L11").SetValue(payInfo.Groups[29].Value);
                sheet.Cell("H12").SetValue(
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                if (!bankInfoResult.IsEmpty)
                {
                    var bankInfo = bankInfoResult[0];
                    sheet.Cell("B15").SetValue(bankInfo.name ?? "");
                    sheet.Cell("F15").SetValue(bankInfo.BankName ?? "");
                    if (bankInfo.cardNumber != null)
                    {
                        var card = bankInfo.cardNumber;
                        var l = card.Length;
                        if (l > 7)
                        {
                            card = card.Substring(0, 3) + "".PadLeft(l - 7, '*') + 
                                card.Substring(l - 4);
                        }
                        else if (l > 4)
                        {
                            card = "".PadLeft(l - 4, '*') + card.Substring(l - 4);
                        }
                        sheet.Cell("J15").SetValue(card);
                    }
                    else
                    {
                        sheet.Cell("B15").SetValue("未绑定银行账户");
                    }
                }

                workbook.Save(
                    Path.Join(outdir, $"{name}[{idcard}]养老金计算表.xlsx"));
            }
            else
            {
                throw new ApplicationException("未查到该人员核定数据");
            }
        }
    }
}
