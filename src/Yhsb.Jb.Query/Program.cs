using System;
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

        public void Execute()
        {
            Session.Use(session =>
            {
                session.SendService(new JfxxQuery(IDCard));
                var result = session.GetResult<Jb.Jfxx>();

                foreach (var info in result.Data)
                {
                    Console.WriteLine(info.ToJson());
                }
            });
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
