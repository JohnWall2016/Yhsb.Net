using System.Linq;
using CommandLine;
using CommandLine.Text;
using Yhsb.Util.Command;
using Yhsb.Util.Excel;
using Yhsb.Jb.Network;
using Yhsb.Jb.Database;

using static Yhsb.Util.DateTime;
using static System.Console;

namespace Yhsb.Jb.Audit
{
    class Program
    {
        static void Main(string[] args)
        {
            Command.Parse<Audit>(args);
        }
    }

    class Audit : ICommand
    {
        [Value(0, HelpText = "起始审核时间, 例如: 20190429",
            Required = true)]
        public string StartDate { get; set; }

        [Value(0, HelpText = "截至审核时间, 例如: 20190505")]
        public string EndDate { get; set; }

        public void Execute()
        {
            var startDate = StartDate != null ? ConvertToDashedDate(StartDate) : "";
            var endDate = EndDate != null ? ConvertToDashedDate(EndDate) : "";

            WriteLine($"'{startDate}' - '{endDate}'");

            // var path = @"D:\精准扶贫\";
            // var xlsx = "批量信息变更模板.xls";

            Result<Cbsh> result = null;
            Session.Use(session =>
            {
                session.SendService(new CbshQuery(startDate, endDate));
                result = session.GetResult<Cbsh>();
            });

            if (result != null)
            {
                WriteLine($"共计 {result.Count} 条");
                if (result.Count > 0)
                {
                    foreach (var cbsh in result.Data)
                    {
                        using var context = new JzfpContext();
                        var data = from fpData in context.FPTable2019
                                   where fpData.IDCard == cbsh.idCard
                                   select fpData;
                        if (data.Any())
                        {
                            var info = data.First();
                            WriteLine(
                                $"{cbsh.idCard} {cbsh.name} {cbsh.birthDay} {info.JBClass} " +
                                $"{(info.Name != cbsh.name ? info.Name : "")}");
                        }
                        else
                        {
                            WriteLine($"{cbsh.idCard} {cbsh.name} {cbsh.birthDay}");
                        }
                    }
                }
            }
        }
    }
}
