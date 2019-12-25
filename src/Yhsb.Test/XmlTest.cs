using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using static System.Console;

public class XmlTest
{
    public static void TestXml()
    {
        var element = new SncbryQuery().ToElement();
        WriteLine(element);

        WriteLine(new LoginInfo().ToElement());
        WriteLine(new Funid().ToElement());

        WriteLine(new InEnvelope<Funid, SncbryQuery>(
            new Funid(), new SncbryQuery()));

        var doc = XDocument.Load(new StringReader(xmlResult));
        var outEnv = new OutEnvelope<QueryList<Sncbry>>(doc);

        WriteLine(outEnv.Header.sessionID);
        WriteLine(outEnv.Body.row_count);
        WriteLine(outEnv.Body.querysql);

        foreach (var ry in outEnv.Body.querylist)
        {
            WriteLine(ry.aac002);
        }
    }

    public class ResultSet<T> : List<T>
        where T : new()
    {
    }

    public abstract class InData
    {
        static readonly XNamespace nsIn = "http://www.molss.gov.cn/";

        readonly string _type;

        public XElement ToElement()
        {
            var element = new XElement(nsIn + _type,
                new XAttribute(XNamespace.Xmlns + "int", nsIn));
            foreach (var minfo in GetType().GetFields())
            {
                if (minfo.FieldType == typeof(string))
                {
                    var value = minfo.GetValue(this);
                    if (value != null)
                    {
                        element.Add(new XElement("para",
                            new XAttribute(minfo.Name, value)));
                    }
                }
            }
            return element;
        }

        public InData(string type)
        {
            _type = type;
        }
    }

    public class InSystem : InData
    {
        public InSystem() : base("system") {}
    }

    public class InBussiness : InData
    {
        public InBussiness() : base("bussiness") {}
    }

    public class InEnvelope<Header, Body> 
        where Header : InData
        where Body : InData
    {
        static readonly XNamespace nsSoap = 
            "http://schemas.xmlsoap.org/soap/envelope/";

        public Header _header;
        public Body _body;

        public InEnvelope(Header header, Body body)
        {
            _header = header;
            _body = body;
        }

        public override string ToString()
        {
            return new XDeclaration("1.0", "GBK", null).ToString() +
                new XDocument(
                    new XElement(nsSoap + "Envelope",
                        new XAttribute(XNamespace.Xmlns + "soap", nsSoap),
                        new XAttribute(nsSoap + "encodingStyle", 
                            "http://schemas.xmlsoap.org/soap/encoding/"),
                        new XElement(nsSoap + "Header", _header.ToElement()),
                        new XElement(nsSoap + "Body", _body.ToElement())))
                        .ToString(SaveOptions.DisableFormatting);
        }
    }

