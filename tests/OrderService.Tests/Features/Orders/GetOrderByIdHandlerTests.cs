using FluentAssertions;
using OrderService.Api.Domain.Entities;
using OrderService.Api.Features.Orders.Queries.GetOrderById;
using Shared.Errors;

namespace OrderService.Tests.Features.Orders;

public sealed class GetOrderByIdHandlerTests
{
    [Fact]
    public async Task Handle_ExistingOrder_ReturnsOk()
    {
        await using var db = TestDbContextFactory.Create();
        var order = Order.Create("Ada", 150m, DateTime.UtcNow);
        db.Orders.Add(order);
        await db.SaveChangesAsync();
        var handler = new GetOrderByIdHandler(db);

        var result = await handler.Handle(new GetOrderByIdQuery(order.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.CustomerName.Should().Be("Ada");
    }

    [Fact]
    public async Task Handle_MissingOrder_ReturnsNotFoundError()
    {
        await using var db = TestDbContextFactory.Create();
        var handler = new GetOrderByIdHandler(db);

        var result = await handler.Handle(new GetOrderByIdQuery(999), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(AppErrorType.NotFound);
    }
}
