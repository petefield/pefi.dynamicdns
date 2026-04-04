using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using pefi.dynamicdns;
using pefi.dynamicdns.Infrastructure;
using pefi.dynamicdns.Models;
using pefi.dynamicdns.Services;
using Xunit;

namespace pefi.dynamicdns.Tests;

public class DnsMessageHandlerTests
{
    private readonly Mock<IDNSClient> _dnsClientMock = new();
    private readonly Mock<IServiceManagerClient> _serviceManagerClientMock = new();
    private readonly IOptions<DnsSettings> _dnsOptions = Options.Create(new DnsSettings
    {
        Domain = "example.com",
        HomeHostname = "home"
    });
    private readonly NullLogger<DnsMessageHandler> _logger = new();

    private DnsMessageHandler CreateHandler() =>
        new(_dnsClientMock.Object, _serviceManagerClientMock.Object, _dnsOptions, _logger);

    [Fact]
    public async Task HandleServiceCreated_CallsAddCNAMERecord_WithCorrectParameters()
    {
        var serviceName = "myservice";
        var hostName = "myservice";
        _serviceManagerClientMock
            .Setup(x => x.GetServiceByNameAsync(serviceName))
            .ReturnsAsync(new ServiceInfo(serviceName, hostName));

        var handler = CreateHandler();
        await handler.HandleServiceCreated(serviceName);

        _dnsClientMock.Verify(x => x.AddCNAMERecord("example.com", hostName, "home"), Times.Once);
    }

    [Fact]
    public async Task HandleServiceCreated_PropagatesException_WhenServiceManagerFails()
    {
        _serviceManagerClientMock
            .Setup(x => x.GetServiceByNameAsync(It.IsAny<string>()))
            .ThrowsAsync(new HttpRequestException("Service not found"));

        var handler = CreateHandler();

        await Assert.ThrowsAsync<HttpRequestException>(() => handler.HandleServiceCreated("missing-service"));
    }

    [Fact]
    public async Task HandleServiceDeleted_CallsDeleteDnsRecord_WhenHostNameIsSet()
    {
        var service = new Service("myservice", "myservice", null, null, null);
        var message = new ServiceDeletedMessage(service);

        var handler = CreateHandler();
        await handler.HandleServiceDeleted(message);

        _dnsClientMock.Verify(x => x.DeleteDnsRecord("myservice"), Times.Once);
    }

    [Fact]
    public async Task HandleServiceDeleted_DoesNotCallDeleteDnsRecord_WhenHostNameIsNull()
    {
        var service = new Service("myservice", null, null, null, null);
        var message = new ServiceDeletedMessage(service);

        var handler = CreateHandler();
        await handler.HandleServiceDeleted(message);

        _dnsClientMock.Verify(x => x.DeleteDnsRecord(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleServiceCreated_WhenHostNameIsNull_DoesNotCallAddCNAMERecord()
    {
        var serviceName = "myservice";
        _serviceManagerClientMock
            .Setup(x => x.GetServiceByNameAsync(serviceName))
            .ReturnsAsync(new ServiceInfo(serviceName, null));

        var handler = CreateHandler();
        await handler.HandleServiceCreated(serviceName);

        _dnsClientMock.Verify(x => x.AddCNAMERecord(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleServiceDeleted_PropagatesException_WhenDnsClientFails()
    {
        var service = new Service("myservice", "myservice", null, null, null);
        var message = new ServiceDeletedMessage(service);

        _dnsClientMock
            .Setup(x => x.DeleteDnsRecord(It.IsAny<string>()))
            .Throws(new InvalidOperationException("DNS error"));

        var handler = CreateHandler();

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleServiceDeleted(message));
    }
}
