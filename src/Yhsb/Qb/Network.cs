using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

using static System.Console;

using Yhsb.Net;

namespace Yhsb.Qb.Network
{
    public class Session : HttpSocket
    {
        readonly string _userID;
        readonly string _password;
        readonly Dictionary<string, string> _cookies;

        public Session(
            string host, int port, string userID, string password)
            : base(host, port, "GBK")
        {
            _userID = userID;
            _password = password;
            _cookies = new Dictionary<string, string>();
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
            if (_cookies.Any())
            {
                request.AddHeader(
                    "Cookie", string.Join("; ", _cookies.Select(e => $"{e.Key}={e.Value}")));
            }
            return request;
        }

        HttpRequest BuildRequest(string content)
        {
            var request = CreateRequest();
            request.AddBody(content);
            return request;
        }

        public void Request(string content)
        {
            var request = BuildRequest(content);

            // WriteLine($"Request: {Encoding.GetString(request.ToArray())}");

            Write(request.ToArray());
        }

        public void SendInEnvelope<T>(T body)
            where T : InBody<T>
        {
            Request(ToInEnvelopeString(body));
        }

        public string ToInEnvelopeString<T>(T body)
            where T : InBody<T>
        {
            var inEnv = new InEnvelope<T>(body, _userID, _password);
            return inEnv.ToString();
        }

        public OutEnvelope<T> GetOutEnvelope<T>()
            where T : OutData<T>, new()
        {
            var result = ReadBody();

            // WriteLine($"GetOutEnvelope: {result}");

            return FromOutEnvelope<T>(result);
        }

        public OutEnvelope<T> FromOutEnvelope<T>(string xml)
            where T : OutData<T>, new()
        {
            var doc = XDocument.Load(new StringReader(xml));
            var outEnv = new OutEnvelope<T>(doc);
            return outEnv;
        }

        public OutEnvelope<LoginInfo> Login()
        {
            SendInEnvelope(new Login());
            var header = ReadHeader();
            if (header.TryGetValue("set-cookie", out var cookies))
            {
                cookies.ForEach(cookie =>
                {
                    var match = Regex.Match(cookie, @"([^=]+?)=(.+?);");
                    if (match.Success)
                    {
                        _cookies[match.Groups[1].Value] = match.Groups[2].Value;
                    }
                });
            }
            return FromOutEnvelope<LoginInfo>(ReadBody(header));
        }

        public void Logout()
        {

        }

        public static void Use(
            Action<Session> action, string user = "ssb")
        {
            using var session = new Session(
                _internal.Session.Host,
                _internal.Session.Port,
                _internal.Session.Users[user].ID,
                _internal.Session.Users[user].Pwd);
            session.Login();
            action(session);
            session.Logout();
        }

        public static void Use(
            Action<Session, LoginInfo> action, string user = "ssb")
        {
            using var session = new Session(
                _internal.Session.Host,
                _internal.Session.Port,
                _internal.Session.Users[user].ID,
                _internal.Session.Users[user].Pwd);
            var info = session.Login();
            action(session, info.Body);
            session.Logout();
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

        public override string ToString()
        {
            var fields = string.Join(
                ", ", FieldMap.Select((m) => $"{m.Value.Name}: {m.Value.GetValue(this)}"));
            return $"({this.GetType()}: {{ {fields} }})";
        }
    }

    public interface IToXElement
    {
        XElement ToXElement();
    }

    public abstract class InData<T> : FieldData<T>, IToXElement
    {
        static readonly XNamespace nsIn = "http://www.molss.gov.cn/";

        readonly string _type;

        public virtual XElement ToXElement()
        {
            var element = new XElement(nsIn + _type,
                new XAttribute(XNamespace.Xmlns + "in", nsIn));
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
        public InBussiness() : base("business") { }
    }

    public class InHeader : InSystem<InHeader>
    {
        [Field("usr")]
        public string user;

        [Field("pwd")]
        public string password;

        public string funid;

        public InHeader(string funid, string user, string password)
        {
            this.user = user;
            this.password = password;
            this.funid = funid;
        }
    }

    public class InBody<T> : InBussiness<T>
    {
        public string FunID { get; set; } = "";

        public InBody(string funID)
        {
            FunID = funID;
        }
    }

    public class InFunction<T> : InBody<T>
    {
        [Field("functionid")]
        public string functionID = "";

        public InFunction(string funID, string functionID) : base(funID)
        {
            this.functionID = functionID;
        }
    }
    
    public class RowQuery<T> : InFunction<T>
    {
        [Field("startrow")]
        public string startRow = "1";

