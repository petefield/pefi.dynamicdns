using pefi.dynamicdns.Models;

namespace pefi.dynamicdns.Services;

public interface IDnsMessageHandler
{
    Task HandleServiceCreated(string serviceName);
    Task HandleServiceDeleted(ServiceDeletedMessage message);
}
