namespace OrderService.Api.Features.Orders.Queries.GetOrderList;

public sealed record OrderListDto(int Id, string CustomerName, decimal TotalAmount, string Status, DateTime CreatedAt);
