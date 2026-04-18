using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using pefi.dynamicdns;
using pefi.dynamicdns.Infrastructure;
using pefi.dynamicdns.Infrastructure.DNSimple;
using pefi.dynamicdns.Services;

Console.WriteLine($"{DateTime.UtcNow}: PeFi.Dynamic.DNS Started");

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddHostedService<ScheduledIpCheck>();

builder.Services.AddHttpClient<IIpAddressLookup, IpAddressLookup>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.Configure<DnsSettings>(builder.Configuration.GetSection("DNS"));

builder.Services.AddSingleton<IDNSClient>(sp => {
    var dnsSettings = sp.GetRequiredService<IOptions<DnsSettings>>().Value;
    if (string.IsNullOrWhiteSpace(dnsSettings.Domain))
        throw new InvalidOperationException("DNS:Domain configuration is required.");
    if (string.IsNullOrWhiteSpace(dnsSettings.ApiToken))
        throw new InvalidOperationException("DNS:ApiToken configuration is required.");
    return new DNSimpleClient(dnsSettings.Domain, dnsSettings.ApiToken, sp.GetRequiredService<ILogger<DNSimpleClient>>());
});

var host = builder.Build();
await host.RunAsync();

