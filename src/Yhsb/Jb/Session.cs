using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Yhsb.Net;
using Yhsb.Json;

namespace Yhsb.Jb
{
    public class Session : HttpSocket
    {
        readonly string _userID;
        readonly string _password;
        string _sessionID, _cxCookie;

        public Session(
            string host, int port, string userID, string password) : base(host, port)
        {
            _userID = userID;
            _password = password;
        }

        HttpRequest CreateRequest()
        {
            var request = new HttpRequest("/hncjb/reports/crud", method: "POST");
            request
                .AddHeader("Host", Url)
                .AddHeader("Connection", "keep-alive")
                .AddHeader("Accept", "application/json, text/javascript, */*; q=0.01")
                .AddHeader("Origin", $"http://{Url}")
                .AddHeader("X-Requested-With", "XMLHttpRequest")
                .AddHeader("User-Agent",
                    "Mozilla/5.0 (Windows NT 5.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.95 Safari/537.36")
                .AddHeader("Content-Type", "multipart/form-data;charset=UTF-8")
                .AddHeader("Referer", $"http://{Url}/hncjb/pages/html/index.html")
                .AddHeader("Accept-Encoding", "gzip, deflate")
                .AddHeader("Accept-Language", "zh-CN,zh;q=0.8");
            if (_sessionID != null)
            {
                request.AddHeader(
                    "Cookie", $"jsessionid_ylzcbp={_sessionID}; cxcookie={_cxCookie}");
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

        public void SendService(Parameters parameters)
        {
            var service = new Service(parameters, _userID, _password);
            Request(service.ToJson());
        }

        public void SendService(string id)
        {
            var service = new Service(new Parameters(id), _userID, _password);
            Request(service.ToJson());
        }

        public Result<T> GetResult<T>() where T : ResultData
        {
            var result = ReadBody();
            return Result<T>.FromJson(result);
        }

        public string Login()
        {
            SendService("loadCurrentUser");
            var header = ReadHeader();
            var cookies = header["set-cookie"];
            cookies?.ForEach(cookie =>
            {
                var match = Regex.Match(cookie, @"jsessionid_ylzcbp=(.+?);");
                if (match.Success)
                {
                    _sessionID = match.Groups[1].Value;
                    return;
                }
                match = Regex.Match(cookie, @"cxcookie=(.+?);");
                if (match.Success)
                {
                    _cxCookie = match.Groups[1].Value;
                    return;
                }
            });
            ReadBody(header);

            SendService(new Syslogin(_userID, _password));
            return ReadBody();
        }

        public string Logout()
        {
            SendService("syslogout");
            return ReadBody();
        }

        public static void Use(
            Action<Session> action, string user = "002")
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
    }

    public class Parameters
    {
        [JsonIgnore]
        public string ServiceID { get; }

        public Parameters(string serviceID) => ServiceID = serviceID;
    }

    public class PageParameters : Parameters
    {
        public int page;

        [JsonProperty("pagesize")]
        public int pageSize;

        public List<Dictionary<string, string>> filtering
            = new List<Dictionary<string, string>>();

        public List<Dictionary<string, string>> sorting
            = new List<Dictionary<string, string>>();

        public List<Dictionary<string, string>> totals
            = new List<Dictionary<string, string>>();

        public PageParameters(
            string id, int page = 1, int pageSize = 15,
            List<Dictionary<string, string>> filtering = null,
            List<Dictionary<string, string>> sorting = null,
            List<Dictionary<string, string>> totals = null) : base(id)
        {
            this.page = page;
            this.pageSize = pageSize;
            if (filtering != null) this.filtering = filtering;
            if (sorting != null) this.sorting = sorting;
            if (totals != null) this.totals = totals;
        }
    }

    public class Service
    {
        [JsonProperty("serviceid")]
        public string serviceID;

        public string target = "";

        [JsonProperty("sessionid")]
        public string sessionID;

        [JsonProperty("loginname")]
        public string loginName;

        public string password;

        [JsonProperty("params")]
        public Parameters parameters;

        [JsonProperty("datas")]
        public List<Parameters> data = new List<Parameters>();

        public Service(
            Parameters parameters, string userID, string password)
        {
            serviceID = parameters.ServiceID;
            loginName = userID;
            this.password = password;
            this.parameters = parameters;
            data.Add(parameters);
        }

        public string ToJson() => JsonConvert.SerializeObject(this);
    }

    public class ResultData
    {
        class Resolver : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(
                Type type, MemberSerialization memberSerialization)
            {
                IList<JsonProperty> list = base.CreateProperties(
                    type, memberSerialization);

                foreach (JsonProperty prop in list)
                {
                    prop.PropertyName = prop.UnderlyingName;
                }

                return list;
            }
        }

        public string ToJson(bool orignalName = true)
        {
            var settings = new JsonSerializerSettings();
            if (orignalName) settings.ContractResolver = new Resolver();
            return JsonConvert.SerializeObject(this, settings);
        }
    }

