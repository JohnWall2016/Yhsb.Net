using CommandLine;
using Yhsb.Jb.Database.Jzfp2020;
using Yhsb.Util.Excel;
using Yhsb.Util.Command;
using System.Linq;
using System.Collections.Generic;

using static System.Console;

namespace Yhsb.Jb.FpData
{
    class Program
    {
        [App(Name = "扶贫数据导库比对程序")]
        static void Main(string[] args)
        {
            Command.Parse<Pkrk, Tkry, Csdb, Ncdb, Dcsj, Cjry>(args);
        }

        public static void ImportFpRawData(IEnumerable<FpRawData> records)
        {
            var index = 1;
            using var context = new FpDbContext();
            foreach (var record in records)
            {
                WriteLine($"{index++} {record.Idcard} {record.Name} {record.Type}");
                if (!string.IsNullOrEmpty(record.Idcard))
                {
                    var fpData = from e in context.FpRawData
                                 where e.Idcard == record.Idcard &&
                                    e.Type == record.Type &&
                                    e.Date == record.Date
                                 select e;
                    if (fpData.Any())
                    {
                        foreach (var data in fpData)
                        {
                            record.NO = data.NO;
                            context.Entry(data)
                                .CurrentValues.SetValues(record);
                        }
                    }
                    else
                    {
                        context.Add(record);
                    }
                    context.SaveChanges();
                }
            }
        }

        public static IEnumerable<Database.Jzfp2020.FpData>
            FetchFpRawData(string month, bool onlyPkry = false)
        {
            using var db = new FpDbContext();
            var fpRawData = from data in db.FpRawData
                where data.Date == month select data;
            foreach (var d in fpRawData)
            {
                // TODO(wj): 
            }
            return null;
        }

        public static void ExportFpData(string month, string tmplXlsx, string saveXlsx)
        {
            using var db = new FpDbContext();

            WriteLine($"开始导出扶贫底册: {month}扶贫数据=>{saveXlsx}");

            var workbook = ExcelExtension.LoadExcel(tmplXlsx);
            var sheet = workbook.GetSheetAt(0);
            int startRow = 2, currentRow = 2;

            IQueryable<Database.Jzfp2020.FpData> data = null;

            if (month.ToUpper() == "ALL")
            {
                data = from e in db.FpHistoryData select e;
            }
            else
            {
                data = from e in db.FpMonthData
                    where e.Month == month select e;
            }

            foreach (var d in data)
            {
                var index = currentRow - startRow + 1;

                WriteLine($"{index} {d.Idcard} {d.Name}");

                var row = sheet.GetOrCopyRow(currentRow++, startRow);

                row.Cell("A").SetValue(index);
                row.Cell("B").SetValue(d.NO);
                row.Cell("C").SetValue(d.Xzj);
                row.Cell("D").SetValue(d.Csq);
                row.Cell("E").SetValue(d.Address);
                row.Cell("F").SetValue(d.Name);
                row.Cell("G").SetValue(d.Idcard);
                row.Cell("H").SetValue(d.BirthDay);
                row.Cell("I").SetValue(d.Pkrk);
                row.Cell("J").SetValue(d.PkrkDate);
                row.Cell("K").SetValue(d.Tkry);
                row.Cell("L").SetValue(d.TkryDate);
                row.Cell("M").SetValue(d.Qedb);
                row.Cell("N").SetValue(d.QedbDate);
                row.Cell("O").SetValue(d.Cedb);
                row.Cell("P").SetValue(d.CedbDate);
                row.Cell("Q").SetValue(d.Yejc);
                row.Cell("R").SetValue(d.YejcDate);
                row.Cell("S").SetValue(d.Ssjc);
                row.Cell("T").SetValue(d.SsjcDate);
                row.Cell("U").SetValue(d.Sypkry);
                row.Cell("V").SetValue(d.Jbrdsf);
                row.Cell("W").SetValue(d.JbrdsfFirstDate);
                row.Cell("X").SetValue(d.JbrdsfLastDate);
                row.Cell("Y").SetValue(d.Jbcbqk);
                row.Cell("Z").SetValue(d.JbcbqkDate);
            }

            workbook.Save(saveXlsx);

            WriteLine($"结束导出扶贫底册: {month}扶贫数据=>{saveXlsx}");
        }
    }

