using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using pefi.Rabbit;

namespace PeFi.Proxy;

public class EventListener(ILogger<EventListener> logger,
    IMessageBroker messageBroker) : BackgroundService
{
    private ITopic? _topic;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Dynamic DNS Service is listenting for new services.");
        
        _topic = await messageBroker.CreateTopic("Events");

        await _topic.Subscribe("#", async (key, message) 
            => await UpdateDNS());
    }

    private async Task UpdateDNS()
    {
        logger.LogInformation("Update DNS");
        await Task.CompletedTask;
    }
}