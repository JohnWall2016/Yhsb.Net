using System;
using System.IO;
using Newtonsoft.Json;
using Yhsb.Jb.Network;
using Yhsb.Json;

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

    public class JBType : JsonField
    {
        public override string Name => Value switch
        {
            "10" => "正常应缴",
            "31" => "补缴",
            _ => $"未知值: {Value}"
        };
    }

    public class Person
    {
        public string name;

        [JsonProperty("abc001")]
        public string idcard;

        public JBType type;

        public Person(string name, string idcard, string type)
        {
            this.name = name;
            this.idcard = idcard;
            this.type = new JBType
            {
                Value = type
            };
        }
    }

    public static void TestJsonField()
    {
        var json = JsonExtension.Serialize(
            new Person("Jack", "3252352352", "10"));
        Console.WriteLine(json);

        var person = JsonExtension.Deserialize<Person>(json);
        Console.WriteLine(
            $"{person.name} {person.idcard} {person.type} {person.type.Value}");

        Console.WriteLine(JsonExtension.Serialize(person, true));
    }
}