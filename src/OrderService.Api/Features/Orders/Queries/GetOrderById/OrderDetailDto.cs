namespace OrderService.Api.Features.Orders.Queries.GetOrderById;

public sealed record OrderDetailDto(int Id, string CustomerName, decimal TotalAmount, string Status, DateTime CreatedAt);
