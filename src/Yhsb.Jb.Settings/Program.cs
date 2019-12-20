using System;
using Yhsb.Jb.Network;

using static System.Console;

namespace Yhsb.Jb.Settings
{
    class Program
    {
        static void Main(string[] args)
        {
            StopZjgz("10", "011", "001");
        }

        static void StopZjgz(
            string hkxz, string sflx, string jfdc, string jfnd = "2019")
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
                    if (zjgz.aaz289 != null)
                    {
                        session.SendService(new ZjgzcsQuery(zjgz));
                        var csResult = session.GetResult<Zjgzcs>();
                        foreach (var cs in csResult.Data)
                        {
                            WriteLine(cs.ToJson());
                            // session.SendService(new StopZjgzcsAction(cs, "201912"));
                            // var stopResult = session.GetResult();
                        }
                    }
                }
            }, user: "001");
        }
    }
}
