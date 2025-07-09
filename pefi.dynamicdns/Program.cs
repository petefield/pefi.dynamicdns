// See https://aka.ms/new-console-template for more information
using DnsClient.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

builder.Services.AddPefiObservability("http://192.168.0.42:4317", t => t
    .AddRabbitMQInstrumentation());

builder.Logging.AddPefiLogging();

builder.Services.AddHttpClient<ServiceManagerClient>(c => c.BaseAddress = new Uri("http://192.168.1.86:5550"));

builder.Services.AddPeFiMessaging(options => {
    options.Username = builder.Configuration.GetSection("Messaging").GetValue<string>("username") ?? "";
    options.Password = builder.Configuration.GetSection("Messaging").GetValue<string>("password") ?? "";
    options.Address = builder.Configuration.GetSection("Messaging").GetValue<string>("address") ?? "";
});




builder.Services.AddSingleton<IDNSClient>(sp => new DNSimpleClient("pefi.co.uk", sp.GetRequiredService<ILogger<DNSimpleClient>>()));
var host = builder.Build();
await host.RunAsync();

Console.ReadLine();

