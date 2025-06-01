using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using pefi.dynamicdns.Persistance;
using pefi.Rabbit;
using pefi.servicemanager;

namespace pefi.dynamicdns;

public class EventListener(ILogger<EventListener> logger,
    IMessageBroker messageBroker, 
    IDataStore dataStore,
    IDNSClient DNSClient) : BackgroundService
{
    private ITopic? _topic;
    private readonly string databaseName = "ServiceDb";
    private readonly string serviceCollectionName = "services";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Dynamic DNS Service is listening for new services.");
        
        _topic = await messageBroker.CreateTopic("Events");

        await _topic.Subscribe("#", async (key, message) 
            => await UpdateDNS(message));
    }

    private async Task UpdateDNS(string serviceName)
    {
        logger.LogInformation("Update DNS {serviceName}", serviceName);
        var service = await dataStore.Get<ServiceDescription>(databaseName, serviceCollectionName, s => s.ServiceName == serviceName);

        logger.LogInformation("Adding CNAME '{name}' to zone 'pefi.co.uk' with content 'home.pefi.co.uk'", service.HostName);
        DNSClient.AddCNAMERecord("pefi.co.uk", $"{service.HostName}", "home");
    }
}