    public abstract class OutData
    {
        public void FromXElement(XElement element)
        {
            var type = GetType();
            foreach(var elem in element.Elements())
            {
                if (elem.Name == "result")
                {
                    var attr = elem.Attributes().FirstOrDefault();
                    var field = type.GetField(attr.Name.LocalName);
                    field?.SetValue(this, attr.Value);
                }
                else if (elem.Name == "resultset")
                {
                    var attr = elem.Attributes().FirstOrDefault();
                    if (attr.Name.LocalName == "name")
                    {
                        var field = type.GetField(attr.Value);
                        if (field != null)
                        {
                            var rsType = field.FieldType;
                            if (rsType.IsGenericType)
                            {
                                if (rsType.GetGenericTypeDefinition() == typeof(ResultSet<>))
                                {
                                    var resultSet = rsType.GetConstructor(
                                        new System.Type[0]).Invoke(null);

                                    var subType = rsType.GetGenericArguments()[0];
                                    foreach (var e in elem.Elements())
                                    {
                                        var obj = subType.GetConstructor(
                                            new System.Type[0]).Invoke(null);
                                        foreach (var a in e.Attributes())
                                        {
                                            var f = subType.GetField(a.Name.LocalName);
                                            f?.SetValue(obj, a.Value);
                                        }
                                        rsType.GetMethod("Add").Invoke(resultSet, new []{obj});
                                    }
                                    field.SetValue(this, resultSet);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public class OutHeader : OutData
    {
        public string sessionID;
        public string message;
    }

    public class OutBusiness : OutData
    {
        public string result;
        public string row_count;
        public string querysql;
    }

    public class QueryList<T> : OutBusiness
        where T : new()
    {
        public ResultSet<T> querylist;
    }

    public class OutEnvelope<OutBody>
        where OutBody : OutData, new()
    {
        public OutHeader Header { get; }
        public OutBody Body { get; }

        public OutEnvelope(XDocument doc)
        {
            XNamespace nsSoap = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace nsOut = "http://www.molss.gov.cn/";

            var envelope = doc.Element(nsSoap + "Envelope");

            var header = envelope.Element(nsSoap + "Header");
            Header = new OutHeader();
            Header.FromXElement(header);

            var body = envelope.Element(nsSoap + "Body");
            var @out = body.Element(nsOut + "business");
            Body = new OutBody();
            Body.FromXElement(@out);
        }
    }

    public class LoginInfo : InSystem
    {
        public string usr = "hqm";
        public string pwd = "YLZ_A2A5F63315129CB2998A0E0FCE31BA51";
    }

    public class Funid : LoginInfo
    {
        public string funid = "F00.01.03";
    }

    public class SncbryQuery : InBussiness
    {
        public string startrow = "1";
        public string row_count = "-1";
        public string pagesize = "500";
        public string clientsql = "( aac002 = &apos;430302195806251012&apos;)";
        public string functionid = "F27.06";
    }

    public class Sncbry
    {
        public string aac003;
        public string rown;
        public string aac008;
        public string aab300;
        public string sac007;
        public string aac031;
        public string aac002;
    }
    
    const string xmlPara =
@"<?xml version=""1.0"" encoding=""GBK""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" soap:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
  <soap:Header>
    <in:system xmlns:in=""http://www.molss.gov.cn/"">
      <para usr=""hqm""/>
      <para pwd=""YLZ_A2A5F63315129CB2998A0E0FCE31BA51""/>
      <para funid=""F00.01.03""/>
    </in:system>
  </soap:Header>
  <soap:Body>
    <in:business xmlns:in=""http://www.molss.gov.cn/"">
      <para startrow=""1""/>
      <para row_count=""-1""/>
      <para pagesize=""500""/>
      <para clientsql=""( aac002 = &apos;430302195806251012&apos;)""/>
      <para functionid=""F27.06""/>
    </in:business>
  </soap:Body>
</soap:Envelope>";

    const string xmlResult = 
@"<?xml version=""1.0"" encoding=""GBK""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" soap:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
  <soap:Header>
    <result sessionID=""DpPZb8mZ0qgv08kN26LyKmm1yDz4nn7QvXxh2VD32vDvgvQ2zw14!-23337339!1530701497001""/>
    <result message=""""/>
  </soap:Header>
  <soap:Body>
    <out:business xmlns:out=""http://www.molss.gov.cn/"">
      <result result="""" />
      <resultset name=""querylist"">
        <row aac003=""徐X"" rown=""1"" aac008=""2"" aab300=""XXXXXXX服务局"" sac007=""101"" aac031=""3"" aac002=""43030219XXXXXXXXXX"" />
      </resultset>
      <result row_count=""1"" />
      <result querysql=""select * from 
  from ac01_css a, ac02_css b
 where a.aac001 = b.aac001) where ( aac002 = &apos;43030219XXXXXXXX&apos;) and 1=1) row_ where rownum &lt;(501)) where rown &gt;=(1) "" />
    </out:business>
  </soap:Body>
</soap:Envelope>";

}