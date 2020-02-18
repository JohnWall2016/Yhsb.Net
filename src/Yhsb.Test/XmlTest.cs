using Yhsb.Qb.Network;
using System.IO;
using System.Xml.Linq;

using static System.Console;

public class XmlTest
{
    public static void TestXml()
    {
        var element = new SncbryQuery("430302195806251012").ToXElement();
        WriteLine(element);

        WriteLine(new InEnvelope<SncbryQuery>(
          new SncbryQuery("430302195806251012"), "hqm", "YLZ_A2A5F63315129CB2998A0E0FCE31BA51"));

        WriteLine(new InEnvelope<Login>(new Login(), "hqm", "YLZ_A2A5F63315129CB2998A0E0FCE31BA51"));

        var doc = XDocument.Load(new StringReader(xmlResult));
        var (header, body) = new OutEnvelope<QueryList<Sncbry>>(doc);

        WriteLine(header.sessionID);
        WriteLine(body.rowCount);
        WriteLine(body.querySql);

        foreach (var ry in body.queryList)
        {
            WriteLine(ry.name);
            WriteLine(ry.idcard);
        }
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