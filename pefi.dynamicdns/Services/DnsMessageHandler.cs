using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using pefi.dynamicdns.Infrastructure;
using pefi.dynamicdns;
using pefi.dynamicdns.Models;

namespace pefi.dynamicdns.Services;

public class DnsMessageHandler(
    IDNSClient dnsClient,
    IServiceManagerClient serviceManagerClient,
    IOptions<DnsSettings> dnsOptions,
    ILogger<DnsMessageHandler> logger) : IDnsMessageHandler
{
    private readonly DnsSettings _dnsSettings = dnsOptions.Value;

    public async Task HandleServiceCreated(string serviceName)
    {
        try
        {
            var service = await serviceManagerClient.GetServiceByNameAsync(serviceName);

            logger.LogInformation("Update DNS {serviceName}", service.ServiceName);
            logger.LogInformation("Adding CNAME '{serviceName}' to zone '{domain}' with content '{homeHostname}.{domain}'",
                serviceName, _dnsSettings.Domain, _dnsSettings.HomeHostname, _dnsSettings.Domain);

            dnsClient.AddCNAMERecord(_dnsSettings.Domain, $"{service.HostName}", _dnsSettings.HomeHostname);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Updating DNS for service {serviceName}", serviceName);
            throw;
        }
    }

    public Task HandleServiceDeleted(ServiceDeletedMessage message)
    {
        try
        {
            var service = message.Service;

            logger.LogInformation("Delete DNS {serviceName}", service.ServiceName);

            if (service.HostName is not null)
            {
                dnsClient.DeleteDnsRecord(service.HostName);
                logger.LogInformation("Deleted DNS {hostName}", service.HostName);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Deleting DNS for service {message}", message);
            throw;
        }

        return Task.CompletedTask;
    }
}
