using Asp.Versioning;
using MediatR;
using OrderService.Api.Features.Orders.Commands.CancelOrder;
using OrderService.Api.Features.Orders.Commands.CompleteOrder;
using OrderService.Api.Features.Orders.Commands.CreateOrder;
using OrderService.Api.Features.Orders.Queries.GetOrderById;
using Shared.Extensions;

namespace OrderService.Api.Features.Orders;

public static class OrderEndpoints
{
    private const string GetOrderByIdRoute = "GetOrderById";

    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var versionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        var group = app.MapGroup("/api/v{version:apiVersion}/orders")
            .WithApiVersionSet(versionSet)
            .WithTags("Orders")
            .WithOpenApi();

        group.MapGet("/{id:int}", GetById)
            .WithName(GetOrderByIdRoute)
            .WithSummary("Tekil sipariş getirir")
            .Produces<OrderDetailDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .WithName("CreateOrder")
            .WithSummary("Yeni sipariş oluşturur")
            .Produces<int>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:int}/cancel", Cancel)
            .WithName("CancelOrder")
            .WithSummary("Bekleyen siparişi iptal eder")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:int}/complete", Complete)
            .WithName("CompleteOrder")
            .WithSummary("Bekleyen siparişi tamamlar")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }

    private static async Task<IResult> GetById(int id, ISender sender, CancellationToken ct)
        => (await sender.Send(new GetOrderByIdQuery(id), ct)).ToHttpResult();

    private static async Task<IResult> Create(CreateOrderCommand cmd, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(cmd, ct);
        if (result.IsFailure) return result.ToHttpResult();
        return Results.CreatedAtRoute(GetOrderByIdRoute, new { id = result.Value, version = "1" }, new { id = result.Value });
    }

    private static async Task<IResult> Cancel(int id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new CancelOrderCommand(id), ct);
        if (result.IsFailure) return result.ToHttpResult();
        return Results.NoContent();
    }

    private static async Task<IResult> Complete(int id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new CompleteOrderCommand(id), ct);
        if (result.IsFailure) return result.ToHttpResult();
        return Results.NoContent();
    }
}
