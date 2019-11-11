using System.Text.RegularExpressions;
using System.Collections.Generic;
using Newtonsoft.Json;
using Json = Newtonsoft.Json.JsonPropertyAttribute;
using Yhsb.Net;

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

        public Result<T> GetResult<T>()
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

        [Json("pagesize")]
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
        [Json("serviceid")]
        public string serviceID;

        public string target = "";

        [Json("sessionid")]
        public string sessionID;

        [Json("loginname")]
        public string loginName;

        public string password;

        [Json("params")]
        public Parameters parameters;

        [Json("datas")]
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

    public class Result<T>
    {
        [Json("rowcount")]
        public int rowCount;

        public int page;

        [Json("pagesize")]
        public int pageSize;

        [Json("serviceid")]
        public string serviceID;

        public string type;
        public string vcode;
        public string message;

        [Json("messagedetail")]
        public string messageDetail;

        [Json("datas")]
        public List<T> data;

        [JsonIgnore]
        public List<T> Data
            => data != null ? data : (data = new List<T>());

        public T this[int index] => Data[index];

        public int Count => data.Count;

        public static Result<T> FromJson(string json)
            => JsonConvert.DeserializeObject<Result<T>>(json);
    }

    public class Syslogin : Parameters
    {
        [Json("username")]
        public readonly string userName;

        [Json("passwd")]
        public readonly string password;

        public Syslogin(
            string userName, string password) : base("syslogin")
        {
            this.userName = userName;
            this.password = password;
        }
    }
}