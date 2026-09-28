using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Infrastructure.Persistence;
using Shared.Pagination;
using Shared.Results;

namespace OrderService.Api.Features.Orders.Queries.GetOrderList;

internal sealed class GetOrderListHandler(AppDbContext db)
    : IRequestHandler<GetOrderListQuery, Result<PagedResult<OrderListDto>>>
{
    private const int MinPage = 1;
    private const int MinPageSize = 1;
    private const int MaxPageSize = 100;

    public async Task<Result<PagedResult<OrderListDto>>> Handle(GetOrderListQuery q, CancellationToken ct)
    {
        var page = Math.Max(q.Page, MinPage);
        var pageSize = Math.Clamp(q.PageSize, MinPageSize, MaxPageSize);

        var query = db.Orders.AsNoTracking();

        if (q.Status is { } status)
            query = query.Where(o => o.Status == status);

        var total = await query.CountAsync(ct);

        var offset = (long)(page - 1) * pageSize;
        if (offset > int.MaxValue)
            return Result.Ok(new PagedResult<OrderListDto>([], total, page, pageSize));

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .ThenByDescending(o => o.Id)
            .Skip((int)offset)
            .Take(pageSize)
            .Select(o => new OrderListDto(o.Id, o.CustomerName, o.TotalAmount, o.Status.ToString(), o.CreatedAt))
            .ToListAsync(ct);

        return Result.Ok(new PagedResult<OrderListDto>(items, total, page, pageSize));
    }
}
