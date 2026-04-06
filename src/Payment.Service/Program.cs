using Microsoft.Extensions.Hosting;
using Payment.Service;
using Payment.Service.Services;
using Serilog;

var host = Host.CreateDefaultBuilder(args)
    .UseSerilog((ctx, services, cfg) => cfg
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName())
    .ConfigureServices((_, services) =>
    {
        services.AddScoped<PaymentProcessor>();
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
