// See https://aka.ms/new-console-template for more information
using dnsimple;
using dnsimple.Services;
using Microsoft.Extensions.Logging;

namespace pefi.dynamicdns.Infrastructure.DNSimple;

public class DNSimpleClient : IDNSClient
{
    private readonly Client client;
    private readonly string zoneId;
    private readonly long accountId;
    private readonly ILogger<DNSimpleClient> logger;

    public DNSimpleClient(string domain, ILogger<DNSimpleClient> logger)
    {
        client = new Client();
        client.AddCredentials(new OAuth2Credentials("dnsimple_a_D8HUCvbQYCorXSAa1ebJuMtWUeZGR8K3"));
        var account = client.Identity.Whoami().Data.Account;
        accountId = account.Id;
        var zone = client.Zones.ListZones(accountId).Data.Single(x => x.Name == domain);
        zoneId = zone.Id.ToString();
        this.logger = logger;
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

    public void DeleteDnsRecord(string host)
    {
        logger.LogInformation("Delete zone record :{host}", host);

        try
        {
            var record = client.Zones.ListZoneRecords(accountId, zoneId)
                .Data.Single(record => record.Name == host);

            client.Zones.DeleteZoneRecord(accountId, zoneId, record.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Delete zone record :{host}", host);
        }


    }
}