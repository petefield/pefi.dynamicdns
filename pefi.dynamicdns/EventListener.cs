using dnsimple.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using pefi.dynamicdns.Infrastructure;
using pefi.dynamicdns.Models;
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
            await UpdateDNS(svc.Name);
        });

        await _topic.Subscribe("events.service.deleted", async (key, message) => {
            var svc = System.Text.Json.JsonSerializer.Deserialize<ServiceDeletedMessage>(message);
            await DeleteDNs(svc);
        });
    }

    private async Task DeleteDNs(ServiceDeletedMessage svgMessage )
    {
        try
        {
            var service = svgMessage.Service;

            logger.LogInformation("Delete DNS {serviceName}", service.ServiceName);

            if (service.HostName is not null)
            {
                DNSClient.DeleteDnsRecord(service.HostName);
                logger.LogInformation("Deleted DNS {service.hostName}", service.HostName);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Deleting DNS {service}", svgMessage);
            throw;
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