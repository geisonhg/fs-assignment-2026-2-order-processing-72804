using Inventory.Service;
using Inventory.Service.Data;
using Inventory.Service.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Serilog;

var host = Host.CreateDefaultBuilder(args)
    .UseSerilog((ctx, services, cfg) => cfg
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName())
    .ConfigureServices((ctx, services) =>
    {
        var conn = ctx.Configuration.GetConnectionString("DefaultConnection")!;
        if (ctx.HostingEnvironment.IsDevelopment())
            services.AddDbContext<InventoryDbContext>(o => o.UseSqlite(conn));
        else
            services.AddDbContext<InventoryDbContext>(o => o.UseSqlServer(conn));

        services.AddScoped<InventoryChecker>();
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
