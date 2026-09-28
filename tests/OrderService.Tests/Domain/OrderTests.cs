using FluentAssertions;
using OrderService.Api.Domain.Entities;
using OrderService.Api.Domain.Enums;

namespace OrderService.Tests.Domain;

public sealed class OrderTests
{
    [Fact]
    public void TryCancel_PendingOrder_ReturnsTrueAndCancels()
    {
        var order = Order.Create("Ada", 150m, DateTime.UtcNow);

        var cancelled = order.TryCancel();

        cancelled.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void TryCancel_CancelledOrder_ReturnsFalseAndKeepsStatus()
    {
        var order = Order.Create("Ada", 150m, DateTime.UtcNow);
        order.TryCancel();

        var cancelled = order.TryCancel();

        cancelled.Should().BeFalse();
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void TryCancel_CompletedOrder_ReturnsFalseAndKeepsStatus()
    {
        var order = Order.Create("Ada", 150m, DateTime.UtcNow);
        order.TryComplete();

        var cancelled = order.TryCancel();

        cancelled.Should().BeFalse();
        order.Status.Should().Be(OrderStatus.Completed);
    }

    [Fact]
    public void TryComplete_PendingOrder_ReturnsTrueAndCompletes()
    {
        var order = Order.Create("Ada", 150m, DateTime.UtcNow);

        var completed = order.TryComplete();

        completed.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Completed);
    }

    [Fact]
    public void TryComplete_CancelledOrder_ReturnsFalseAndKeepsStatus()
    {
        var order = Order.Create("Ada", 150m, DateTime.UtcNow);
        order.TryCancel();

        var completed = order.TryComplete();

        completed.Should().BeFalse();
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void TryComplete_CompletedOrder_ReturnsFalseAndKeepsStatus()
    {
        var order = Order.Create("Ada", 150m, DateTime.UtcNow);
        order.TryComplete();

        var completed = order.TryComplete();

        completed.Should().BeFalse();
        order.Status.Should().Be(OrderStatus.Completed);
    }
}
