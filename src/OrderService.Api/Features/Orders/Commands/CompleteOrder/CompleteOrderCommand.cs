using MediatR;
using Shared.Results;

namespace OrderService.Api.Features.Orders.Commands.CompleteOrder;

public sealed record CompleteOrderCommand(int Id) : IRequest<Result<Unit>>;