    abstract class ImportCommand : ICommand
    {
        [Value(0, HelpText = "数据月份, 例如: 201912",
            Required = true, MetaName = "date")]
        public string Date { get; set; }

        [Value(1, HelpText = "xlsx文件",
            Required = true, MetaName = "xslx")]
        public string Xlsx { get; set; }

        [Value(2, HelpText = "数据开始行, 从1开始",
            Required = true, MetaName = "beginRow")]
        public int BeginRow { get; set; }

        [Value(3, HelpText = "数据结束行(包含), 从1开始",
            Required = true, MetaName = "endRow")]
        public int EndRow { get; set; }

        public void Execute() => Program.ImportFpRawData(Fetch());

        protected abstract IEnumerable<FpRawData> Fetch();
    }

    [Verb("pkrk", HelpText = "导入贫困人口数据")]
    class Pkrk : ImportCommand
    {
        protected override IEnumerable<FpRawData> Fetch()
        {
            var workbook = ExcelExtension.LoadExcel(Xlsx);
            var sheet = workbook.GetSheetAt(0);

            for (var index = BeginRow - 1; index < EndRow; index++)
            {
                var row = sheet.Row(index);
                if (row != null)
                {
                    var name = row.Cell("H").Value();
                    var idcard = row.Cell("I").Value();
                    idcard = idcard.Trim().Substring(0, 18).ToUpper();
                    var birthDay = idcard.Substring(6, 8);
                    var xzj = row.Cell("C").Value();
                    var csq = row.Cell("D").Value();

                    yield return new FpRawData
                    {
                        Name = name,
                        Idcard = idcard,
                        BirthDay = birthDay,
                        Xzj = xzj,
                        Csq = csq,
                        Type = "贫困人口",
                        Detail = "是",
                        Date = Date
                    };
                }
            }
        }
    }

    [Verb("tkry", HelpText = "导入特困人员数据")]
    class Tkry : ImportCommand
    {
        protected override IEnumerable<FpRawData> Fetch()
        {
            var workbook = ExcelExtension.LoadExcel(Xlsx);
            var sheet = workbook.GetSheetAt(0);

            for (var index = BeginRow - 1; index < EndRow; index++)
            {
                var row = sheet.Row(index);
                if (row != null)
                {
                    var name = row.Cell("G").Value();
                    var idcard = row.Cell("H").Value();
                    idcard = idcard.Trim().Substring(0, 18).ToUpper();
                    var birthDay = idcard.Substring(6, 8);
                    var xzj = row.Cell("C").Value();
                    var csq = row.Cell("D").Value();

                    yield return new FpRawData
                    {
                        Name = name,
                        Idcard = idcard,
                        BirthDay = birthDay,
                        Xzj = xzj,
                        Csq = csq,
                        Type = "特困人员",
                        Detail = "是",
                        Date = Date
                    };
                }
            }
        }
    }

    [Verb("csdb", HelpText = "导入城市低保数据")]
    class Csdb : ImportCommand
    {
        readonly List<(string name, string idcard)> colNameIdcards = 
            new List<(string name, string idcard)>
            {
                ("G", "H"),
                ("I", "J"),
                ("K", "L"),
                ("M", "N"),
                ("O", "P"),
            };

        protected override IEnumerable<FpRawData> Fetch()
        {
            var workbook = ExcelExtension.LoadExcel(Xlsx);
            var sheet = workbook.GetSheetAt(0);

            for (var index = BeginRow - 1; index < EndRow; index++)
            {
                var row = sheet.Row(index);
                if (row != null)
                {
                    var xzj = row.Cell("A").Value();
                    var csq = row.Cell("B").Value();
                    var address = row.Cell("D").Value();

                    var type = row.Cell("E").Value();
                    if (type == "全额救助" || type == "全额") 
                        type = "全额低保人员";
                    else
                        type = "差额低保人员";

                    foreach (var cols in colNameIdcards)
                    {
                        var name = row.Cell(cols.name).Value();
                        var idcard = row.Cell(cols.idcard).Value();
                        if (!string.IsNullOrEmpty(name) && 
                            !string.IsNullOrEmpty(idcard))
                        {
                            idcard = idcard.Trim().Substring(0, 18).ToUpper();
                            var birthDay = idcard.Substring(6, 8);

                            yield return new FpRawData
                            {
                                Name = name,
                                Idcard = idcard,
                                BirthDay = birthDay,
                                Xzj = xzj,
                                Csq = csq,
                                Address = address,
                                Type = type,
                                Detail = "城市",
                                Date = Date
                            };
                        }
                    }
                }
            }
        }
    }

