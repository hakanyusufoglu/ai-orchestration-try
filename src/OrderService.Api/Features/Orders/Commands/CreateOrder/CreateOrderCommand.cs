using MediatR;
using Shared.Results;

namespace OrderService.Api.Features.Orders.Commands.CreateOrder;

public sealed record CreateOrderCommand(string CustomerName, decimal TotalAmount) : IRequest<Result<int>>;
