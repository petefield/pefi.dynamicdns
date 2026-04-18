using pefi.dynamicdns;
using pefi.dynamicdns.Models;
using pefi.dynamicdns.Services;
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

    [Fact]
    public void ServiceCreatedMessage_StoresName()
    {
        var message = new ServiceCreatedMessage("my-service");
        Assert.Equal("my-service", message.Name);
    }

    [Fact]
    public void ServiceDeletedMessage_StoresService()
    {
        var service = new Service("my-service", "my-host", "80", "8080", "docker/image:latest");
        var message = new ServiceDeletedMessage(service);

        Assert.Equal(service, message.Service);
    }

    [Fact]
    public void Service_StoresAllProperties()
    {
        var service = new Service("my-service", "my-host", "80", "8080", "docker/image:latest");

        Assert.Equal("my-service", service.ServiceName);
        Assert.Equal("my-host", service.HostName);
        Assert.Equal("80", service.ContainerPortNumber);
        Assert.Equal("8080", service.HostPortNumber);
        Assert.Equal("docker/image:latest", service.DockerImageUrl);
    }

    [Fact]
    public void Service_AllowsNullOptionalProperties()
    {
        var service = new Service("my-service", null, null, null, null);

        Assert.Equal("my-service", service.ServiceName);
        Assert.Null(service.HostName);
        Assert.Null(service.ContainerPortNumber);
        Assert.Null(service.HostPortNumber);
        Assert.Null(service.DockerImageUrl);
    }

    [Fact]
    public void ServiceInfo_StoresServiceNameAndHostName()
    {
        var info = new ServiceInfo("my-service", "my-host");
        Assert.Equal("my-service", info.ServiceName);
        Assert.Equal("my-host", info.HostName);
    }

    [Fact]
    public void ServiceInfo_AllowsNullValues()
    {
        var info = new ServiceInfo(null, null);
        Assert.Null(info.ServiceName);
        Assert.Null(info.HostName);
    }
}
