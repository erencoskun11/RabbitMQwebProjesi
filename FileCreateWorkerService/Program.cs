// Program.cs
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using FileCreateWorkerService;
using Microsoft.Extensions.Options;
using FileCreateWorkerService.Models;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

// Configuration
var config = builder.Configuration;

// Try to read AMQP URI from either ConnectionStrings:RabbitMQ or RabbitMq:Url
string? amqpUri = config.GetConnectionString("RabbitMQ") ?? config["RabbitMq:Url"];
if (string.IsNullOrWhiteSpace(amqpUri))
{
    throw new InvalidOperationException(
        "RabbitMQ connection string not found. Set ConnectionStrings:RabbitMQ or RabbitMq:Url in configuration.");
}

builder.Services.AddDbContext<AdventureWorks2019Context>(options =>
{
    options.UseSqlServer(config.GetConnectionString("SqlServer"));
});



// Register ConnectionFactory as singleton
builder.Services.AddSingleton(sp =>
{
    var factory = new ConnectionFactory
    {
        Uri = new Uri(amqpUri),
        DispatchConsumersAsync = true,
        AutomaticRecoveryEnabled = true
    };

    return factory;
});

// Register your Worker as hosted service
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

// Run the host (top-level await supported)
await host.RunAsync();
