using Serilog;
using Shipping.Service;
using Shipping.Service.Services;

var host = Host.CreateDefaultBuilder(args)
    .UseSerilog((ctx, services, cfg) => cfg
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName())
    .ConfigureServices((_, services) =>
    {
        services.AddScoped<ShipmentCreator>();
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
