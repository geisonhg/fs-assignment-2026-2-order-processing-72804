using Microsoft.EntityFrameworkCore;
using OrderManagement.API.Data;
using OrderManagement.API.Mapping;
using OrderManagement.API.Messaging;
using OrderManagement.API.Messaging.Consumers;
using OrderManagement.API.Services;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting OrderManagement.API");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, services, cfg) => cfg
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName());

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
    if (builder.Environment.IsDevelopment())
        builder.Services.AddDbContext<OrderDbContext>(o => o.UseSqlite(connectionString));
    else
        builder.Services.AddDbContext<OrderDbContext>(o => o.UseSqlServer(connectionString));

    builder.Services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

    builder.Services.AddAutoMapper(typeof(MappingProfile));

    builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
    builder.Services.AddMemoryCache();
    builder.Services.AddScoped<IStripeService, StripeService>();

    builder.Services.AddHostedService<InventoryResultConsumer>();
    builder.Services.AddHostedService<PaymentResultConsumer>();
    builder.Services.AddHostedService<ShippingResultConsumer>();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddCors(o => o.AddPolicy("AllowAll", p =>
        p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

    var app = builder.Build();

    SeedData.Initialize(app.Services);

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseSerilogRequestLogging();
    app.UseCors("AllowAll");
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "OrderManagement.API failed to start");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
