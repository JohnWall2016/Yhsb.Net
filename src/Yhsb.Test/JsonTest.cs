using Newtonsoft.Json;
using Json = Newtonsoft.Json.JsonPropertyAttribute;

public class JsonTest
{
    public class Parameters
    {
        [Json("serviceId", Required = Required.Default)]
        public readonly string serviceID;

        public Parameters(string serviceID) => this.serviceID = serviceID;
    }

    public class Service
    {

    }

    public static void TestToJson()
    {
        
    }
    
}