using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Moq;
using Xunit;

namespace pefi.dynamicdns.Tests;

public class IpAddressLookupTests
{
    private static HttpClient CreateMockHttpClient(string responseBody, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var handler = new MockHttpMessageHandler(responseBody, statusCode);
        return new HttpClient(handler) { BaseAddress = new Uri("https://api4.ipify.org") };
    }

    [Fact]
    public async Task GetPublicIpAddress_ReturnsIpAddress_WhenResponseIsValid()
    {
        var expectedIp = "1.2.3.4";
        var responseBody = JsonSerializer.Serialize(new { ip = expectedIp });
        var httpClient = CreateMockHttpClient(responseBody);

        var lookup = new IpAddressLookup(httpClient);

        var result = await lookup.GetPublicIpAddress();

        Assert.NotNull(result);
        Assert.Equal(expectedIp, result.Ip);
    }

    [Fact]
    public async Task GetPublicIpAddress_ThrowsException_WhenResponseIsNull()
    {
        var httpClient = CreateMockHttpClient("null");

        var lookup = new IpAddressLookup(httpClient);

        await Assert.ThrowsAsync<InvalidOperationException>(() => lookup.GetPublicIpAddress());
    }

    [Fact]
    public async Task GetPublicIpAddress_ThrowsException_WhenRequestFails()
    {
        var handler = new MockHttpMessageHandler("Error", HttpStatusCode.InternalServerError);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api4.ipify.org") };

        var lookup = new IpAddressLookup(httpClient);

        await Assert.ThrowsAnyAsync<Exception>(() => lookup.GetPublicIpAddress());
    }

    private sealed class MockHttpMessageHandler(string responseBody, HttpStatusCode statusCode) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(responseBody, System.Text.Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }
}
