// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using pefi.Rabbit;
using pefi.observability;
using pefi.dynamicdns;
using pefi.servicemanager;
using OpenTelemetry.Trace;

Console.WriteLine($"{DateTime.UtcNow}: PeFi.Dynamic.DNS Started");

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<ScheduledIpCheck>();
builder.Services.AddHostedService<EventListener>();
builder.Services.AddPefiObservability("http://192.168.0.5:4317", t => t
    .AddSource("MongoDB.Driver.Core.Extensions.DiagnosticSources")
    .AddRabbitMQInstrumentation());
builder.Logging.AddPefiLogging();

builder.Services.AddPeFiPersistance("mongodb://192.168.0.5:27017");
builder.Services.AddSingleton<IMessageBroker>(sp => new MessageBroker("192.168.0.5", "username", "password"));
builder.Services.AddSingleton<IDNSClient>(_ => new DNSimpleClient("pefi.co.uk"));
var host = builder.Build();
await host.RunAsync();

Console.ReadLine();

