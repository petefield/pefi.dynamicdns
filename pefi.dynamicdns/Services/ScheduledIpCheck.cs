using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using pefi.dynamicdns;
using pefi.dynamicdns.Infrastructure;
using pefi.dynamicdns.Services;

public class ScheduledIpCheck(IDNSClient dnsClient, IOptions<DnsSettings> dnsOptions, IIpAddressLookup ipAddressLookup, ILogger<ScheduledIpCheck> logger) : BackgroundService
{

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        IPAddressInfo? previousIPAddress = null;
        var dnsSettings = dnsOptions.Value;

        while (!stoppingToken.IsCancellationRequested)
        {

            var currentIPAddress = await ipAddressLookup.GetPublicIpAddress();

            if (previousIPAddress != currentIPAddress)
            {
                var previousIPAddressValue = previousIPAddress?.Ip ?? "NOT SET";


                logger.LogInformation("IP address changed from '{oldIpAddress}' to '{CurrentIpAddress}'", previousIPAddressValue, currentIPAddress.Ip);
                dnsClient.UpdateDNSRecord(dnsSettings.Domain, dnsSettings.HomeHostname, currentIPAddress);
                previousIPAddress = currentIPAddress;
            }
            else
            {
                logger.LogInformation("IP address current address '{CurrentIpAddress}' has not changed", currentIPAddress.Ip);
            }

            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
        }
    }
}