using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Newtonsoft.Json;

using Yhsb.Net;
using Yhsb.Json;

namespace Yhsb.Jb.Network
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

        public string ToJson() => JsonExtension.Serialize(this);
    }

    public class ResultData
    {
        public string ToJson(bool orignalName = true) =>
            JsonExtension.Serialize(this, orignalName);
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
        public List<T> Data => data ?? (data = new List<T>());

        public T this[int index] => Data[index];

        public int Count => Data.Count;

        public bool IsEmpty => Count <= 0;

        public static Result<T> FromJson(string json) =>
            JsonExtension.Deserialize<Result<T>>(json);
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
        public int? year;

        /// 备注
        [JsonProperty("aae013")]
        public string memo;

        /// 金额
        [JsonProperty("aae022")]
        public decimal amount;

        /// 缴费类型
        public class Type : JsonField
        {
            public override string Name => Value switch
            {
                "10" => "正常应缴",
                "31" => "补缴",
                _ => $"未知值: {Value}"
            };
        }

        [JsonProperty("aaa115")]
        public Type type;

        /// 缴费项目
        public class Item : JsonField
        {
            public override string Name => Value switch
            {
                "1" => "个人缴费",
                "3" => "省级财政补贴",
                "4" => "市级财政补贴",
                "5" => "县级财政补贴",
                "11" => "政府代缴",
                _ => $"未知值: {Value}"
            };
        }

        [JsonProperty("aae341")]
        public Item item;

        /// 缴费方式
        public class Method : JsonField
        {
            public override string Name => Value switch
            {
                "2" => "银行代收",
                "3" => "经办机构自收",
                _ => $"未知值: {Value}"
            };
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
        public string agency;

        /// 行政区划代码
        [JsonProperty("aaf101")]
        public string xzqhCode;
    }

    /// 省内参保信息查询
    public class CbxxQuery : Parameters
    {
        [JsonProperty("aac002")]
        public string idCard = "";

        public CbxxQuery(string idCard) : base("executeSncbxxConQ")
        {
            this.idCard = idCard;
        }
    }

    /// 省内参保信息
    public class Cbxx : ResultData
    {
        /// 个人编号
        [JsonProperty("aac001")]
        public int pid;

        /// 身份证号码
        [JsonProperty("aac002")]
        public string idCard;

        [JsonProperty("aac003")]
        public string name;

        [JsonProperty("aac006")]
        public string birthDay;

        /// 参保状态
        public class CBState : JsonField
        {
            public override string Name => Value switch
            {
                "0" => "未参保",
                "1" => "正常参保",
                "2" => "暂停参保",
                "4" => "终止参保",
                _ => $"未知值: {Value}"
            };
        }

        [JsonProperty("aac008")]
        public CBState cbState;

        /// 缴费状态
        public class JFState : JsonField
        {
            public override string Name => Value switch
            {
                "1" => "参保缴费",
                "2" => "暂停缴费",
                "3" => "终止缴费",
                _ => $"未知值: {Value}"
            };
        }

        [JsonProperty("aac031")]
        public JFState jfState;

        /// 参保时间
        [JsonProperty("aac049")]
        public int cbDate;

        /// 参保身份编码
        [JsonProperty("aac066")]
        public string sfCode;

        /// 社保机构
        [JsonProperty("aaa129")]
        public string agency;

        /// 经办时间
        [JsonProperty("aae036")]
        public string dealDate;

        /// 行政区划编码
        [JsonProperty("aaf101")]
        public string xzqhCode;

        /// 村组名称
        [JsonProperty("aaf102")]
        public string czName;

        /// 村社区名称
        [JsonProperty("aaf103")]
        public string csName;

        /// 居保状态
        public string JBState => jfState.Value switch
        {
            "1" => cbState.Value switch
            {
                "1" => "正常缴费人员",
                _ => $"未知类型参保缴费人员: {cbState.Value}"
            },
            "2" => cbState.Value switch
            {
                "2" => "暂停缴费人员",
                _ => $"未知类型暂停缴费人员: {cbState.Value}"
            },
            "3" => cbState.Value switch
            {
                "1" => "正常待遇人员",
                "2" => "暂停待遇人员",
                "4" => "终止参保人员",
                _ => $"未知类型终止缴费人员: {cbState.Value}"
            },
            _ => $"未知类型人员: {jfState.Value}, {cbState.Value}",
        };

        /// 参保身份类型
        public string JBClass => sfCode switch
        {
            "011" => "普通参保人员",
            "021" => "残一级",
            "022" => "残二级",
            "031" => "特困一级",
            "051" => "贫困人口一级",
            "061" => "低保对象一级",
            "062" => "低保对象二级",
            _ => $"未知身份类型: {sfCode}"
        };

        public bool Valid => idCard != null;

        public bool Invalid => !Valid;
    }

    /// 参保审核查询
    public class CbshQuery : PageParameters
    {
        public string aaf013 = "",
            aaf030 = "",
            aae011 = "",
            aae036 = "",
            aae036s = "",
            aae014 = "",
            aac009 = "",
            aac002 = "",
            aac003 = "",
            sfccb = "";

        [JsonProperty("aae015")]
        public string startDate = ""; // "2019-04-29"

        [JsonProperty("aae015s")]
        public string endDate = "";

        [JsonProperty("aae016")]
        public string shzt = "1";

        public CbshQuery(string startDate = "", string endDate = "",
            string shzt = "") : base("cbshQuery", pageSize: 500)
        {
            this.startDate = startDate;
            this.endDate = endDate;
            this.shzt = shzt;
        }
    }

    public class Cbsh : ResultData
    {
        /// 身份证号码
        [JsonProperty("aac002")]
        public string idCard;

        [JsonProperty("aac003")]
        public string name;

        [JsonProperty("aac006")]
        public string birthDay;
    }

}