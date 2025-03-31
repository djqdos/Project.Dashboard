using API.ConfigModels;
using API.SampleFeature;
using HealthChecks.UI.Client;
using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using RabbitMQ.Client;
using Shared.Events;

var builder = WebApplication.CreateBuilder();

var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .Build();


var rabbitMqConfig = new RabbitMqConfig();
builder.Configuration.GetSection("RabbitMq").Bind(rabbitMqConfig);

builder.Services.AddMassTransit(opt =>
{
    opt.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host($"rabbitmq-server-0.rabbitmq-nodes.project-dashboard-default.svc.cluster.local", rabbitMqConfig.VirtualHost, h =>
        {
            h.Username(rabbitMqConfig.Username);
            h.Password(rabbitMqConfig.Password);
        });
    });
});

builder.Services
    // the way to get rabbitmq working, apparently.
    .AddSingleton<IConnection>(r =>
    {
        var factory = new ConnectionFactory
        {
            Uri = new Uri($"{rabbitMqConfig.Url}")
        };
        return factory.CreateConnectionAsync().GetAwaiter().GetResult();
    })
    .AddHealthChecks()
    .AddProcessAllocatedMemoryHealthCheck(1024)
    .AddRabbitMQ();

var app = builder.Build();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});


app.MapGet("/", async (IPublishEndpoint endpoint) =>
{
    for (int i = 0; i < 10; i++)
    {
        var newMessage = new SampleEvent
        {
            Id = Guid.NewGuid(),
            Name = $"Item {i}"
        };

        await endpoint.Publish<SampleEvent>(newMessage);
        await Task.Delay(200);
    }
})
.DisableAntiforgery();


app.Run();