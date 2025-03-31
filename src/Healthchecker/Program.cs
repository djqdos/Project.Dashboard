using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder();

builder.Services.AddAntiforgery();

// builder.Services
//     .AddHealthChecksUI(setupSettings: s =>
//     {
//         s.AddHealthCheckEndpoint("Website", "http://website.project-dashboard-default.svc.cluster.local:8080/health");
//         s.AddHealthCheckEndpoint("API", "http://api.project-dashboard-default.svc.cluster.local:9000/health");
//     })
//     .AddInMemoryStorage();
builder.Services.AddHealthChecksUI().AddInMemoryStorage();


var app = builder.Build();
app.UseAntiforgery();
app.MapStaticAssets();

app.UseRouting().UseEndpoints(conf =>
{
    conf.MapHealthChecksUI(setup =>
    {
        setup.AddCustomStylesheet("styles.css");
    });
    //conf.MapHealthChecks("/health", new HealthCheckOptions
    //{
    //    Predicate = _ => true,
    //    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    //});
});

app.Run();
 