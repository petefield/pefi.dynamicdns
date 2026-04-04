namespace pefi.dynamicdns.Services;

public record ServiceInfo(string? ServiceName, string? HostName);

public interface IServiceManagerClient
{
    Task<ServiceInfo> GetServiceByNameAsync(string serviceName);
}
