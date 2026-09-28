using MediatR;
using OrderService.Api.Domain.Entities;
using OrderService.Api.Infrastructure.Persistence;
using Shared.Results;

namespace OrderService.Api.Features.Orders.Commands.CreateOrder;

internal sealed class CreateOrderHandler(AppDbContext db, TimeProvider clock)
    : IRequestHandler<CreateOrderCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateOrderCommand cmd, CancellationToken ct)
    {
        var order = Order.Create(cmd.CustomerName, cmd.TotalAmount, clock.GetUtcNow().UtcDateTime);
        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);
        return Result.Ok(order.Id);
    }
}
