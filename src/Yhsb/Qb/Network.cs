using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System;
using System.Reflection;

using static System.Console;

using Yhsb.Net;

namespace Yhsb.Qb.Network
{
    public class Session : HttpSocket
    {
        readonly string _userID;
        readonly string _password;
        string _sessionID;

        public Session(
            string host, int port, string userID, string password) : base(host, port, "GBK")
        {
            _userID = userID;
            _password = password;
        }

        HttpRequest CreateRequest()
        {
            var request = new HttpRequest("/sbzhpt/MainServlet", method: "POST");
            request
                .AddHeader("SOAPAction", "mainservlet")
                .AddHeader("Content-Type", "text/html;charset=GBK")
                .AddHeader("Host", Url)
                .AddHeader("Connection", "Keep-Alive")
                .AddHeader("Cache-Control", "no-cache");
            if (_sessionID != null)
            {
                request.AddHeader(
                    "Cookie", $"JSESSIONID={_sessionID}");
            }
            return request;
        }

        HttpRequest BuildRequest(string content)
        {
            var request = CreateRequest();
            request.AddBody(content);
            return request;
        }

        void Request(string content)
        {
            var request = BuildRequest(content);
            Write(request.ToArray());
        }

        public void SendInEnvelope<T>(string funid, T body)
            where T : InData
        {
            var inEnv = new InEnvelope<Funid, T>(
                new Funid(funid, _userID, _password), body);
            Request(inEnv.ToString());
        }

        public string ToInEnvelopeString<T>(string funid, T body)
            where T : InData
        {
            var inEnv = new InEnvelope<Funid, T>(
                new Funid(funid, _userID, _password), body);
            return inEnv.ToString();
        }

        public OutEnvelope<T> GetOutEnvelope<T>()
            where T : OutData<T>, new()
        {
            var result = ReadBody();
            var doc = XDocument.Load(new StringReader(result));
            var outEnv = new OutEnvelope<T>(doc);
            return outEnv;
        }
    }

    public class ResultSet<T> : List<T>
        where T : OutData<T>, new()
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
        public InSystem() : base("system") { }
    }

    public class InBussiness : InData
    {
        public InBussiness() : base("bussiness") { }
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

    public sealed class FieldProperty : Attribute
    {
        public FieldProperty(string name = "")
        {
            Name = name;
        }

        public string Name { get; set; }

        public bool Ignore { get; set; } = false;
    }

    public class FieldsData<T>
    {
        static Dictionary<string, FieldInfo> _fieldsMap = null;

        static FieldsData()
        {
            if (_fieldsMap == null) LoadFieldsMap();
        }

        static void LoadFieldsMap()
        {
            _fieldsMap = new Dictionary<string, FieldInfo>();

            Type type = typeof(T);
            foreach (var field in type.GetFields())
            {
                var attr = field.GetCustomAttribute<FieldProperty>();
                if (attr != null)
                {
                    if (!attr.Ignore)
                        _fieldsMap[attr.Name] = field;
                }
                else
                {
                    _fieldsMap[field.Name] = field;
                }
            }
        }

        protected static FieldInfo GetField(string name)
        {
            if (_fieldsMap.TryGetValue(name, out var field))
            {
                return field;
            }
            else
            {
                return null;
            }
        }
    }

    public interface IXElementSerializor
    {
        void FromXElement(XElement element);
    }

    public abstract class OutData<T> : FieldsData<T>, IXElementSerializor
    {
        public void FromXElement(XElement element)
        {
            foreach (var elem in element.Elements())
            {
                if (elem.Name == "result")
                {
                    var attr = elem.Attributes().FirstOrDefault();
                    var field = GetField(attr.Name.LocalName);
                    field?.SetValue(this, attr.Value);
                }
                else if (elem.Name == "resultset")
                {
                    var attr = elem.Attributes().FirstOrDefault(a => a.Name.LocalName == "name");

                    var field = GetField(attr.Value);
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
                                        new System.Type[0]).Invoke(null) as IXElementSerializor;
                                    obj.FromXElement(e);
                                    rsType.GetMethod("Add").Invoke(resultSet, new[] { obj });
                                }
                                field.SetValue(this, resultSet);
                            }
                        }
                    }
                }
                else if (elem.Name == "row")
                {
                    foreach (var a in elem.Attributes())
                    {
                        var f = GetField(a.Name.LocalName);
                        f?.SetValue(this, a.Value);
                    }
                }
            }
        }
    }

    public class OutHeader : OutData<OutHeader>
    {
        public string sessionID;
        public string message;
    }

    public class OutBusiness : OutData<OutBusiness>
    {
        public string result;
        public string row_count;
        public string querysql;
    }

    public class QueryList<T> : OutBusiness
        where T : OutData<T>, new()
    {
        public ResultSet<T> querylist;
    }

    public class OutEnvelope<OutBody>
        where OutBody : OutData<OutBody>, new()
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
        public string usr;
        public string pwd;

        public LoginInfo(string user, string password)
        {
            usr = user;
            pwd = password;
        }
    }

    public class Funid : LoginInfo
    {
        public string funid;

        public Funid(string funid, string user, string password)
            : base(user, password)
        {
            this.funid = funid;
        }
    }

    public class SncbryQuery : InBussiness
    {
        public string startrow = "1";
        public string row_count = "-1";
        public string pagesize = "500";
        public string clientsql;
        public string functionid = "F27.06";

        public SncbryQuery(string idcard)
        {
            clientsql = $"( aac002 = &apos;{idcard}&apos;)";
        }
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
}