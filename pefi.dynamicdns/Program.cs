// See https://aka.ms/new-console-template for more information
using DnsClient.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Trace;
using pefi;
using pefi.dynamicdns;
using pefi.dynamicdns.Infrastructure;
using pefi.dynamicdns.Infrastructure.DNSimple;
using pefi.dynamicdns.Services;
using pefi.observability;
using pefi.Rabbit;

Console.WriteLine($"{DateTime.UtcNow}: PeFi.Dynamic.DNS Started");

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddHostedService<ScheduledIpCheck>();
builder.Services.AddHostedService<EventListener>();

var collectorUrl = builder.Configuration.GetSection("Observability").GetValue<string>("CollectorUrl") ?? "";
builder.Services.AddPefiObservability(collectorUrl, t => t
    .AddRabbitMQInstrumentation());

builder.Logging.AddPefiLogging();

builder.Services.AddHttpClient<ServiceManagerClient>((sp, c) => {
    var baseAddress = builder.Configuration.GetSection("ServiceManager").GetValue<string>("baseurl") ?? "";
    c.BaseAddress = new Uri(baseAddress);
});


builder.Services.AddPeFiMessaging(options => {
    options.Username = builder.Configuration.GetSection("Messaging").GetValue<string>("username") ?? "";
    options.Password = builder.Configuration.GetSection("Messaging").GetValue<string>("password") ?? "";
    options.Address = builder.Configuration.GetSection("Messaging").GetValue<string>("address") ?? "";
});




builder.Services.Configure<DnsSettings>(builder.Configuration.GetSection("DNS"));

builder.Services.AddSingleton<IDNSClient>(sp => {
    var dnsSettings = sp.GetRequiredService<IOptions<DnsSettings>>().Value;
    return new DNSimpleClient(dnsSettings.Domain, sp.GetRequiredService<ILogger<DNSimpleClient>>());
});
var host = builder.Build();
await host.RunAsync();

Console.ReadLine();

