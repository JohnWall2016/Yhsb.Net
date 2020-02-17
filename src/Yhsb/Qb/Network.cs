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
            string host, int port, string userID, string password)
            : base(host, port, "GBK")
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
            where T : InData<T>
        {
            var inEnv = new InEnvelope<Funid, T>(
                new Funid(funid, _userID, _password), body);
            Request(inEnv.ToString());
        }

        public string ToInEnvelopeString<T>(string funid, T body)
            where T : InData<T>
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

    public sealed class FieldAttribute : Attribute
    {
        public FieldAttribute(string name = "")
        {
            Name = name;
        }

        public string Name { get; set; }

        public bool Ignore { get; set; } = false;
    }

    public class FieldData<T>
    {
        static Dictionary<string, FieldInfo> _fieldsMap = null;

        static FieldData()
        {
            if (_fieldsMap == null) LoadFieldsMap();
        }

        static void LoadFieldsMap()
        {
            _fieldsMap = new Dictionary<string, FieldInfo>();

            Type type = typeof(T);
            foreach (var field in type.GetFields())
            {
                var attr = field.GetCustomAttribute<FieldAttribute>();
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

        public Dictionary<string, FieldInfo> FieldMap
            => _fieldsMap ?? new Dictionary<string, FieldInfo>();
    }

    public interface IToXElement
    {
        XElement ToXElement();
    }

    public abstract class InData<T> : FieldData<T>, IToXElement
    {
        static readonly XNamespace nsIn = "http://www.molss.gov.cn/";

        readonly string _type;

        public XElement ToXElement()
        {
            var element = new XElement(nsIn + _type,
                new XAttribute(XNamespace.Xmlns + "int", nsIn));
            foreach (var (name, finfo) in FieldMap)
            {
                if (finfo.FieldType == typeof(string))
                {
                    var value = finfo.GetValue(this);
                    if (value != null)
                    {
                        element.Add(new XElement("para",
                            new XAttribute(name, value)));
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

    public class InSystem<T> : InData<T>
    {
        public InSystem() : base("system") { }
    }

    public class InBussiness<T> : InData<T>
    {
        public InBussiness() : base("bussiness") { }
    }

    public class InEnvelope<Header, Body>
        where Header : InData<Header>
        where Body : InData<Body>
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
                        new XElement(nsSoap + "Header", _header.ToXElement()),
                        new XElement(nsSoap + "Body", _body.ToXElement())))
                        .ToString(SaveOptions.DisableFormatting);
        }
    }

    public interface IFromXElement
    {
        void FromXElement(XElement element);
    }

    public abstract class OutData<T> : FieldData<T>, IFromXElement
    {
        public void FromXElement(XElement element)
        {
            if (element.Name == "row")
            {
                foreach (var a in element.Attributes())
                {
                    var f = GetField(a.Name.LocalName);
                    f?.SetValue(this, a.Value);
                }
            }

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
                    var attr = elem.Attributes().FirstOrDefault(
                        a => a.Name.LocalName == "name");

                    var field = GetField(attr.Value);
                    if (field != null)
                    {
                        var rsType = field.FieldType;
                        if (rsType.IsGenericType)
                        {
                            if (rsType.GetGenericTypeDefinition() == typeof(ResultSet<>))
                            {
                                var resultSet = rsType.GetConstructor(new Type[0]).Invoke(null);

                                var subType = rsType.GetGenericArguments()[0];
                                foreach (var e in elem.Elements())
                                {
                                    var obj = subType.GetConstructor(new Type[0])
                                        .Invoke(null) as IFromXElement;
                                    obj.FromXElement(e);
                                    rsType.GetMethod("Add").Invoke(resultSet, new[] { obj });
                                }
                                field.SetValue(this, resultSet);
                            }
                        }
                    }
                }
            }
        }
    }

    public class ResultSet<T> : List<T>
        where T : OutData<T>, new()
    {
    }

    public class OutHeader : OutData<OutHeader>
    {
        public string sessionID;
        public string message;
    }

    public class OutBusiness<T> : OutData<T>
    {
        public string result;

        [Field("row_count")]
        public string rowCount;

        [Field("querysql")]
        public string querySql;
    }

    public class QueryList<T> : OutBusiness<QueryList<T>>
        where T : OutData<T>, new()
    {
        [Field("querylist")]
        public ResultSet<T> queryList;
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

        public void Deconstruct(out OutHeader header, out OutBody body)
        {
            header = Header;
            body = Body;
        }
    }

    public class LoginInfo<T> : InSystem<T>
    {
        [Field("usr")]
        public string user;

        [Field("pwd")]
        public string password;

        public LoginInfo(string user, string password)
        {
            this.user = user;
            this.password = password;
        }
    }

    public class Funid : LoginInfo<Funid>
    {
        public string funid;

        public Funid(string funid, string user, string password)
            : base(user, password)
        {
            this.funid = funid;
        }
    }

    public class SncbryQuery : InBussiness<SncbryQuery>
    {
        [Field("startrow")]
        public string startRow = "1";

        [Field("row_count")]
        public string rowCount = "-1";

        [Field("pagesize")]
        public string pageSize = "500";

        [Field("clientsql")]
        public string clientSql;

        [Field("functionid")]
        public string functionID = "F27.06";

        public SncbryQuery(string idcard)
        {
            clientSql = $"( aac002 = &apos;{idcard}&apos;)";
        }
    }

    public class Sncbry : OutData<Sncbry>
    {
        [Field("rown")]
        public string rowNO;

        [Field("aac002")]
        public string idcard;

        [Field("aac003")]
        public string name;

        public string aac008;

        [Field("aab300")]
        public string agency;

        public string sac007;
        public string aac031;
    }
}