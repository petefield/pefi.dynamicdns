using System.Net.Http.Json;
using pefi.dynamicdns.Services;

public class IpAddressLookup(HttpClient httpClient) : IIpAddressLookup
{
    private const string IpifyUrl = "https://api4.ipify.org?format=json";

    public async Task<IPAddressInfo> GetPublicIpAddress()
    {
        var ipAddressResponse = await httpClient.GetFromJsonAsync<IPAddressInfo>(IpifyUrl);

        if (ipAddressResponse == null)
        {
            throw new InvalidOperationException("Failed to retrieve IP address from ipify.");
        }

        return ipAddressResponse;
    }
}
