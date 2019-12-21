using System;
using Yhsb.Jb.Network;

using static System.Console;

namespace Yhsb.Jb.Settings
{
    class Program
    {
        static void Main(string[] args)
        {
            // StopAndAddZjgz("20", "011", "001", "12", "9.9", "8.1");
            // StopAndAddZjgz("20", "011", "002", "12", "9.9", "8.1", test: false);
            
            // StopAndAddZjgz("20", "021", "001", "12", "9.9", "8.1", test: false);
            // StopAndAddZjgz("20", "021", "002", "12", "9.9", "8.1", test: false);

            // StopAndAddZjgz("20", "011", false);
            StopAndAddZjgz("20", "021", false);
        }

        static void StopAndAddZjgz(string hkxz, string sflx, bool test = true)
        {
            StopAndAddZjgz(hkxz, sflx, "001", "12", "9.9", "8.1", test: test);
            StopAndAddZjgz(hkxz, sflx, "002", "12", "9.9", "8.1", test: test);
            StopAndAddZjgz(hkxz, sflx, "003", "16", "13.2", "10.8", test: test);
            StopAndAddZjgz(hkxz, sflx, "004", "16", "13.2", "10.8", test: test);
            StopAndAddZjgz(hkxz, sflx, "005", "24", "19.8", "16.2", test: test);
            StopAndAddZjgz(hkxz, sflx, "006", "24", "19.8", "16.2", test: test);
            StopAndAddZjgz(hkxz, sflx, "007", "24", "19.8", "16.2", test: test);
            StopAndAddZjgz(hkxz, sflx, "008", "24", "19.8", "16.2", test: test);
            StopAndAddZjgz(hkxz, sflx, "009", "24", "19.8", "16.2", test: test);
            StopAndAddZjgz(hkxz, sflx, "010", "24", "19.8", "16.2", test: test);
            StopAndAddZjgz(hkxz, sflx, "011", "24", "19.8", "16.2", test: test);
            StopAndAddZjgz(hkxz, sflx, "012", "24", "19.8", "16.2", test: test);
            StopAndAddZjgz(hkxz, sflx, "013", "24", "19.8", "16.2", test: test);
            StopAndAddZjgz(hkxz, sflx, "014", "24", "19.8", "16.2", test: test);
        }

        static void StopAndAddZjgz(
            string hkxz, string sflx, string jfdc,
            string shbt, string sjbt, string xjbt,
            string jfnd = "2019", bool test = true)
        {
            Session.Use(session =>
            {
                session.SendService(new ZjgzQuery
                {
                    hkxz = hkxz, sflx = sflx,
                    jfdc = jfdc, jfnd = jfnd,
                });
                var result = session.GetResult<Zjgz>();
                if (!result.IsEmpty)
                {
                    var zjgz = result[0];
                    if (zjgz.gzid != null)
                    {
                        WriteLine("".PadLeft(100, '='));
                        WriteLine(zjgz.bz);

                        bool addShbt = true, addSjbt = true, addXjbt = true;

                        session.SendService(new ZjgzcsQuery(zjgz));
                        var csResult = session.GetResult<Zjgzcs>();
                        foreach (var cs in csResult.Data)
                        {
                            WriteLine("".PadLeft(100, '-'));

                            WriteLine(cs.ToJson());

                            bool delCs = false;

                            static void TestZjgzcs(
                                Zjgzcs cs, string message, ref bool del, ref bool add)
                            {
                                if (cs.ksny < 202001 && cs.zzny > 201912)
                                {
                                    WriteLine($"将终止 {message}");
                                    del = true;
                                }
                                else if (cs.ksny == 202001)
                                {
                                    WriteLine($"已添加 {message}");
                                    add = false;
                                }
                                else if (cs.zzny == 201912)
                                {
                                    WriteLine($"已终止 {message}");
                                }
                            }

                            if (cs.czxm.Value.Equals("3")) // 省级财政补贴
                            {
                                TestZjgzcs(cs, "省级财政补贴", ref delCs, ref addShbt);
                            }
                            else if (cs.czxm.Value.Equals("4")) // 市级财政补贴
                            {
                                TestZjgzcs(cs, "市级财政补贴", ref delCs, ref addSjbt);
                            }
                            else if (cs.czxm.Value.Equals("5")) // 县级财政补贴
                            {
                                TestZjgzcs(cs, "县级财政补贴", ref delCs, ref addXjbt);
                            }

                            if (delCs)
                            {
                                WriteLine("终止补贴: {0}", 
                                    session.ToServiceString(
                                    new StopZjgzcsAction(cs, "201912")));

                                if (!test)
                                {
                                    session.SendService(new StopZjgzcsAction(cs, "201912"));
                                    var stopResult = session.GetResult();
                                    WriteLine("操作结果: {0}", stopResult.message);
                                }
                            }
                        }

                        if (addShbt || addSjbt || addXjbt)
                        {
                            WriteLine("".PadLeft(100, '-'));

                            var saveZjgzcs = new SaveZjgzcsAction(zjgz);
                            if (addShbt)
                            {
                                saveZjgzcs.Add(new NewZjgzcs
                                {
                                    czxm = "3", debz = shbt,
                                    ksny = "2020-01", zzny = "2099-12"
                                });
                            }
                            if (addSjbt)
                            {
                                saveZjgzcs.Add(new NewZjgzcs
                                {
                                    czxm = "4", debz = sjbt,
                                    ksny = "2020-01", zzny = "2099-12"
                                });
                            }
                            if (addXjbt)                                
                            {
                                saveZjgzcs.Add(new NewZjgzcs
                                {
                                    czxm = "5", debz = xjbt,
                                    ksny = "2020-01", zzny = "2099-12"
                                });
                            }
                            WriteLine("新增补贴: {0}", 
                                session.ToServiceString(saveZjgzcs));

                            if (!test)
                            {
                                session.SendService(saveZjgzcs);
                                var saveResult = session.GetResult();
                                WriteLine("操作结果: {0}", saveResult.message);
                            }
                        }
                        
                        WriteLine("".PadLeft(100, '='));
                    }
                }
            }, user: "001");
        }
    }
}
