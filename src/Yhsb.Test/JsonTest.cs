using System;
using Newtonsoft.Json;
using Yhsb.Jb;

public class JsonTest
{
    public static void TestToJson()
    {
        var service = new Service(new Parameters("abc"), "efg", "1234");
        var json = JsonConvert.SerializeObject(service);
        Console.WriteLine(json);

        service = new Service(new PageParameters("abc"), "efg", "1234");
        json = JsonConvert.SerializeObject(service);
        Console.WriteLine(json);
    }
}