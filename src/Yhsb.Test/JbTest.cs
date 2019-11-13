using System;
using Yhsb.Jb;

public class JbTest
{
    public static void TestService()
    {
        Console.WriteLine(
            new Service(new JfxxQuery("123456"), "abc", "123").ToJson());
    }

    public static void TestJfxxQuery()
    {
        Session.Use(session => {
            session.SendService(new JfxxQuery("430122195709247411"));
            Console.WriteLine(session.ReadBody());

            session.SendService(new JfxxQuery("430122195709247411"));
            var result = session.GetResult<Jfxx>();

            foreach (var info in result.Data)
            {
                Console.WriteLine(info.year);
                Console.WriteLine(info.type);
            }
        });
    }
}
