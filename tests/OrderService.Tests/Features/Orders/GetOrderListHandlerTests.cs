using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Domain.Entities;
using OrderService.Api.Domain.Enums;
using OrderService.Api.Features.Orders.Queries.GetOrderList;
using OrderService.Api.Infrastructure.Persistence;

namespace OrderService.Tests.Features.Orders;

public sealed class GetOrderListHandlerTests
{
    private static readonly DateTime BaseTime = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static async Task SeedAsync(AppDbContext db, int count, int cancelledEvery = 0)
    {
        for (var i = 0; i < count; i++)
        {
            var order = Order.Create($"Customer{i}", 100m + i, BaseTime.AddMinutes(i));
            if (cancelledEvery > 0 && i % cancelledEvery == 0)
                order.TryCancel();
            db.Orders.Add(order);
        }

        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_SecondPage_ReturnsRequestedPageItems()
    {
        await using var db = TestDbContextFactory.Create();
        await SeedAsync(db, 25);
        var handler = new GetOrderListHandler(db);

        var result = await handler.Handle(new GetOrderListQuery(Page: 2, PageSize: 10), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(10);
        result.Value.Page.Should().Be(2);
        result.Value.PageSize.Should().Be(10);
        result.Value.Items[0].CustomerName.Should().Be("Customer14");
    }

    [Fact]
    public async Task Handle_LastPage_ReturnsRemainingItems()
    {
        await using var db = TestDbContextFactory.Create();
        await SeedAsync(db, 25);
        var handler = new GetOrderListHandler(db);

        var result = await handler.Handle(new GetOrderListQuery(Page: 3, PageSize: 10), CancellationToken.None);

        result.Value!.Items.Should().HaveCount(5);
    }

    [Fact]
    public async Task Handle_StatusFilter_ReturnsOnlyMatchingOrders()
    {
        await using var db = TestDbContextFactory.Create();
        await SeedAsync(db, 10, cancelledEvery: 2);
        var handler = new GetOrderListHandler(db);

        var result = await handler.Handle(new GetOrderListQuery(Status: OrderStatus.Cancelled), CancellationToken.None);

        result.Value!.Items.Should().HaveCount(5)
            .And.OnlyContain(o => o.Status == nameof(OrderStatus.Cancelled));
    }

    [Fact]
    public async Task Handle_StatusFilterWithPaging_TotalCountReflectsFilteredTotal()
    {
        await using var db = TestDbContextFactory.Create();
        await SeedAsync(db, 12, cancelledEvery: 3);
        var handler = new GetOrderListHandler(db);

        var result = await handler.Handle(
            new GetOrderListQuery(Page: 1, PageSize: 5, Status: OrderStatus.Pending), CancellationToken.None);

        result.Value!.Items.Should().HaveCount(5);
        result.Value.TotalCount.Should().Be(8);
    }

    [Fact]
    public async Task Handle_MultipleOrders_ReturnsNewestFirst()
    {
        await using var db = TestDbContextFactory.Create();
        await SeedAsync(db, 5);
        var handler = new GetOrderListHandler(db);

        var result = await handler.Handle(new GetOrderListQuery(), CancellationToken.None);

        result.Value!.Items.Select(o => o.CreatedAt).Should().BeInDescendingOrder();
        result.Value.Items[0].CreatedAt.Should().Be(BaseTime.AddMinutes(4));
    }

    [Fact]
    public async Task Handle_SameCreatedAtAcrossPageBoundary_ReturnsEachOrderExactlyOnce()
    {
        await using var db = TestDbContextFactory.Create();
        for (var i = 0; i < 6; i++)
            db.Orders.Add(Order.Create($"Customer{i}", 100m, BaseTime));
        await db.SaveChangesAsync();
        var handler = new GetOrderListHandler(db);

        var first = await handler.Handle(new GetOrderListQuery(Page: 1, PageSize: 3), CancellationToken.None);
        var second = await handler.Handle(new GetOrderListQuery(Page: 2, PageSize: 3), CancellationToken.None);

        var allIds = await db.Orders.Select(o => o.Id).ToListAsync();
        var pagedIds = first.Value!.Items.Concat(second.Value!.Items).Select(o => o.Id).ToList();
        pagedIds.Should().OnlyHaveUniqueItems()
            .And.BeEquivalentTo(allIds);
        pagedIds.Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task Handle_PageBelowMin_ClampsToOne()
    {
        await using var db = TestDbContextFactory.Create();
        await SeedAsync(db, 3);
        var handler = new GetOrderListHandler(db);

        var result = await handler.Handle(new GetOrderListQuery(Page: 0), CancellationToken.None);

        result.Value!.Page.Should().Be(1);
        result.Value.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_PageSizeBelowMin_ClampsToOne()
    {
        await using var db = TestDbContextFactory.Create();
        await SeedAsync(db, 3);
        var handler = new GetOrderListHandler(db);

        var result = await handler.Handle(new GetOrderListQuery(PageSize: 0), CancellationToken.None);

        result.Value!.PageSize.Should().Be(1);
        result.Value.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_PageOffsetBeyondIntRange_ReturnsEmptyPageWithTotalCount()
    {
        await using var db = TestDbContextFactory.Create();
        await SeedAsync(db, 3);
        var handler = new GetOrderListHandler(db);

        var result = await handler.Handle(
            new GetOrderListQuery(Page: int.MaxValue, PageSize: 100), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(3);
        result.Value.Page.Should().Be(int.MaxValue);
    }

    [Fact]
    public async Task Handle_PageSizeAboveMax_ClampsToMax()
    {
        await using var db = TestDbContextFactory.Create();
        await SeedAsync(db, 120);
        var handler = new GetOrderListHandler(db);

        var result = await handler.Handle(new GetOrderListQuery(PageSize: 500), CancellationToken.None);

        result.Value!.PageSize.Should().Be(100);
        result.Value.Items.Should().HaveCount(100);
        result.Value.TotalCount.Should().Be(120);
    }
}
