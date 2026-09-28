using Microsoft.EntityFrameworkCore;
using OrderService.Api.Infrastructure.Persistence;

namespace OrderService.Tests.Features.Orders;

internal static class TestDbContextFactory
{
    public static AppDbContext Create() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
