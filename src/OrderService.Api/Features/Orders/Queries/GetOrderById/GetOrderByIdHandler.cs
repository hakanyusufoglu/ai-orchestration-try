using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Domain.Entities;
using OrderService.Api.Infrastructure.Persistence;
using Shared.Errors;
using Shared.Results;

namespace OrderService.Api.Features.Orders.Queries.GetOrderById;

internal sealed class GetOrderByIdHandler(AppDbContext db)
    : IRequestHandler<GetOrderByIdQuery, Result<OrderDetailDto>>
{
    public async Task<Result<OrderDetailDto>> Handle(GetOrderByIdQuery q, CancellationToken ct)
    {
        var order = await db.Orders
            .AsNoTracking()
            .Where(o => o.Id == q.Id)
            .Select(o => new OrderDetailDto(o.Id, o.CustomerName, o.TotalAmount, o.Status.ToString(), o.CreatedAt))
            .FirstOrDefaultAsync(ct);

        if (order is null)
            return Result.Fail<OrderDetailDto>(AppError.NotFound(nameof(Order), q.Id));

        return Result.Ok(order);
    }
}
