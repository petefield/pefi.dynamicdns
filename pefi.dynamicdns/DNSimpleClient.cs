// See https://aka.ms/new-console-template for more information
using dnsimple;
using dnsimple.Services;

namespace pefi.dynamicdns;

public class DNSimpleClient : IDNSClient
{
    private readonly Client client;
    private readonly string zoneId;
    private readonly long accountId;

    public DNSimpleClient(string domain)
    {
        client = new Client();
        client.AddCredentials(new OAuth2Credentials("dnsimple_a_D8HUCvbQYCorXSAa1ebJuMtWUeZGR8K3"));
        var account = client.Identity.Whoami().Data.Account;
        accountId = account.Id;
        var zone = client.Zones.ListZones(accountId).Data.Single(x => x.Name == domain);
        zoneId = zone.Id.ToString();
    }

    public void UpdateDNSRecord(string domain, string recordName, IPAddressInfo ipAddressInfo)
    {
        var record = client.Zones.ListZoneRecords(accountId, zoneId).Data.Single(x => x.Name == recordName);

        client.Zones.UpdateZoneRecord(accountId, zoneId, record.Id, new ZoneRecord
        {
            Content = ipAddressInfo.Ip,
        });
    }

    public void AddCNAMERecord(string domain, string host, string content)
    {
        client.Zones.CreateZoneRecord(accountId, zoneId, new ZoneRecord()
        {
            Content = $"{content}.{domain}",
            Name = host,
            Type = ZoneRecordType.CNAME
        });
    }

}