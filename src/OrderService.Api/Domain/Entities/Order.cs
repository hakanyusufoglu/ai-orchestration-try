using OrderService.Api.Domain.Enums;

namespace OrderService.Api.Domain.Entities;

public sealed class Order
{
    public int Id { get; private set; }
    public string CustomerName { get; private set; } = default!;
    public decimal TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsDeleted { get; private set; }

    private Order() { }

    public static Order Create(string customerName, decimal totalAmount, DateTime createdAt) => new()
    {
        CustomerName = customerName,
        TotalAmount = totalAmount,
        Status = OrderStatus.Pending,
        CreatedAt = createdAt
    };

    public void SoftDelete() => IsDeleted = true;
}
