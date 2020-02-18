using Yhsb.Qb.Network;

using static System.Console;

public class QbTest
{
    public static void TestLogin()
    {
        Session.Use((session, info) => {
            WriteLine(info.XmlData);
            WriteLine(info);

            WriteLine(session.ToInEnvelopeString(new SncbryQuery("430302195806251012")));

            session.SendInEnvelope(new SncbryQuery("430302195806251012"));
            var (header, body) = session.GetOutEnvelope<QueryList<Sncbry>>();
            
            WriteLine(header.XmlData);
            WriteLine(header);

            WriteLine(body.XmlData);
            WriteLine(body);
        });
    }
}