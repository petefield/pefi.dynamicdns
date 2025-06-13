using dnsimple.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using pefi.dynamicdns.Infrastructure;
using pefi.dynamicdns.Services;
using pefi.Rabbit;

namespace pefi.dynamicdns;

public class EventListener(ILogger<EventListener> logger,
    IMessageBroker messageBroker, 
    IDNSClient DNSClient, ServiceManagerClient serviceManagerClient) : BackgroundService
{
    private ITopic? _topic;


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Dynamic DNS Service is listening for new services.");

        _topic = await messageBroker.CreateTopic("Events");

        await _topic.Subscribe("events.service.created", async (key, message) => {
            var svc = System.Text.Json.JsonSerializer.Deserialize<ServiceCreatedMessage>(message);
            await UpdateDNS(svc.ServiceName);
        });

        await _topic.Subscribe("events.service.deleted", async (key, message) => {
            var svc = System.Text.Json.JsonSerializer.Deserialize<ServiceCreatedMessage>(message);
            await DeleteDNs(svc.ServiceName);
        });
    }

    private async Task DeleteDNs(string serviceName)
    {

        var service = await serviceManagerClient.Get_Service_By_NameAsync(serviceName);
        logger.LogInformation("Delete DNS {serviceName}", service.serviceName);

        if (service.hostName is not null)
        {
            DNSClient.DeleteDnsRecord(service.hostName);
            logger.LogInformation("Deleted DNS {service.hostName}", service.hostName);
        }
    }

    private async Task UpdateDNS(string serviceName)
    {
        try
        {
            var service = await serviceManagerClient.Get_Service_By_NameAsync(serviceName);


            logger.LogInformation("Update DNS {serviceName}", service.serviceName);

            logger.LogInformation("Adding CNAME '{name}' to zone 'pefi.co.uk' with content 'home.pefi.co.uk'", serviceName);
            DNSClient.AddCNAMERecord("pefi.co.uk", $"{service.hostName}", "home");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}