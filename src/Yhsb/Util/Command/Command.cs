using System;
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
                .WithParsed(exec => 
                {
                    try 
                    {
                        exec.Execute();
                    } 
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(ex);
                    }
                });
        }

        public static ParserResult<object> Parse<T1, T2>(IEnumerable<string> args)
            where T1 : ICommand where T2 : ICommand
        {
            return Parser.Default.ParseArguments<T1, T2>(args)
                .WithParsed(exec =>
                {
                    try 
                    {
                        (exec as ICommand).Execute();
                    } 
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(ex);
                    }
                });
        }

        public static ParserResult<object> Parse<T1, T2, T3>(IEnumerable<string> args)
            where T1 : ICommand where T2 : ICommand where T3 : ICommand
        {
            return Parser.Default.ParseArguments<T1, T2, T3>(args)
                .WithParsed(exec =>
                {
                    try 
                    {
                        (exec as ICommand).Execute();
                    } 
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(ex);
                    }
                });
        }

        public static ParserResult<object> Parse<T1, T2, T3, T4>(IEnumerable<string> args)
            where T1 : ICommand where T2 : ICommand where T3 : ICommand
            where T4 : ICommand
        {
            return Parser.Default.ParseArguments<T1, T2, T3, T4>(args)
                .WithParsed(exec =>
                {
                    try 
                    {
                        (exec as ICommand).Execute();
                    } 
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(ex);
                    }
                });
        }

        public static ParserResult<object> Parse<T1, T2, T3, T4, T5>(IEnumerable<string> args)
            where T1 : ICommand where T2 : ICommand where T3 : ICommand
            where T4 : ICommand where T5 : ICommand
        {
            return Parser.Default.ParseArguments<T1, T2, T3, T4, T5>(args)
                .WithParsed(exec =>
                {
                    try 
                    {
                        (exec as ICommand).Execute();
                    } 
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(ex);
                    }
                });
        }

        public static ParserResult<object> Parse<T1, T2, T3, T4, T5, T6>(IEnumerable<string> args)
            where T1 : ICommand where T2 : ICommand where T3 : ICommand
            where T4 : ICommand where T5 : ICommand where T6 : ICommand
        {
            return Parser.Default.ParseArguments<T1, T2, T3, T4, T5, T6>(args)
                .WithParsed(exec =>
                {
                    try 
                    {
                        (exec as ICommand).Execute();
                    } 
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(ex);
                    }
                });
        }

        public static ParserResult<object> Parse<T1, T2, T3, T4, T5, T6, T7>(IEnumerable<string> args)
            where T1 : ICommand where T2 : ICommand where T3 : ICommand
            where T4 : ICommand where T5 : ICommand where T6 : ICommand
            where T7 : ICommand
        {
            return Parser.Default.ParseArguments<T1, T2, T3, T4, T5, T6, T7>(args)
                .WithParsed(exec =>
                {
                    try 
                    {
                        (exec as ICommand).Execute();
                    } 
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(ex);
                    }
                });
        }

        public static ParserResult<object> Parse<T1, T2, T3, T4, T5, T6, T7, T8>(IEnumerable<string> args)
            where T1 : ICommand where T2 : ICommand where T3 : ICommand
            where T4 : ICommand where T5 : ICommand where T6 : ICommand
            where T7 : ICommand where T8 : ICommand
        {
            return Parser.Default.ParseArguments<T1, T2, T3, T4, T5, T6, T7, T8>(args)
                .WithParsed(exec =>
                {
                    try 
                    {
                        (exec as ICommand).Execute();
                    } 
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(ex);
                    }
                });
        }

        public static ParserResult<object> Parse<T1, T2, T3, T4, T5, T6, T7, T8, T9>(IEnumerable<string> args)
            where T1 : ICommand where T2 : ICommand where T3 : ICommand
            where T4 : ICommand where T5 : ICommand where T6 : ICommand
            where T7 : ICommand where T8 : ICommand where T9 : ICommand
        {
            return Parser.Default.ParseArguments<T1, T2, T3, T4, T5, T6, T7, T8, T9>(args)
                .WithParsed(exec =>
                {
                    try 
                    {
                        (exec as ICommand).Execute();
                    } 
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(ex);
                    }
                });
        }

        public static ParserResult<object> Parse<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(IEnumerable<string> args)
            where T1 : ICommand where T2 : ICommand where T3 : ICommand
            where T4 : ICommand where T5 : ICommand where T6 : ICommand
            where T7 : ICommand where T8 : ICommand where T9 : ICommand
            where T10 : ICommand
        {
            return Parser.Default.ParseArguments<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(args)
                .WithParsed(exec =>
                {
                    try 
                    {
                        (exec as ICommand).Execute();
                    } 
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(ex);
                    }
                });
        }

        public static ParserResult<object> Parse<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(IEnumerable<string> args)
            where T1 : ICommand where T2 : ICommand where T3 : ICommand
            where T4 : ICommand where T5 : ICommand where T6 : ICommand
            where T7 : ICommand where T8 : ICommand where T9 : ICommand
            where T10 : ICommand where T11 : ICommand
        {
            return Parser.Default.ParseArguments<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(args)
                .WithParsed(exec =>
                {
                    try 
                    {
                        (exec as ICommand).Execute();
                    } 
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(ex);
                    }
                });
        }

        public static ParserResult<object> Parse<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(IEnumerable<string> args)
            where T1 : ICommand where T2 : ICommand where T3 : ICommand
            where T4 : ICommand where T5 : ICommand where T6 : ICommand
            where T7 : ICommand where T8 : ICommand where T9 : ICommand
            where T10 : ICommand where T11 : ICommand where T12 : ICommand
        {
            return Parser.Default.ParseArguments<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(args)
                .WithParsed(exec =>
                {
                    try 
                    {
                        (exec as ICommand).Execute();
                    } 
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(ex);
                    }
                });
        }

        public static ParserResult<object> Parse<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(IEnumerable<string> args)
            where T1 : ICommand where T2 : ICommand where T3 : ICommand
            where T4 : ICommand where T5 : ICommand where T6 : ICommand
            where T7 : ICommand where T8 : ICommand where T9 : ICommand
            where T10 : ICommand where T11 : ICommand where T12 : ICommand
            where T13 : ICommand
        {
            return Parser.Default.ParseArguments<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(args)
                .WithParsed(exec =>
                {
                    try 
                    {
                        (exec as ICommand).Execute();
                    } 
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(ex);
                    }
                });
        }

        public static ParserResult<object> Parse<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(IEnumerable<string> args)
            where T1 : ICommand where T2 : ICommand where T3 : ICommand
            where T4 : ICommand where T5 : ICommand where T6 : ICommand
            where T7 : ICommand where T8 : ICommand where T9 : ICommand
            where T10 : ICommand where T11 : ICommand where T12 : ICommand
            where T13 : ICommand where T14 : ICommand
        {
            return Parser.Default.ParseArguments<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(args)
                .WithParsed(exec =>
                {
                    try 
                    {
                        (exec as ICommand).Execute();
                    } 
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(ex);
                    }
                });
        }

        public static ParserResult<object> Parse<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(IEnumerable<string> args)
            where T1 : ICommand where T2 : ICommand where T3 : ICommand
            where T4 : ICommand where T5 : ICommand where T6 : ICommand
            where T7 : ICommand where T8 : ICommand where T9 : ICommand
            where T10 : ICommand where T11 : ICommand where T12 : ICommand
            where T13 : ICommand where T14 : ICommand where T15 : ICommand
        {
            return Parser.Default.ParseArguments<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(args)
                .WithParsed(exec =>
                {
                    try 
                    {
                        (exec as ICommand).Execute();
                    } 
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(ex);
                    }
                });
        }

        public static ParserResult<object> Parse<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(IEnumerable<string> args)
            where T1 : ICommand where T2 : ICommand where T3 : ICommand
            where T4 : ICommand where T5 : ICommand where T6 : ICommand
            where T7 : ICommand where T8 : ICommand where T9 : ICommand
            where T10 : ICommand where T11 : ICommand where T12 : ICommand
            where T13 : ICommand where T14 : ICommand where T15 : ICommand
            where T16 : ICommand
        {
            return Parser.Default.ParseArguments<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(args)
                .WithParsed(exec =>
                {
                    try 
                    {
                        (exec as ICommand).Execute();
                    } 
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(ex);
                    }
                });
        }
    }
}
