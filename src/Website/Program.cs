using HealthChecks.UI.Client;
using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Website.Components;
using Website.ConfigModels;
using Website.Consumers;
using Website.Hubs;

var builder = WebApplication.CreateBuilder(args);

var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .Build();


builder.Services.AddSignalR();

builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        ["application/octet-stream"]);
});


var rabbitMqConfig = new RabbitMqConfig();
builder.Configuration.GetSection("RabbitMq").Bind(rabbitMqConfig);

builder.Services.AddMassTransit(opt =>
{
    opt.AddConsumer<SampleConsumer>();

    opt.UsingRabbitMq((context, cfg) =>
    { 
        cfg.Host($"rabbitmq-server-0.rabbitmq-nodes.project-dashboard-default.svc.cluster.local", rabbitMqConfig.VirtualHost, h =>
        {
            h.Username(rabbitMqConfig.Username);
            h.Password(rabbitMqConfig.Password);
        });

        cfg.ConfigureEndpoints(context);
    });
});




builder.Services.AddStackExchangeRedisCache(opt =>
{
    opt.Configuration = builder.Configuration.GetValue<string>("redis");
    opt.InstanceName = "Dashboard_";
});
builder.Services.AddHybridCache(opt =>
{
    opt.DefaultEntryOptions = new Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryOptions
    {
        LocalCacheExpiration = TimeSpan.FromSeconds(30)
    };
});


// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


//builder.Services.AddAntiforgery();
builder.Services.AddAntiforgery(options => 
    { 
        options.SuppressXFrameOptionsHeader = true;
        options.Cookie.Expiration = TimeSpan.Zero; 
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
    .AddProcessAllocatedMemoryHealthCheck(1024, name: "Memory")
    .AddRedis(builder.Configuration.GetValue<string>("redis"))
    .AddSignalRHub("http://localhost:8080/samplehub")
    .AddRabbitMQ();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .DisableAntiforgery()
    .AddInteractiveServerRenderMode();




app.MapHub<SampleHub>("/samplehub");

app.MapHealthChecksUI();
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

 
app.Run();
