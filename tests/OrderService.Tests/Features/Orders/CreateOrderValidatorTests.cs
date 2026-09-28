using FluentValidation.TestHelper;
using OrderService.Api.Features.Orders.Commands.CreateOrder;

namespace OrderService.Tests.Features.Orders;

public sealed class CreateOrderValidatorTests
{
    private readonly CreateOrderValidator _validator = new();

    [Fact]
    public void Validate_EmptyCustomerName_ReturnsError()
    {
        var result = _validator.TestValidate(new CreateOrderCommand("", 100m));
        result.ShouldHaveValidationErrorFor(x => x.CustomerName);
    }

    [Fact]
    public void Validate_NonPositiveAmount_ReturnsError()
    {
        var result = _validator.TestValidate(new CreateOrderCommand("Ada", 0m));
        result.ShouldHaveValidationErrorFor(x => x.TotalAmount);
    }

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        var result = _validator.TestValidate(new CreateOrderCommand("Ada", 100m));
        result.ShouldNotHaveAnyValidationErrors();
    }
}
