// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using pefi.Rabbit;
using PeFi.Proxy;
using pefi.observability;

Console.WriteLine($"{DateTime.UtcNow}: PeFi.Dynamic.DNS Started");

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<ScheduledIpCheck>();
builder.Services.AddHostedService<EventListener>();
builder.Services.AddPefiObservability("http://192.168.0.5:4317");

builder.Services.AddSingleton<IMessageBroker>(sp => new MessageBroker("192.168.0.5", "username", "password"));
var host = builder.Build();
await host.RunAsync();

Console.ReadLine();

