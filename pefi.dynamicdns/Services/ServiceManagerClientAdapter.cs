namespace pefi.dynamicdns.Services;

public class ServiceManagerClientAdapter(ServiceManagerClient client) : IServiceManagerClient
{
    public async Task<ServiceInfo> GetServiceByNameAsync(string serviceName)
    {
        var response = await client.Get_Service_By_NameAsync(serviceName);
        return new ServiceInfo(response.serviceName, response.hostName);
    }
}
