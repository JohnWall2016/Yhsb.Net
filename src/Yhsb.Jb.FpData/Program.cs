using CommandLine;
using Yhsb.Database;
using Yhsb.Jb.Database.Jzfp2020;
using Yhsb.Util.Excel;
using Yhsb.Util.Command;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NPOI.SS.UserModel;
using Microsoft.EntityFrameworkCore;

using static System.Console;

namespace Yhsb.Jb.FpData
{
    class Program
    {
        [App(Name = "扶贫数据导库比对程序")]
        static void Main(string[] args)
        {
            Command.Parse<Pkrk, Tkry, Csdb, Ncdb, Cjry,
                Hbdc, Scdc, Rdsf, Drjb, Jbzt, Dcsj, Sfbg>(args);
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

        static readonly string[] pkry = new[]
            {"贫困人口", "特困人员", "全额低保人员", "差额低保人员"};
        static readonly string[] cjry = new[]
            {"一二级残疾人员", "三四级残疾人员"};

        public static IEnumerable<FpRawData>
            FetchFpRawData(string month, int skip, bool onlyPkry = false)
        {
            using var db = new FpDbContext();
            IEnumerable<string> types = pkry;
            if (!onlyPkry) types = types.Concat(cjry);
            var fpRawData = from data in db.FpRawData
                            where data.Date == month &&
                                types.Contains(data.Type)
                            select data;
            foreach (var d in fpRawData.Skip(skip))
                yield return d;
        }

        public static void ExportFpData(
            string monthOrAll, string tmplXlsx, string saveXlsx)
        {
            using var db = new FpDbContext();

            WriteLine($"开始导出扶贫底册: {monthOrAll}扶贫数据=>{saveXlsx}");

            var workbook = ExcelExtension.LoadExcel(tmplXlsx);
            var sheet = workbook.GetSheetAt(0);
            int startRow = 2, currentRow = 2;

            IQueryable<Database.Jzfp2020.FpData> data = null;

            if (monthOrAll.ToUpper() == "ALL")
            {
                data = from e in db.FpHistoryData select e;
            }
            else
            {
                data = from e in db.FpMonthData
                       where e.Month == monthOrAll
                       select e;
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

            WriteLine($"结束导出扶贫底册: {monthOrAll}扶贫数据=>{saveXlsx}");
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
                    var name = row.Cell("G").Value();

                    var idcard = row.Cell("H").Value();
                    idcard = idcard.Trim();
                    if (idcard.Length < 18)
                        continue;
                    if (idcard.Length > 18)
                        idcard = idcard.Substring(0, 18).ToUpper();

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
                    var name = row.Cell("C").Value();

                    var idcard = row.Cell("D").Value();
                    idcard = idcard.Trim();
                    if (idcard.Length < 18)
                        continue;
                    if (idcard.Length > 18)
                        idcard = idcard.Substring(0, 18).ToUpper();

                    var birthDay = idcard.Substring(6, 8);
                    var xzj = row.Cell("A").Value();
                    var csq = row.Cell("B").Value();

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
                ("H", "I"),
                ("J", "K"),
                ("L", "M"),
                ("N", "O"),
                ("P", "Q"),
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
                            idcard = idcard.Trim();
                            if (idcard.Length < 18)
                                continue;
                            if (idcard.Length > 18)
                                idcard = idcard.Substring(0, 18).ToUpper();

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
                ("G", "I"),
                ("J", "K"),
                ("L", "M"),
                ("N", "O"),
                ("P", "Q"),
                ("R", "S"),
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
                            idcard = idcard.Trim();
                            if (idcard.Length < 18)
                                continue;
                            if (idcard.Length > 18)
                                idcard = idcard.Substring(0, 18).ToUpper();

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
                    idcard = idcard.Trim();
                    if (idcard.Length < 18)
                        continue;
                    if (idcard.Length > 18)
                        idcard = idcard.Substring(0, 18).ToUpper();

                    var birthDay = idcard.Substring(6, 8);
                    var xzj = row.Cell("F").Value();
                    var csq = row.Cell("G").Value();
                    var address = row.Cell("H").Value();
                    var level = row.Cell("K").Value();
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

    [Verb("hbdc", HelpText = "合并到扶贫历史数据底册")]
    class Hbdc : ICommand
    {
        [Value(0, HelpText = "数据月份, 例如: 201912",
            Required = true, MetaName = "date")]
        public string Date { get; set; }

        [Value(1, HelpText = "跳过记录数", MetaName = "skip")]
        public int Skip { get; set; } = 0;

        public void Execute()
        {
            WriteLine("开始合并扶贫数据至: 扶贫历史数据底册");

            var index = 1;
            using var db = new FpDbContext();
            IEnumerable<FpRawData> fpRawData = Program.FetchFpRawData(Date, Skip);
            foreach (var rawData in fpRawData)
            {
                WriteLine($"{index++} {rawData.NO} {rawData.Idcard} {rawData.Name}");
                if (rawData.Idcard != null)
                {
                    var fpData = from data in db.FpHistoryData
                                 where data.Idcard == rawData.Idcard
                                 select data;
                    if (fpData.Any())
                    {
                        var updated = false;
                        foreach (var data in fpData)
                        {
                            if (data.Merge(rawData))
                                updated = true;
                        }
                        if (updated)
                            db.SaveChanges();
                    }
                    else
                    {
                        var data = new FpHistoryData();
                        if (Database.Jzfp2020.FpData.Merge(data, rawData))
                        {
                            db.Add(data);
                            db.SaveChanges();
                        }
                    }
                }
            }

            WriteLine("结束合并扶贫数据至: 扶贫历史数据底册");
        }
    }

    [Verb("scdc", HelpText = "生成当月扶贫数据底册")]
    class Scdc : ICommand
    {
        [Value(0, HelpText = "数据月份, 例如: 201912",
            Required = true, MetaName = "date")]
        public string Date { get; set; }
        
        [Value(1, HelpText = "跳过记录数",  MetaName = "skip")]
        public int Skip { get; set; } = 0;

        [Value(2, HelpText = "是否清除数据表", MetaName = "clear")]
        public bool Clear { get; set; } = false;

        public void Execute()
        {
            using var db = new FpDbContext();

            if (Clear)
            {
                WriteLine($"开始清除数据表: {Date}扶贫数据底册");
                db.Delete<FpMonthData>(where: $"Month='{Date}'", printSql: true);
                WriteLine($"结束清除数据表: {Date}扶贫数据底册");
            }

            WriteLine($"开始合并扶贫数据至: {Date}扶贫数据底册");

            var index = 1;
            IEnumerable<FpRawData> fpRawData = Program.FetchFpRawData(Date, Skip, true);
            foreach (var rawData in fpRawData)
            {
                WriteLine($"{index++} {rawData.NO} {rawData.Idcard} {rawData.Name}");
                if (rawData.Idcard != null)
                {
                    var fpData = from data in db.FpMonthData
                                 where data.Idcard == rawData.Idcard &&
                                    data.Month == Date
                                 select data;
                    if (fpData.Any())
                    {
                        var updated = false;
                        foreach (var data in fpData)
                        {
                            if (data.Merge(rawData))
                                updated = true;
                        }
                        if (updated)
                            db.SaveChanges();
                    }
                    else
                    {
                        var data = new FpMonthData() { Month = Date };
                        if (Database.Jzfp2020.FpData.Merge(data, rawData))
                        {
                            db.Add(data);
                            db.SaveChanges();
                        }
                    }
                }
            }

            WriteLine($"结束合并扶贫数据至: {Date}扶贫数据底册");
        }
    }

    [Verb("rdsf", HelpText = "认定居保身份")]
    class Rdsf : ICommand
    {
        [Value(0, HelpText = "数据表月份，例如：201912, ALL",
            Required = true, MetaName = "monthOrAll")]
        public string MonthOrAll { get; set; }

        [Value(1, HelpText = "数据月份，例如：201912",
            Required = true, MetaName = "date")]
        public string Date { get; set; }

        public void Execute()
        {
            using var db = new FpDbContext();

            WriteLine($"开始认定参保人员身份: {MonthOrAll}扶贫数据底册");

            IEnumerable<Database.Jzfp2020.FpData> fpData;

            if (MonthOrAll.ToUpper() == "ALL")
                fpData = from d in db.FpHistoryData
                         select d;
            else
                fpData = from d in db.FpMonthData
                         where d.Month == MonthOrAll
                         select d;

            var i = 1;
            foreach (var d in fpData)
            {
                string jbrdsf = null;
                string sypkry = null;

                if (!string.IsNullOrEmpty(d.Pkrk))
                {
                    jbrdsf = "贫困人口一级";
                    sypkry = "贫困人口";
                }
                else if (!string.IsNullOrEmpty(d.Tkry))
                {
                    jbrdsf = "特困一级";
                    sypkry = "特困人员";
                }
                else if (!string.IsNullOrEmpty(d.Qedb))
                {
                    jbrdsf = "低保对象一级";
                    sypkry = "低保对象";
                }
                else if (!string.IsNullOrEmpty(d.Yejc))
                {
                    jbrdsf = "残一级";
                }
                else if (!string.IsNullOrEmpty(d.Cedb))
                {
                    jbrdsf = "低保对象二级";
                    sypkry = "低保对象";
                }
                else if (!string.IsNullOrEmpty(d.Ssjc))
                {
                    jbrdsf = "残二级";
                }

                if (jbrdsf != null && jbrdsf != d.Jbrdsf)
                {
                    if (!string.IsNullOrEmpty(d.Jbrdsf))
                    {
                        WriteLine(
                            $"{i++} {d.Idcard} {d.Name} {jbrdsf} <= {d.Jbrdsf}");
                        d.Jbrdsf = jbrdsf;
                        d.JbrdsfLastDate = Date;
                    }
                    else
                    {
                        WriteLine($"{i++} {d.Idcard} {d.Name} {jbrdsf}");
                        d.Jbrdsf = jbrdsf;
                        d.JbrdsfFirstDate = Date;
                    }
                }

                if (sypkry != null && sypkry != d.Sypkry)
                {
                    d.Sypkry = sypkry;
                }
            }
            db.SaveChanges();

            WriteLine($"结束认定参保人员身份: {MonthOrAll}扶贫数据底册");
        }
    }

    [Verb("drjb", HelpText = "导入居保参保人员明细表")]
    class Drjb : ICommand
    {
        [Value(0, HelpText = "xlsx文件",
            Required = true, MetaName = "xslx")]
        public string Xlsx { get; set; }

        [Value(1, HelpText = "数据开始行, 从1开始",
            Required = true, MetaName = "beginRow")]
        public int BeginRow { get; set; }

        [Value(2, HelpText = "数据结束行(包含), 从1开始",
            Required = true, MetaName = "endRow")]
        public int EndRow { get; set; }

        [Value(3, HelpText = "是否清除数据表", MetaName = "clear")]
        public bool Clear { get; set; } = false;

        public void Execute()
        {
            using var db = new FpDbContext();
            // WriteLine($"{Xlsx} {BeginRow} {EndRow} {Clear}");

            if (Clear)
            {
                WriteLine("开始清除数据表: 居保参保人员明细表");
                db.DeleteAll<Jbrymx>(printSql: true);
                WriteLine("结束清除数据表: 居保参保人员明细表");
            }

            WriteLine("开始导入居保参保人员明细表");
            db.LoadExcel<Jbrymx>(Xlsx, BeginRow, EndRow,
                new List<string>
                { "E", "A", "B", "C", "F", "G", "I", "K", "L", "O" },
                printSql: true);
            WriteLine("结束导入居保参保人员明细表");
        }
    }

    [Verb("jbzt", HelpText = "更新居保参保状态")]
    class Jbzt : ICommand
    {
        [Value(0, HelpText = "数据表月份，例如：201912, ALL",
            Required = true, MetaName = "monthOrAll")]
        public string MonthOrAll { get; set; }

        [Value(1, HelpText = "居保数据日期，例如：20191231",
            Required = true, MetaName = "date")]
        public string Date { get; set; }

        static readonly List<(int cbzt, int jfzt, string jbzt)> jbztMap =
            new List<(int cbzt, int jfzt, string jbzt)>
            {
                (1, 3, "正常待遇"),
                (2, 3, "暂停待遇"),
                (4, 3, "终止参保"),
                (1, 1, "正常缴费"),
                (2, 2, "暂停缴费"),
            };

        public void Execute()
        {
            using var db = new FpDbContext();

            WriteLine($"开始更新居保状态: {MonthOrAll}扶贫数据底册");

            var jbTable = db.GetTableName<Jbrymx>();
            if (MonthOrAll.ToUpper() == "ALL")
            {
                var fpTable = db.GetTableName<FpHistoryData>();
                foreach (var (cbzt, jfzt, jbzt) in jbztMap)
                {
                    var sql =
                    $"update {fpTable}, {jbTable}\n" +
                    $"   set {fpTable}.Jbcbqk='{jbzt}',\n" +
                    $"       {fpTable}.JbcbqkDate='{Date}'\n" +
                    $" where {fpTable}.Idcard={jbTable}.Idcard\n" +
                    $"   and {jbTable}.Cbzt='{cbzt}'\n" +
                    $"   and {jbTable}.Jfzt='{jfzt}'\n";
                    db.ExecuteSql(sql, printSql: true);
                }
            }
            else
            {
                var fpTable = db.GetTableName<FpMonthData>();
                foreach (var (cbzt, jfzt, jbzt) in jbztMap)
                {
                    var sql =
                    $"update {fpTable}, {jbTable}\n" +
                    $"   set {fpTable}.Jbcbqk='{jbzt}',\n" +
                    $"       {fpTable}.JbcbqkDate='{Date}'\n" +
                    $" where {fpTable}.Month='{MonthOrAll}'\n" +
                    $"   and {fpTable}.Idcard={jbTable}.Idcard\n" +
                    $"   and {jbTable}.Cbzt='{cbzt}'\n" +
                    $"   and {jbTable}.Jfzt='{jfzt}'\n";
                    db.ExecuteSql(sql, printSql: true);
                }
            }

            WriteLine($"结束更新居保状态: {MonthOrAll}扶贫数据底册");
        }
    }

    [Verb("dcsj", HelpText = "导出扶贫底册数据")]
    class Dcsj : ICommand
    {
        [Value(0, HelpText = "数据表月份，例如：201912, ALL",
            Required = true, MetaName = "monthOrAll")]
        public string MonthOrAll { get; set; }

        public void Execute()
        {
            string fileName;

            if (MonthOrAll.ToUpper() == "ALL")
                fileName = 
                    $@"D:\精准扶贫\2020年度扶贫数据底册{Util.DateTime.FormatedDate()}.xlsx";
            else
                fileName = 
                    $@"D:\精准扶贫\{MonthOrAll}扶贫数据底册{Util.DateTime.FormatedDate()}.xlsx";

            Program.ExportFpData(
                MonthOrAll, @"D:\精准扶贫\雨湖区精准扶贫底册模板.xlsx", fileName);
        }
    }

    [Verb("sfbg", HelpText = "导出居保参保身份变更信息表")]
    class Sfbg : ICommand
    {
        [Value(0, HelpText = "导出目录",
            Required = true, MetaName = "dir")]
        public string Dir { get; set; }

        [Value(1, HelpText = "导出清除信息变更表",
            MetaName = "clear")]
        public bool Clear { get; set; } = false;

        static readonly List<(string type, string code)> jbsfMap =
            new List<(string type, string code)>
            {
                ("贫困人口一级", "051"),
                ("特困一级", "031"),
                ("低保对象一级", "061"),
                ("低保对象二级", "062"),
                ("残一级", "021"),
                ("残二级", "022"),
            };

        public void Execute()
        {
            var tmplXlsx = "D:\\精准扶贫\\批量信息变更模板.xls";
            var rowsPerXlsx = 500;

            if (!Directory.Exists(Dir))
                Directory.CreateDirectory(Dir);
            else
            {
                WriteLine($"目录已存在: {Dir}");
                return;
            }

            using var db = new FpDbContext();

            WriteLine("从 扶贫历史数据底册 和 居保参保人员明细表 导出信息变更表");

            foreach (var (type, code) in jbsfMap)
            {
                var data = from fp in db.FpHistoryData
                           from jb in db.Jbrymx
                           where fp.Idcard == jb.Idcard &&
                               fp.Jbrdsf == type &&
                               jb.Cbsf != code &&
                               jb.Cbzt == "1" &&
                               jb.Jfzt == "1"
                           select new { jb.Name, jb.Idcard, Code = code };
                if (data.Any())
                {
                    WriteLine($"开始导出 {type} 批量信息变更表");

                    int i = 0, files = 0;
                    IWorkbook workbook = null;
                    ISheet sheet = null;
                    int startRow = 1, currentRow = 1;
                    foreach (var d in data)
                    {
                        if (i++ % rowsPerXlsx == 0)
                        {
                            if (workbook != null)
                            {
                                workbook.Save(
                                    Path.Join(Dir, $"{type}批量信息变更表{++files}.xls"));
                                workbook = null;
                            }
                            if (workbook == null)
                            {
                                workbook = ExcelExtension.LoadExcel(tmplXlsx);
                                sheet = workbook.GetSheetAt(0);
                                currentRow = 1;
                            }
                        }
                        var row = sheet.GetOrCopyRow(currentRow++, startRow, false);
                        row.Cell("B").SetValue(d.Idcard);
                        row.Cell("E").SetValue(d.Name);
                        row.Cell("J").SetValue(d.Code);
                    }
                    if (workbook != null)
                    {
                        workbook.Save(Path.Join(Dir, $"{type}批量信息变更表{++files}.xls"));
                    }

                    WriteLine($"结束导出 {type} 批量信息变更表: {i} 条");
                }
            }

            if (Clear)
            {
                WriteLine("开始导出 普通参保 批量信息变更表");

                int i = 0, files = 0;
                IWorkbook workbook = null;
                ISheet sheet = null;
                int startRow = 1, currentRow = 1;
/*
                var clrData = from jb in db.Jbrymx
                              join fp in db.FpHistoryData on jb.Idcard equals fp.Idcard into JbFpData
                              from fp in JbFpData.DefaultIfEmpty()
                              //where jbfp.Jbrdsf == null &&
                              //  jb.Cbzt == "1" && jb.Jfzt == "1"
                              //  string.IsNullOrEmpty(fp.Jbrdsf)
                              //from jbfp in JbfpTable
                              select new { fp.Name, fp.Idcard, Code = "011" };
                //clrData.FromSql("")
*/
                var clearData = from jb in db.Jbrymx
                                where jb.Cbsf != "011" && jb.Cbzt == "1" && jb.Jfzt == "1"
                                select new { jb.Name, jb.Idcard };

                using var db2 = new FpDbContext();
                foreach (var d in clearData)
                {
                    var inData = from fp in db2.FpHistoryData
                                 where fp.Idcard == d.Idcard
                                 select fp;
                    if (!inData.Any())
                    {
                        WriteLine($"{i + 1} {d.Idcard} {d.Name}");

                        if (i++ % rowsPerXlsx == 0)
                        {
                            if (workbook != null)
                            {
                                workbook.Save(
                                    Path.Join(Dir, $"普通参保批量信息变更表{++files}.xls"));
                                workbook = null;
                            }
                            if (workbook == null)
                            {
                                workbook = ExcelExtension.LoadExcel(tmplXlsx);
                                sheet = workbook.GetSheetAt(0);
                                currentRow = 1;
                            }
                        }
                        var row = sheet.GetOrCopyRow(currentRow++, startRow, false);
                        row.Cell("B").SetValue(d.Idcard);
                        row.Cell("E").SetValue(d.Name);
                        row.Cell("J").SetValue("011");
                    }
                }
                if (workbook != null)
                    workbook.Save(
                        Path.Join(Dir, $"普通参保批量信息变更表{++files}.xls"));

                WriteLine($"结束导出 普通参保 批量信息变更表: {i} 条");
            }
        }
    }
}