        [Field("row_count")]
        public string rowCount = "-1";

        [Field("pagesize")]
        public string pageSize = "500";

        [Field("clientsql")]
        public string clientSql;

        public RowQuery(string funID, string functionID)
            : base(funID, functionID)
        {}
    }

    public class InAddFid<T> : InBody<T>
    {
        [Field("fid")]
        public string fID = "";

        public InAddFid(string funID, string fID) : base(funID)
        {
            this.fID = fID;
        }
    }

    public class Query<T> : InAddFid<T>
    {
        [Field("pagesize")]
        public string pageSize = "0";

        [Field("addsql")]
        public string addSql;

        public string begin = "0";

        public Query(string funID, string functionID)
            : base(funID, functionID)
        {}
    }

    class XmlRawTextWriter : XmlTextWriter
    {
        public XmlRawTextWriter(TextWriter writer)
            : base(writer)
        {
        }

        public override void WriteString(string text)
        {
            base.WriteRaw(text);
        }
    }

    public class InEnvelope<Body> where Body : InBody<Body>
    {
        static readonly XNamespace nsSoap =
            "http://schemas.xmlsoap.org/soap/envelope/";

        public InHeader _header;
        public Body _body;

        public InEnvelope(Body body, string user, string password)
        {
            _header = new InHeader(body.FunID, user, password);
            _body = body;
        }

        public override string ToString()
        {
            /*return new XDeclaration("1.0", "GBK", null).ToString() +
                new XDocument(
                    new XElement(nsSoap + "Envelope",
                        new XAttribute(XNamespace.Xmlns + "soap", nsSoap),
                        new XAttribute(nsSoap + "encodingStyle",
                            "http://schemas.xmlsoap.org/soap/encoding/"),
                        new XElement(nsSoap + "Header", _header.ToXElement()),
                        new XElement(nsSoap + "Body", _body.ToXElement())))
                        .ToString(SaveOptions.DisableFormatting);*/

            var w = new StringWriter();
            w.Write(new XDeclaration("1.0", "GBK", null));

            using XmlWriter xw = new XmlRawTextWriter(w);
            IXmlSerializable ser = new XElement(nsSoap + "Envelope",
                    new XAttribute(XNamespace.Xmlns + "soap", nsSoap),
                    new XAttribute(nsSoap + "encodingStyle",
                    "http://schemas.xmlsoap.org/soap/encoding/"),
                    new XElement(nsSoap + "Header", _header.ToXElement()),
                    new XElement(nsSoap + "Body", _body.ToXElement())
                );
            ser.WriteXml(xw);

            var result = w.ToString();
            return result;
        }
    }

    public interface IFromXElement
    {
        void FromXElement(XElement element);
    }

    public class CustomField
    {
        public string Value { get; set; } = default;
        
        public virtual string Name => $"{Value}";

        public override string ToString() => Name;
    }

    public static class FieldInfoExtension
    {
        public static void SetCustomValue(this FieldInfo info, object obj, string value)
        {
            if (info.FieldType.IsSubclassOf(typeof(CustomField)))
            {
                var constructor = info.FieldType.GetConstructor(new Type[0]);
                if (constructor != null)
                {
                    var field = constructor.Invoke(null) as CustomField;
                    field.Value = value;
                    info.SetValue(obj, field);
                }
            }
            else if (info.FieldType.IsAssignableFrom(typeof(string)))
            {
                info.SetValue(obj, value);
            }
        }
    }

    public class OutData<T> : FieldData<T>, IFromXElement
    {
        public string XmlData { get; private set; }

