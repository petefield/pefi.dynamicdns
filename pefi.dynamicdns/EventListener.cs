using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using pefi.dynamicdns.Models;
using pefi.dynamicdns.Services;
using pefi.Rabbit;

namespace pefi.dynamicdns;

public class EventListener(ILogger<EventListener> logger,
    IMessageBroker messageBroker,
    IDnsMessageHandler dnsMessageHandler) : BackgroundService
{
    private ITopic? _topic;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Dynamic DNS Service is listening for new services.");

        _topic = await messageBroker.CreateTopic("Events");

        await _topic.Subscribe("events.service.created", async (key, message) => {
            var svc = System.Text.Json.JsonSerializer.Deserialize<ServiceCreatedMessage>(message);
            if (svc is null)
            {
                logger.LogWarning("Received null or undeserializable service.created message, skipping.");
                return;
            }
            await dnsMessageHandler.HandleServiceCreated(svc.Name);
        });

        await _topic.Subscribe("events.service.deleted", async (key, message) => {
            var svc = System.Text.Json.JsonSerializer.Deserialize<ServiceDeletedMessage>(message);
            if (svc is null)
            {
                logger.LogWarning("Received null or undeserializable service.deleted message, skipping.");
                return;
            }
            await dnsMessageHandler.HandleServiceDeleted(svc);
        });
    }
}