    [Verb("ncdb", HelpText = "导入农村低保数据")]
    class Ncdb : ImportCommand
    {
        readonly List<(string name, string idcard)> colNameIdcards = 
            new List<(string name, string idcard)>
            {
                ("H", "J"),
                ("K", "L"),
                ("M", "N"),
                ("O", "P"),
                ("Q", "R"),
                ("S", "T"),
                ("U", "V"),
            };

        protected override IEnumerable<FpRawData> Fetch()
        {
            var workbook = ExcelExtension.LoadExcel(Xlsx);
            var sheet = workbook.GetSheetAt(0);

            for (var index = BeginRow - 1; index < EndRow; index++)
            {
                var row = sheet.Row(index);
                if (row != null)
                {
                    var xzj = row.Cell("A").Value();
                    var csq = row.Cell("B").Value();
                    var address = row.Cell("D").Value();

                    var type = row.Cell("F").Value();
                    if (type == "全额救助" || type == "全额") 
                        type = "全额低保人员";
                    else
                        type = "差额低保人员";

                    foreach (var cols in colNameIdcards)
                    {
                        var name = row.Cell(cols.name).Value();
                        var idcard = row.Cell(cols.idcard).Value();
                        if (!string.IsNullOrEmpty(name) && 
                            !string.IsNullOrEmpty(idcard))
                        {
                            idcard = idcard.Trim().Substring(0, 18).ToUpper();
                            var birthDay = idcard.Substring(6, 8);

                            yield return new FpRawData
                            {
                                Name = name,
                                Idcard = idcard,
                                BirthDay = birthDay,
                                Xzj = xzj,
                                Csq = csq,
                                Address = address,
                                Type = type,
                                Detail = "农村",
                                Date = Date
                            };
                        }
                    }
                }
            }
        }
    }

    [Verb("cjry", HelpText = "导入残疾人员数据")]
    class Cjry : ImportCommand
    {
        protected override IEnumerable<FpRawData> Fetch()
        {
            var workbook = ExcelExtension.LoadExcel(Xlsx);
            var sheet = workbook.GetSheetAt(0);

            for (var index = BeginRow - 1; index < EndRow; index++)
            {
                var row = sheet.Row(index);
                if (row != null)
                {
                    var name = row.Cell("A").Value();
                    var idcard = row.Cell("B").Value();
                    idcard = idcard.Trim().Substring(0, 18).ToUpper();
                    var birthDay = idcard.Substring(6, 8);
                    var xzj = row.Cell("H").Value();
                    var csq = row.Cell("I").Value();
                    var address = row.Cell("G").Value();
                    var level = row.Cell("L").Value();
                    string type;
                    switch (level)
                    {
                        case "一级":
                        case "二级":
                            type = "一二级残疾人员";
                            break;
                        case "三级":
                        case "四级":
                            type = "三四级残疾人员";
                            break;
                        default:
                            continue;
                    }

                    yield return new FpRawData
                    {
                        Name = name,
                        Idcard = idcard,
                        BirthDay = birthDay,
                        Xzj = xzj,
                        Csq = csq,
                        Address = address,
                        Type = type,
                        Detail = level,
                        Date = Date
                    };
                }
            }
        }
    }

    [Verb("dcsj", HelpText = "导出扶贫底册数据")]
    class Dcsj : ICommand
    {
        [Value(0, HelpText = "数据表月份，例如：201912, ALL",
            Required = true, MetaName = "month")]
        public string Month { get; set; }

        public void Execute()
        {
            string fileName;

            if (Month.ToUpper() == "ALL")
                fileName = $@"D:\精准扶贫\2020年度扶贫数据底册{Util.DateTime.FormatedDate()}.xlsx";
            else
                fileName = $@"D:\精准扶贫\{Month}扶贫数据底册{Util.DateTime.FormatedDate()}.xlsx";

            Program.ExportFpData(
                Month, @"D:\精准扶贫\雨湖区精准扶贫底册模板.xlsx", fileName);
        }
    }
}
