using System.Diagnostics;
using Asp.Versioning;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Features.Orders;
using OrderService.Api.Infrastructure.Persistence;
using Shared.Behaviors;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
       .UseSnakeCaseNamingConvention());

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly, includeInternalTypes: true);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

builder.Services.AddApiVersioning(opt =>
{
    opt.DefaultApiVersion = new ApiVersion(1);
    opt.AssumeDefaultVersionWhenUnspecified = true;
    opt.ReportApiVersions = true;
    opt.ApiVersionReader = new UrlSegmentApiVersionReader();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    context.Response.ContentType = "application/problem+json";
    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

    var (status, title, errors) = exception switch
    {
        ValidationException ve => (422, "Validation Error", ve.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())),
        UnauthorizedAccessException => (401, "Unauthorized", (Dictionary<string, string[]>?)null),
        _ => (500, "Internal Server Error", null)
    };

    context.Response.StatusCode = status;

    var problem = new ProblemDetails
    {
        Type = $"https://httpstatuses.io/{status}",
        Title = title,
        Status = status,
        Instance = context.Request.Path,
        Extensions = { ["traceId"] = traceId }
    };

    if (errors is not null)
        problem.Extensions["errors"] = errors;

    await context.Response.WriteAsJsonAsync(problem);
}));

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapOrderEndpoints();

app.Run();

public partial class Program;
