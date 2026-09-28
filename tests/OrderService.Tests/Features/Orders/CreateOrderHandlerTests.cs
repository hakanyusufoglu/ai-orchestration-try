using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Domain.Enums;
using OrderService.Api.Features.Orders.Commands.CreateOrder;

namespace OrderService.Tests.Features.Orders;

public sealed class CreateOrderHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_PersistsPendingOrder()
    {
        await using var db = TestDbContextFactory.Create();
        var handler = new CreateOrderHandler(db, TimeProvider.System);

        var result = await handler.Handle(new CreateOrderCommand("Ada", 150m), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var order = await db.Orders.SingleAsync(o => o.Id == result.Value);
        order.CustomerName.Should().Be("Ada");
        order.Status.Should().Be(OrderStatus.Pending);
    }
}
