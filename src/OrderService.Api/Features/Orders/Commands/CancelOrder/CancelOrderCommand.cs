using MediatR;
using Shared.Results;

namespace OrderService.Api.Features.Orders.Commands.CancelOrder;

public sealed record CancelOrderCommand(int Id) : IRequest<Result<Unit>>;
