// See https://aka.ms/new-console-template for more information
using dnsimple;

public static class DNSUpdater {

    public static void UpdateDNSRecord(string domain, string recordName, IPAddressInfo ipAddressInfo)
    {
        var client = new Client();
        client.AddCredentials(new OAuth2Credentials("dnsimple_a_D8HUCvbQYCorXSAa1ebJuMtWUeZGR8K3"));

        // Fetch your details
        var account = client.Identity.Whoami().Data.Account;   // execute the call

        var zone = client.Zones.ListZones(account.Id).Data.Single(x => x.Name == domain);

        var record = client.Zones.ListZoneRecords(account.Id, zone.Id.ToString()).Data.Single(x => x.Name == recordName);

        client.Zones.UpdateZoneRecord(account.Id, zone.Id.ToString(), record.Id, new dnsimple.Services.ZoneRecord
        {
            Content = ipAddressInfo.Ip,
        });
    }

}
