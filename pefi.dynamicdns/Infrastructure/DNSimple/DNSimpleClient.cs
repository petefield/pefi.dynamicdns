using dnsimple;
using dnsimple.Services;
using Microsoft.Extensions.Logging;
using OneOf.Types;

namespace pefi.dynamicdns.Infrastructure.DNSimple;

public class DNSimpleClient : IDNSClient
{
    private readonly Client client;
    private readonly string zoneId;
    private readonly long accountId;
    private readonly ILogger<DNSimpleClient> logger;

    public DNSimpleClient(string domain, string apiToken, ILogger<DNSimpleClient> logger)
    {
        client = new Client();
        var credentials = new OAuth2Credentials(apiToken);
        client.AddCredentials(credentials);
        var response = client.Identity.Whoami();
        var account = client.Identity.Whoami().Data.Account;
        accountId = account.Id;
        var zone = client.Zones.ListZones(accountId).Data.Single(x => x.Name == domain);
        zoneId = zone.Id.ToString();
        this.logger = logger;
    }

    public Result UpdateDNSRecord(string name, IPAddressInfo ipAddressInfo)
    {
        try
        {
            logger.LogInformation("Updating DNS record '{name}' to IP address '{ipAddress}'", name, ipAddressInfo.Ip);

            var records = client.Zones.ListZoneRecords(accountId, zoneId).Data;

            var record = records.Single(x => x.Name == name);

            client.Zones.UpdateZoneRecord(accountId, zoneId, record.Id, new ZoneRecord
            {
                Content = ipAddressInfo.Ip,
            });

            return new Success();

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update DNS record '{name}' to IP address '{ipAddress}'", name, ipAddressInfo.Ip);
            return new Error();
        }
    }

    public Result AddCNAMERecord(string name, string content)
    {
        try
        {
            logger.LogInformation("Adding CNAME record '{name}' with content '{content}'", name, content);

            client.Zones.CreateZoneRecord(accountId, zoneId, new ZoneRecord()
            {
                Content = content,
                Name = name,
                Type = "CNAME"
            });

            return new Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to add CNAME record '{name}' with content '{content}'", name, content);
            return new Error();
        }
    }

    public Result DeleteDnsRecord(string name)
    {
        logger.LogInformation("Delete zone record :{name}", name);

        try
        {
            var record = client.Zones.ListZoneRecords(accountId, zoneId)
                .Data.Single(record => record.Name == name);

            client.Zones.DeleteZoneRecord(accountId, zoneId, record.Id);
            return new Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete zone record :{name}", name);
            return new Error();
        }


    }
}