using MediatR;
using OrderService.Api.Domain.Enums;
using Shared.Pagination;
using Shared.Results;

namespace OrderService.Api.Features.Orders.Queries.GetOrderList;

public sealed record GetOrderListQuery(int Page = 1, int PageSize = 20, OrderStatus? Status = null)
    : IRequest<Result<PagedResult<OrderListDto>>>;