        public void FromXElement(XElement element)
        {
            XmlData = element.ToString();

            if (element.Name == "row")
            {
                foreach (var a in element.Attributes())
                {
                    var f = GetField(a.Name.LocalName);
                    f?.SetCustomValue(this, a.Value);
                }
            }

            foreach (var elem in element.Elements())
            {
                if (elem.Name == "result")
                {
                    var attr = elem.Attributes().FirstOrDefault();
                    var field = GetField(attr.Name.LocalName);
                    field?.SetCustomValue(this, attr.Value);
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
        public override string ToString()
        {
            return $"[{string.Join(", ", this)}]";
        }
    }

    public class OutHeader : OutData<OutHeader>
    {
        public string sessionID;
        public string message;
        public string username;
        public string producttype;
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
        public ResultSet<T> queryList = new ResultSet<T>();
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

    public class Login : InBody<Login>
    {
        public Login() : base(
            "F00.00.00.00|192.168.1.110|PC-20170427DGON|00-05-0F-08-1A-34")
        {
        }
    }

    public class LoginInfo : OutData<LoginInfo>
    {
        [Field("operator_name")]
        public string name;

        [Field("usr")]
        public string user;

        [Field("login_name")]
        public string loginName;

        [Field("sab090")]
        public string agencyName;

        [Field("grbhqz")]
        public string agencyCode;
    }

    public class SncbryQuery : RowQuery<SncbryQuery>
    {
        public SncbryQuery(string idcard) : base("F00.01.03", "F27.06")
        {
            clientSql = $"( aac002 = &apos;{idcard}&apos;)";
        }
    }

    /// 社会保险状态
    public class SBState : CustomField
    {
        public override string Name => Value switch
        {
            "1" => "在职",
            "2" => "退休",
            "4" => "终止",
            _ => $"未知值: {Value}"
        };
    }

    /// 参保状态
    public class CBState : CustomField
    {
        public override string Name => Value switch
        {
            "1" => "参保缴费",
            "2" => "暂停缴费",
            "3" => "终止缴费",
            _ => $"未知值: {Value}"
        };
    }

    /// 缴费人员类别
    public class JFClass : CustomField
    {
        public override string Name => Value switch
        {
            "102" => "个体缴费",
            "101" => "单位在业人员",
            _ => $"未知值: {Value}"
        };
    }

    public class Sncbry : OutData<Sncbry>
    {
        [Field("rown")]
        public string rowNO;

        [Field("aac002")]
        public string idcard;

        [Field("aac003")]
        public string name;

        [Field("aac008")]
        public SBState sbState;

        [Field("aab300")]
        public string agency;

        [Field("sac007")]
        public JFClass jfClass;

        [Field("aac031")]
        public CBState cbState;

        /// 个人编号
        [Field("sac100")]
        public string grbh;

        /// 单位编号
        [Field("sab100")]
        public string dwbh;
    }

    public class RyxxQuery : RowQuery<RyxxQuery>
    {
        /// 社保机构编码
        [Field("aab034")]
        public string agencyCode;

        public RyxxQuery(string idcard, string agencyCode) : base("F00.01.03", "F27.02")
        {
            clientSql = $"( AC01.AAC002 = &apos;{idcard}&apos;)";
            this.agencyCode = agencyCode;
        }
    }

    /// 人员信息
    public class Ryxx : OutData<Ryxx>
    {
        [Field("rown")]
        public string rowNO;

        /// 单位编号
        [Field("sab100")]
        public string dwbh;

        /// 单位名称
        [Field("aab004")]
        public string dwmc;

        /// 确认状态
        [Field("sae270")]
        public string qrzt;

        /// 个人编号
        [Field("sac100")]
        public string grbh;

        /// 个人识别号
        [Field("aac001")]
        public string pid;

        /// 公民身份号码
        [Field("aac002")]
        public string idcard;

        /// 姓名
        [Field("aac003")]
        public string name;

        /// 出生年月
        [Field("aac006")]
        public string birthDay;

        /// 参加工作日期
        [Field("aac007")]
        public string cjgzrq;

        /// 建账时间
        [Field("sic041")]
        public string jzsj;

        /// 审核状态
        [Field("sae114")]
        public string shzt;

        /// 审核人
        [Field("sae101")]
        public string shr;

        /// 审核时间
        [Field("sae102")]
        public string shsj;

        /// 审批状态
        [Field("sae113")]
        public string spzt;

        [Field("sac004")]
        public string phone;

        /// 社保机构编码
        [Field("aab034")]
        public string agencyCode;
    }

    public class CbxxQuery : Query<CbxxQuery>
    {
        /// 社保机构编码
        [Field("aab034")]
        public string agencyCode;

        public CbxxQuery(string pid, string agencyCode) : base("F27.00.01", "F27.02.01")
        {
            addSql = $"ac01.aac001 = &apos;{pid}&apos;";
            this.agencyCode = agencyCode;
        }
    }

    public class QueryResult<T>  : OutData<T>
        where T : OutData<T>, new()
    {
        public string result;

        [Field("cxjg")]
        public ResultSet<T> queryList = new ResultSet<T>();
    }

    /// 参保信息
    public class Cbxx : OutData<Cbxx>
    {
        /// 个人识别号
        [Field("aac001")]
        public string pid;

        /// 公民身份号码
        [Field("aac002")]
        public string idcard;

        /// 姓名
        [Field("aac003")]
        public string name;

        public string aae140;
    }
}