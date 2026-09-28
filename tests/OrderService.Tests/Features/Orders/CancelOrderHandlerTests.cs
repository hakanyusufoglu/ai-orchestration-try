using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Domain.Entities;
using OrderService.Api.Domain.Enums;
using OrderService.Api.Features.Orders.Commands.CancelOrder;
using Shared.Errors;

namespace OrderService.Tests.Features.Orders;

public sealed class CancelOrderHandlerTests
{
    [Fact]
    public async Task Handle_PendingOrder_CancelsOrder()
    {
        await using var db = TestDbContextFactory.Create();
        var order = Order.Create("Ada", 150m, DateTime.UtcNow);
        db.Orders.Add(order);
        await db.SaveChangesAsync();
        var handler = new CancelOrderHandler(db);

        var result = await handler.Handle(new CancelOrderCommand(order.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var persisted = await db.Orders.AsNoTracking().SingleAsync(o => o.Id == order.Id);
        persisted.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_MissingOrder_ReturnsNotFoundError()
    {
        await using var db = TestDbContextFactory.Create();
        var handler = new CancelOrderHandler(db);

        var result = await handler.Handle(new CancelOrderCommand(999), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(AppErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_AlreadyCancelledOrder_ReturnsConflictError()
    {
        await using var db = TestDbContextFactory.Create();
        var order = Order.Create("Ada", 150m, DateTime.UtcNow);
        order.TryCancel();
        db.Orders.Add(order);
        await db.SaveChangesAsync();
        var handler = new CancelOrderHandler(db);

        var result = await handler.Handle(new CancelOrderCommand(order.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(AppErrorType.Conflict);
    }
}
