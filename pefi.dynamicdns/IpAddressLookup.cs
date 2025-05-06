// See https://aka.ms/new-console-template for more information
using System.Net.Http.Json;

public static class IpAddressLookup
{
    public static async Task<IPAddressInfo> GetPublicIpAddress()
    {
        using var httpClient = new HttpClient();
        var IpAddressResponse = await httpClient.GetFromJsonAsync<IPAddressInfo>("https://api4.ipify.org?format=json");

        if (IpAddressResponse == null)
        {
            throw new Exception("Failed to retrieve IP address");
        }

        return IpAddressResponse;
    }
}
