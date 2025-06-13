using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using pefi.dynamicdns.Infrastructure;

public class ScheduledIpCheck(IDNSClient dnsClient, ILogger<ScheduledIpCheck> logger) : BackgroundService
{

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        IPAddressInfo? previousIPAddress = null;

        while (!stoppingToken.IsCancellationRequested)
        {

            var currentIPAddress = await IpAddressLookup.GetPublicIpAddress();

            if (previousIPAddress != currentIPAddress)
            {
                var previousIPAddressValue = previousIPAddress?.Ip ?? "NOT SET";


                logger.LogInformation("IP address changed from '{oldIpAddress}' to '{CurrentIpAddress}'", previousIPAddressValue, currentIPAddress.Ip);
                dnsClient.UpdateDNSRecord("pefi.co.uk", "home", currentIPAddress);
                previousIPAddress = currentIPAddress;
            }
            else
            {
                logger.LogInformation("IP address current address '{CurrentIpAddress}' has not changed", currentIPAddress.Ip);
            }

            await Task.Delay(TimeSpan.FromMinutes(2));
        }
    }
}