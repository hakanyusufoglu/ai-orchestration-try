using MediatR;
using Shared.Results;

namespace OrderService.Api.Features.Orders.Queries.GetOrderById;

public sealed record GetOrderByIdQuery(int Id) : IRequest<Result<OrderDetailDto>>;
