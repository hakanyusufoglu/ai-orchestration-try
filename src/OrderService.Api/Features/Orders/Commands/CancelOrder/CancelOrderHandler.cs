using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Domain.Entities;
using OrderService.Api.Infrastructure.Persistence;
using Shared.Errors;
using Shared.Results;

namespace OrderService.Api.Features.Orders.Commands.CancelOrder;

internal sealed class CancelOrderHandler(AppDbContext db)
    : IRequestHandler<CancelOrderCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(CancelOrderCommand cmd, CancellationToken ct)
    {
        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == cmd.Id, ct);

        if (order is null)
            return Result.Fail<Unit>(AppError.NotFound(nameof(Order), cmd.Id));

        if (!order.TryCancel())
            return Result.Fail<Unit>(AppError.Conflict(
                $"'{nameof(Order)}' with id '{cmd.Id}' cannot be cancelled because its status is '{order.Status}'."));

        await db.SaveChangesAsync(ct);
        return Result.Ok(Unit.Value);
    }
}
