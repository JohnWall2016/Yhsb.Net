using System;
using CommandLine;
using Yhsb.Util.Command;
using Yhsb.Qb.Network;

namespace Yhsb.Qb.Query
{
    class Program
    {
        [App(Name = "信息查询程序")]
        static void Main(string[] args)
        {
            Command.Parse<Cbcx>(args);
        }
    }

    class Cbcx : ICommand
    {
        [Value(0, HelpText = "身份证号码",
            Required = true, MetaName = "idcard")]
        public string IdCard { get; set; }

        public void Execute()
        {
            Session.Use(session =>
            {
                session.SendInEnvelope(new SncbryQuery(IdCard));
                var (header, body) = session.GetOutEnvelope<QueryList<Sncbry>>();
                foreach (var e in body.queryList)
                {
                    Console.WriteLine($"{e.rowNO} {e.name} {e.idcard} {e.cbState} {e.sbState} {e.jfClass} {e.agency}");
                }
            });
        }
    }
}