    public class Result<T> where T : ResultData
    {
        [JsonProperty("rowcount")]
        public int rowCount;

        public int page;

        [JsonProperty("pagesize")]
        public int pageSize;

        [JsonProperty("serviceid")]
        public string serviceID;

        public string type;
        public string vcode;
        public string message;

        [JsonProperty("messagedetail")]
        public string messageDetail;

        [JsonProperty("datas")]
        public List<T> data;

        [JsonIgnore]
        public List<T> Data
            => data ?? (data = new List<T>());

        public T this[int index] => Data[index];

        public int Count => data.Count;

        public static Result<T> FromJson(string json)
            => JsonConvert.DeserializeObject<Result<T>>(json);
    }

    public class Syslogin : Parameters
    {
        [JsonProperty("username")]
        public readonly string userName;

        [JsonProperty("passwd")]
        public readonly string password;

        public Syslogin(
            string userName, string password) : base("syslogin")
        {
            this.userName = userName;
            this.password = password;
        }
    }

    /// 省内参保缴费信息查询
    public class JfxxQuery : PageParameters
    {
        [JsonProperty("aac002")]
        public string idCard = "";

        public JfxxQuery(string idCard)
            : base("executeSncbqkcxjfxxQ", page: 1, pageSize: 500)
        {
            this.idCard = idCard;
        }
    }

    /// 省内参保缴费信息
    public class Jfxx : ResultData
    {
        /// 缴费年度
        [JsonProperty("aae003")]
        public int year;

        /// 备注
        [JsonProperty("aae013")]
        public string memo;

        /// 金额
        [JsonProperty("aae022")]
        public decimal amount;

        /// 缴费类型
        [JsonConverter(
            typeof(FieldConverter<string, Type>))]
        public class Type : Field<string>
        {
            public override string Name
            {
                get
                {
                    return Value switch
                    {
                        "10" => "正常应缴",
                        "31" => "补缴",
                        _ => $"未知值: {Value}"
                    };
                }
            }
        }

        [JsonProperty("aaa115")]
        public Type type;

        /// 缴费项目
        [JsonConverter(
            typeof(FieldConverter<string, Item>))]
        public class Item : Field<string>
        {
            public override string Name
            {
                get
                {
                    return Value switch
                    {
                        "1" => "个人缴费",
                        "3" => "省级财政补贴",
                        "4" => "市级财政补贴",
                        "5" => "县级财政补贴",
                        "11" => "政府代缴",
                        _ => $"未知值: {Value}"
                    };
                }
            }
        }

        [JsonProperty("aae341")]
        public Item item;

        /// 缴费方式
        [JsonConverter(
            typeof(FieldConverter<string, Method>))]
        public class Method : Field<string>
        {
            public override string Name
            {
                get
                {
                    return Value switch
                    {
                        "2" => "银行代收",
                        "3" => "经办机构自收",
                        _ => $"未知值: {Value}"
                    };
                }
            }
        }

        [JsonProperty("aab033")]
        public Method method;

        /// 划拨日期
        [JsonProperty("aae006")]
        public string payedOffDay;

        /// 是否已划拨
        public bool IsPayedOff => payedOffDay != null;

        /// 社保机构
        [JsonProperty("aaa027")]
        public string agent;

        /// 行政区划代码
        [JsonProperty("aaf101")]
        public string xzqh;
    }

}