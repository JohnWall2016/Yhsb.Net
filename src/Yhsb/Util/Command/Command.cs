using CommandLine;
using CommandLine.Text;
using System.Collections.Generic;

namespace Yhsb.Util.Command
{
    public interface ICommand
    {
        void Execute();
    }

    public class Command
    {
        static Command()
        {
            SentenceBuilder.Factory = () =>
                new LocalizableSentenceBuilder();
        }

        public static ParserResult<T> Parse<T>(IEnumerable<string> args)
            where T : ICommand
        {
            return Parser.Default.ParseArguments<T>(args)
                .WithParsed(exec => exec.Execute());
        }

        public static ParserResult<object> Parse<T1, T2>(IEnumerable<string> args)
            where T1 : ICommand where T2 : ICommand
        {
            return Parser.Default.ParseArguments<T1, T2>(args)
                .WithParsed(exec => (exec as ICommand).Execute());
        }

        public static ParserResult<object> Parse<T1, T2, T3>(IEnumerable<string> args)
            where T1 : ICommand where T2 : ICommand where T3 : ICommand
        {
            return Parser.Default.ParseArguments<T1, T2, T3>(args)
                .WithParsed(exec => (exec as ICommand).Execute());
        }

        public static ParserResult<object> Parse<T1, T2, T3, T4>(IEnumerable<string> args)
            where T1 : ICommand where T2 : ICommand where T3 : ICommand
            where T4 : ICommand
        {
            return Parser.Default.ParseArguments<T1, T2>(args)
                .WithParsed(exec => (exec as ICommand).Execute());
        }

        public static ParserResult<object> Parse<T1, T2, T3, T4, T5>(IEnumerable<string> args)
            where T1 : ICommand where T2 : ICommand where T3 : ICommand
            where T4 : ICommand where T5 : ICommand
        {
            return Parser.Default.ParseArguments<T1, T2, T3, T4, T5>(args)
                .WithParsed(exec => (exec as ICommand).Execute());
        }

        public static ParserResult<object> Parse<T1, T2, T3, T4, T5, T6>(IEnumerable<string> args)
            where T1 : ICommand where T2 : ICommand where T3 : ICommand
            where T4 : ICommand where T5 : ICommand where T6 : ICommand
        {
            return Parser.Default.ParseArguments<T1, T2, T3, T4, T5, T6>(args)
                .WithParsed(exec => (exec as ICommand).Execute());
        }
    }
}
