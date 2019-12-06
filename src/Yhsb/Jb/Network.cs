using System;
using System.Web;
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

        public Dictionary<string, string>[] filtering = { };

        public Dictionary<string, string>[] sorting = { };

        public Dictionary<string, string>[] totals = { };

        public PageParameters(
            string id, int page = 1, int pageSize = 15,
            Dictionary<string, string>[] filtering = null,
            Dictionary<string, string>[] sorting = null,
            Dictionary<string, string>[] totals = null) : base(id)
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

    /// 居保状态
    public class Util
    {
        public static string JBState(object jfState, object cbState) =>
            jfState switch
            {
                "1" => cbState switch
                {
                    "1" => "正常缴费人员",
                    _ => $"未知类型参保缴费人员: {cbState}"
                },
                "2" => cbState switch
                {
                    "2" => "暂停缴费人员",
                    _ => $"未知类型暂停缴费人员: {cbState}"
                },
                "3" => cbState switch
                {
                    "1" => "正常待遇人员",
                    "2" => "暂停待遇人员",
                    "4" => "终止参保人员",
                    _ => $"未知类型终止缴费人员: {cbState}"
                },
                _ => $"未知类型人员: {jfState}, {cbState}",
            };
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

        [JsonProperty("aac008")]
        public CBState cbState;

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
        public string JBState => Util.JBState(jfState.Value, cbState.Value);

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

    /// 待遇人员审核查询
    public class DyryQuery : PageParameters
    {
        public string aaf013 = "", aaf030 = "";

        /// 预算到龄日期
        /// 2019-04-30
        public string dlny = "";

        /// 预算后待遇起始时间: '1'-到龄次月
        public string yssj = "";

        public string aac009 = "";

        /// 是否欠费
        public string qfbz = "";

        public string aac002 = "";

        /// 参保状态: '1'-正常参保
        [JsonProperty("aac008")]
        public string cbzt = "";

        /// 是否和社保比对: '1'-是 '2'-否
        [JsonProperty("sb_type")]
        public string sbbd = "";

        public DyryQuery(string dlny, string yssj = "1",
            string cbzt = "1", string sbbd = "1")
            : base("dyryQuery", pageSize: 500,
                sorting: new[]
                {
                    new Dictionary<string, string>
                    {
                        ["dataKey"] = "xzqh",
                        ["sortDirection"] = "ascending"
                    }
                })
        {
            this.dlny = dlny;
            this.yssj = yssj;
            this.cbzt = cbzt;
            this.sbbd = sbbd;
        }
    }

    /// 性别
    public class Sex : JsonField
    {
        public override string Name => Value switch
        {
            "1" => "男",
            "2" => "女",
            _ => $"未知值: {Value}"
        };
    }

    /// 户藉性质
    public class HJClass : JsonField
    {
        public override string Name => Value switch
        {
            "10" => "城市户籍",
            "20" => "农村户籍",
            _ => $"未知值: {Value}"
        };
    }

    public class Dyry : ResultData
    {
        [JsonProperty("xm")]
        public string name;

        [JsonProperty("sfz")]
        public string idCard;

        [JsonProperty("csrq")]
        public int birthDay;

        [JsonProperty("rycbzt")]
        public CBState cbState;

        [JsonProperty("aac031")]
        public JFState jfState;

        /// 企保参保
        [JsonProperty("qysb_type")]
        public string qbzt;

        /// 共缴年限
        public string gjnx;

        /// 待遇领取年月
        public string lqny;

        /// 备注
        public string bz;

        /// 行政区划
        public string xzqh;

        /// 居保状态
        public string JBState => Util.JBState(cbState.Value, jfState.Value);

        /// 性别
        [JsonProperty("xb")]
        public Sex sex;

        [JsonProperty("aac009")]
        public HJClass hJClass;

        /// 应缴年限
        public int Yjnx
        {
            get
            {
                var year = birthDay / 10000;
                var month = birthDay / 100 - year * 100;
                year -= 1951;
                if (year >= 15) return 15;
                else if (year < 0) return 0;
                else if (year == 0 && month >= 7) return 1;
                else return year;
            }
        }

        /// 实缴年限
        public int Sjnx => int.Parse(gjnx);
    }

    /// 待遇复核查询
    public class DyfhQuery : PageParameters
    {
        public string aaf013 = "", aaf030 = "";

        [JsonProperty("aae016")]
        public string shzt = "";

        public string aae011 = "", aae036 = "", aae036s = "";
        public string aae014 = "";

        [JsonProperty("aae015")]
        public string qsshsj = "";

        [JsonProperty("aae015s")]
        public string jzshsj = "";

        public string aac009 = "", aac003 = "";

        [JsonProperty("aac002")]
        public string idCard = "";

        public DyfhQuery(
            string idcard = "", string shzt = "0",
            string qsshsj = "", string jzshsj = "",
            int page = 1, int pageSize = 500,
            Dictionary<string, string>[] sorting = null)
            : base("dyfhQuery", page: page, pageSize: pageSize,
                sorting: sorting ?? new[]
                {
                    new Dictionary<string, string>
                    {
                        ["dataKey"] = "aaa027",
                        ["sortDirection"] = "ascending"
                    }
                })
        {
            this.idCard = idcard;
            this.shzt = shzt;
            this.qsshsj = qsshsj;
            this.jzshsj = jzshsj;
        }
    }

    public class Xzqh
    {
        public static string[] regex =
            {
                "湘潭市雨湖区((.*?乡)(.*?村))",
                "湘潭市雨湖区((.*?乡)(.*?政府机关))",
                "湘潭市雨湖区((.*?街道)办事处(.*?社区))",
                "湘潭市雨湖区((.*?街道)办事处(.*?政府机关))",
                "湘潭市雨湖区((.*?镇)(.*?社区))",
                "湘潭市雨湖区((.*?镇)(.*?居委会))",
                "湘潭市雨湖区((.*?镇)(.*?村))",
                "湘潭市雨湖区((.*?街道)办事处(.*?村))",
                "湘潭市雨湖区((.*?镇)(.*?政府机关)",
            };
    }

    public class Dyfh : ResultData
    {
        /// 个人编号
        [JsonProperty("aac001")]
        public int grbh;

        /// 身份证号码
        [JsonProperty("aac002")]
        public string idCard;

        [JsonProperty("aac003")]
        public string name;

        /// 行政区划
        [JsonProperty("aaa027")]
        public string xzqh;

        /// 单位名称
        [JsonProperty("aaa129")]
        public string dwmc;

        /// 月养老金
        [JsonProperty("aic166")]
        public decimal payAmount;

        /// 财务月份
        [JsonProperty("aae211")]
        public int accountMonth;

        /// 实际待遇开始月份
        [JsonProperty("aic160")]
        public int payMonth;

        public string bz = "", fpName = "",
            fpType = "", fpDate = "";

        public int aaz170, aaz159, aaz157;

        public Match PaymentInfo
        {
            get
            {
                static string Escape(object str) =>
                    HttpUtility.UrlEncode(str.ToString());

                var path =
                    "/hncjb/reports?method=htmlcontent&name=yljjs&" +
                    $"aaz170={Escape(aaz170)}&aaz159={Escape(aaz159)}&aac001={Escape(grbh)}&" +
                    $"aaz157={Escape(aaz157)}&aaa129={Escape(dwmc)}&aae211={Escape(accountMonth)}";

                using var sock = new HttpSocket(
                    _internal.Session.Host, _internal.Session.Port);
                var content = sock.GetHttp(path);
                return Regex.Match(content, _regexPaymentInfo);
            }
        }

        static readonly string _regexPaymentInfo =
   @"<tr>
        <td height=""32"" align=""center"">姓名</td>
        <td align=""center"">性别</td>
        <td align=""center"" colspan=""3"">身份证</td>
        <td align=""center"" colspan=""2"">困难级别</td>
        <td align=""center"" colspan=""3"">户籍所在地</td>
        <td align=""center"" colspan=""2"">所在地行政区划编码</td>
      </tr>
      <tr class=""detail"" component=""detail"">
        <td height=""39"" align=""center"">(.+?)</td>
        <td align=""center"">(.+?)</td>
        <td align=""center"" colspan=""3"">(.+?)</td>
        <td align=""center"" colspan=""2"">(.+?)</td>
        <td align=""center"" colspan=""3""(?:/>|>(.+?)</td>)
        <td align=""center"" colspan=""2"">(.+?)</td>
      </tr>
      <tr>
        <td height=""77"" align=""center"" rowspan=""2"">缴费起始年月</td>
        <td align=""center"" rowspan=""2"">累计缴费年限</td>
        <td align=""center"" rowspan=""2"" colspan=""2"">个人账户累计存储额</td>
        <td height=""25"" align=""center"" colspan=""8"">其中</td>
      </tr>
      <tr>
        <td height=""30"" align=""center"">个人缴费</td>
        <td align=""center"">省级补贴</td>
        <td align=""center"">市级补贴</td>
        <td align=""center"">县级补贴</td>
        <td align=""center"">集体补助</td>
        <td align=""center"">被征地补助</td>
        <td align=""center"">政府代缴</td>
        <td align=""center"">利息</td>
      </tr>
      <tr class=""detail"" component=""detail"">
        <td height=""40"" align=""center"">(.+?)</td>
        <td align=""center"">(.+?)</td>
        <td align=""center"" colspan=""2"">(.+?)</td>
        <td align=""center"">(.+?)</td>
        <td align=""center"">(.+?)</td>
        <td align=""center"">(.+?)</td>
        <td align=""center"">(.+?)</td>
        <td align=""center"">(.+?)</td>
        <td align=""center"">(.+?)</td>
        <td align=""center"">(.+?)</td>
        <td align=""center"">(.+?)</td>
      </tr>
      <tr>
        <td align=""center"" rowspan=""2"">
          <p>领取养老金起始时间</p>
        </td>
        <td align=""center"" rowspan=""2"">月养老金</td>
        <td height=""29"" align=""center"" colspan=""5"">其中：基础养老金</td>
        <td align=""center"" colspan=""5"">个人账户养老金</td>
      </tr>
      <tr>
        <td height=""31"" align=""center"">国家补贴</td>
        <td height=""31"" align=""center"">省级补贴</td>
        <td align=""center"">市级补贴</td>
        <td align=""center"">县级补贴</td>
        <td align=""center"">加发补贴</td>
        <td align=""center"">个人实缴部分</td>
        <td align=""center"">缴费补贴部分</td>
        <td align=""center"">集体补助部分</td>
        <td align=""center"">被征地补助部分</td>
        <td align=""center"">政府代缴部分</td>
      </tr>
      <tr class=""detail"" component=""detail"">
        <td height=""40"" align=""center"">(.+?)</td>
        <td align=""center"">(.+?)</td>
        <td align=""center"">(.+?)</td>
        <td align=""center"">(.+?)</td>
        <td align=""center"">(.+?)</td>
        <td align=""center"">(.+?)</td>
        <td align=""center"">(.+?)</td>
        <td align=""center"">(.+?)</td>
        <td align=""center"">(.+?)</td>
        <td align=""center"">(.+?)</td>
        <td align=""center"">(.+?)</td>
        <td align=""center"">(.+?)</td>
      </tr>".Replace("\r\n", "\n");
    }

    public class BankInfoQuery : Parameters
    {
        [JsonProperty("aac002")]
        public string idCard = "";

        public BankInfoQuery(string idCard) 
            : base("executeSncbgrBankinfoConQ")
        {
            this.idCard = idCard;
        }
    }

    public class BankInfo : ResultData
    {
        /// 银行类型
        [JsonProperty("bie013")]
        public string bankType;

        /// 户名
        [JsonProperty("aae009")]
        public string name;

        /// 卡号
        [JsonProperty("aae010")]
        public string cardNumber;

        public string BankName =>
            bankType switch
            {
                "LY" => "中国农业银行",
                "ZG" => "中国银行",
                "JS" => "中国建设银行",
                "NH" => "农村信用合作社",
                "YZ" => "邮政",
                "JT" => "交通银行",
                "GS" => "中国工商银行",
                _ => ""
            };
    }

    /// 代发支付单查询
    public class OtherPaymentQuery : PageParameters
    {
        /// 代发类型
        [JsonProperty("aaa121")]
        public string type;

        /// 支付单号
        [JsonProperty("aaz031")]
        public string payList = "";

        /// 发放年月
        [JsonProperty("aae002")]
        public string yearMonth;

        [JsonProperty("aae089")]
        public string state;

        public OtherPaymentQuery(
            string type, string yearMonth, string state = "0") : base("dfpayffzfdjQuery")
        {
            this.type = type;
            this.yearMonth = yearMonth;
            this.state = state;
        }
    }

    public class OtherPayment : ResultData
    {
        /// 业务类型中文名
        [JsonProperty("aaa121")]
        public string typeCH;

        /// 付款单号
        [JsonProperty("aaz031")]
        public int payList;

        public static string Name(string type) =>
            type switch
            {
                "DF0001" => "独生子女",
                "DF0002" => "乡村教师",
                "DF0003" => "乡村医生",
                "DF0007" => "电影放映员",
                _ => ""
            };
    }

    /// 代发支付单明细查询
    public class OtherPaymentDetailQuery : PageParameters
    {
        /// 付款单号
        [JsonProperty("aaz031")]
        public string payList;

        public OtherPaymentDetailQuery(
            int payList, int page = 1, int pageSize = 500)
            : base("dfpayffzfdjmxQuery", page: page, pageSize: pageSize)
        {
            this.payList = $"{payList}";
        }
    }

    public class OtherPaymentDetail : ResultData
    {
        #region PersonInfo

        /// 个人编号
        [JsonProperty("aac001")]
        public int grbh;

        /// 身份证号码
        [JsonProperty("aac002")]
        public string idCard;

        [JsonProperty("aac003")]
        public string name;

        /// 村社区名称
        [JsonProperty("aaf103")]
        public string region;

        #endregion

        #region OtherPayment

        /// 支付标志
        [JsonProperty("aae117")]
        public string flag;

        /// 发放年月
        [JsonProperty("aae002")]
        public int? yearMonth;

        /// 付款单号
        [JsonProperty("aaz031")]
        public int payList;

        /// 个人单号
        [JsonProperty("aaz220")]
        public long personalPayList;

        /// 支付总金额
        [JsonProperty("aae019")]
        public decimal amount;

        #endregion
    }

    /// 代发支付单个人明细查询
    public class OtherPaymentPersonalDetailQuery : PageParameters
    {
        /// 个人编号
        [JsonProperty("aac001")]
        public string grbh;

        /// 付款单号
        [JsonProperty("aaz031")]
        public string payList;

        /// 个人单号
        [JsonProperty("aaz220")]
        public string personalPayList;

        public OtherPaymentPersonalDetailQuery(
            int grbh, int payList, long personalPayList,
            int page = 1, int pageSize = 500)
            : base("dfpayffzfdjgrmxQuery", page: page, pageSize: pageSize)
        {
            this.grbh = $"{grbh}";
            this.payList = $"{payList}";
            this.personalPayList = $"{personalPayList}";
        }
    }

    public class OtherPaymentPersonalDetail : ResultData
    {
        /// 待遇日期
        [JsonProperty("aae003")]
        public int date;

        /// 支付标志
        [JsonProperty("aae117")]
        public string flag;

        /// 发放年月
        [JsonProperty("aae002")]
        public int? yearMonth;

        /// 付款单号
        [JsonProperty("aaz031")]
        public int payList;

        /// 支付总金额
        [JsonProperty("aae019")]
        public decimal amount;
    }
}