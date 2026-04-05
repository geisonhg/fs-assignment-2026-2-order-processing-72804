using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OrderManagement.API.CQRS.Queries;
using OrderManagement.API.Data;
using OrderManagement.API.Domain.Entities;
using OrderManagement.API.Mapping;
using Shared.Contracts.Enums;

namespace OrderManagement.Tests.Queries;

public class GetOrderByIdHandlerTests
{
    private static OrderDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new OrderDbContext(options);
    }

    private static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        return config.CreateMapper();
    }

    [Fact]
    public async Task GetOrderById_ExistingOrder_ReturnsOrderDto()
    {
        var db     = CreateInMemoryDb();
        var mapper = CreateMapper();

        var customer = new Customer { Name = "Test", Email = "test@test.com" };
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        var order = new Order
        {
            OrderReference  = "ORD-20260401-TEST01",
            CorrelationId   = Guid.NewGuid().ToString(),
            CustomerId      = customer.CustomerId,
            Status          = OrderStatus.Completed,
            TotalAmount     = 100m,
            ShippingName    = "Test", ShippingLine1 = "1 St", ShippingCity = "Dublin", ShippingCountry = "IE"
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var handler = new GetOrderByIdHandler(db, mapper);
        var result  = await handler.Handle(new GetOrderByIdQuery(order.OrderId), CancellationToken.None);

        result.Should().NotBeNull();
        result!.OrderReference.Should().Be("ORD-20260401-TEST01");
        result.CustomerName.Should().Be("Test");
        result.Status.Should().Be(OrderStatus.Completed);
    }

    [Fact]
    public async Task GetOrderById_NonExistentOrder_ReturnsNull()
    {
        var db     = CreateInMemoryDb();
        var mapper = CreateMapper();

        var handler = new GetOrderByIdHandler(db, mapper);
        var result  = await handler.Handle(new GetOrderByIdQuery(999), CancellationToken.None);

        result.Should().BeNull();
    }
}
