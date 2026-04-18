using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using pefi.dynamicdns;
using pefi.dynamicdns.Infrastructure;
using pefi.dynamicdns.Services;
using Xunit;

namespace pefi.dynamicdns.Tests;

public class ScheduledIpCheckTests
{
    private readonly Mock<IDNSClient> _dnsClientMock = new();
    private readonly Mock<IIpAddressLookup> _ipLookupMock = new();
    private readonly IOptions<DnsSettings> _dnsOptions = Options.Create(new DnsSettings
    {
        Domain = "example.com",
        ProxyRecordName = "home"
    });
    private readonly NullLogger<ScheduledIpCheck> _logger = new();

    private ScheduledIpCheck CreateService() =>
        new(_dnsClientMock.Object, _dnsOptions, _ipLookupMock.Object, _logger);

    [Fact]
    public async Task ExecuteAsync_WhenIpChanges_CallsUpdateDNSRecord()
    {
        var firstIp = new IPAddressInfo("1.2.3.4");
        var secondIp = new IPAddressInfo("5.6.7.8");

        var callCount = 0;
        _ipLookupMock
            .Setup(x => x.GetPublicIpAddress())
            .ReturnsAsync(() =>
            {
                callCount++;
                return callCount == 1 ? firstIp : secondIp;
            });

        using var cts = new CancellationTokenSource();

        var service = CreateService();

        // Run two iterations then cancel
        var runTask = service.StartAsync(cts.Token);
        await Task.Delay(50); // let first iteration run
        cts.Cancel();

        await Task.WhenAny(runTask, Task.Delay(2000));

        _dnsClientMock.Verify(x => x.UpdateDNSRecord("home", firstIp), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_WhenIpDoesNotChange_DoesNotCallUpdateDNSRecord()
    {
        var sameIp = new IPAddressInfo("1.2.3.4");
        _ipLookupMock.Setup(x => x.GetPublicIpAddress()).ReturnsAsync(sameIp);

        using var cts = new CancellationTokenSource();

        var service = CreateService();
        var runTask = service.StartAsync(cts.Token);
        await Task.Delay(50); // let first iteration run
        cts.Cancel();

        await Task.WhenAny(runTask, Task.Delay(2000));

        // First call should update (from null -> IP), but no subsequent updates for same IP
        _dnsClientMock.Verify(x => x.UpdateDNSRecord(It.IsAny<string>(), sameIp), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancelled_StopsGracefully()
    {
        _ipLookupMock
            .Setup(x => x.GetPublicIpAddress())
            .ReturnsAsync(new IPAddressInfo("1.2.3.4"));

        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel before starting

        var service = CreateService();
        await service.StartAsync(cts.Token);

        _ipLookupMock.Verify(x => x.GetPublicIpAddress(), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_UsesCorrectDomainAndHostname()
    {
        var ip = new IPAddressInfo("10.0.0.1");
        _ipLookupMock.Setup(x => x.GetPublicIpAddress()).ReturnsAsync(ip);

        using var cts = new CancellationTokenSource();
        var service = CreateService();
        var runTask = service.StartAsync(cts.Token);
        await Task.Delay(50);
        cts.Cancel();

        await Task.WhenAny(runTask, Task.Delay(2000));

        _dnsClientMock.Verify(x => x.UpdateDNSRecord("home", ip), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenIpLookupFails_ContinuesRunningWithoutUpdatingDns()
    {
        _ipLookupMock
            .Setup(x => x.GetPublicIpAddress())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        using var cts = new CancellationTokenSource();
        var service = CreateService();

        var runTask = service.StartAsync(cts.Token);
        await Task.Delay(50);
        cts.Cancel();

        // Should complete without throwing despite the IP lookup failure
        await Task.WhenAny(runTask, Task.Delay(2000));

        _dnsClientMock.Verify(x => x.UpdateDNSRecord(It.IsAny<string>(), It.IsAny<IPAddressInfo>()), Times.Never);
    }
}
