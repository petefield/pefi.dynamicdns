using Xunit;

namespace pefi.dynamicdns.Tests;

public class ModelTests
{
    [Fact]
    public void IPAddressInfo_StoresIp()
    {
        var info = new IPAddressInfo("1.2.3.4");
        Assert.Equal("1.2.3.4", info.Ip);
    }

    [Fact]
    public void IPAddressInfo_EqualityIsValueBased()
    {
        var a = new IPAddressInfo("1.2.3.4");
        var b = new IPAddressInfo("1.2.3.4");
        var c = new IPAddressInfo("5.6.7.8");

        Assert.Equal(a, b);
        Assert.NotEqual(a, c);
    }

    [Fact]
    public void DnsSettings_DefaultsAreEmpty()
    {
        var settings = new DnsSettings();
        Assert.Equal("", settings.Domain);
        Assert.Equal("", settings.ProxyRecordName);
    }

    [Fact]
    public void DnsSettings_CheckIntervalMinutes_DefaultsToTwo()
    {
        var settings = new DnsSettings();
        Assert.Equal(2, settings.CheckIntervalMinutes);
    }

    [Fact]
    public void DnsSettings_PropertiesAreAssignable()
    {
        var settings = new DnsSettings { Domain = "example.com", ProxyRecordName = "home" };
        Assert.Equal("example.com", settings.Domain);
        Assert.Equal("home", settings.ProxyRecordName);
    }


}
