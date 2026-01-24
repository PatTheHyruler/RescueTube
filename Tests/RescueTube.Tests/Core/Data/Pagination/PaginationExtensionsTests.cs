using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RescueTube.Core.Data;
using RescueTube.Core.Data.Pagination;
using RescueTube.Core.Utils.Pagination;
using RescueTube.Domain.Entities;
using RescueTube.Tests.TestUtils;

namespace RescueTube.Tests.Core.Data.Pagination;

public class PaginationExtensionsTests : BaseEfPostgresTest
{
    private class TestOrderByProperty(string propertyName, bool descending) : IOrderByProperty
    {
        public string PropertyName { get; } = propertyName;
        public bool Descending { get; } = descending;
    }

    [Test]
    public async Task OrderByWithConfiguration_WithValidProperties_ReturnsOrderedResults()
    {
        // Arrange
        await using var serviceProvider = BuildServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = new Domain.Entities.Identity.User();
        dbContext.Users.Add(user);

        var baseTime = TimeProvider.GetUtcNow();
        const int totalSubmissions = 25;
        for (var i = 0; i < totalSubmissions; i++)
        {
            dbContext.Submissions.Add(new Submission(user.Id, false)
            {
                Id = Guid.NewGuid(),
                Platform = Domain.Enums.EPlatform.YouTube,
                IdOnPlatform = $"{i:D11}",
                EntityType = Domain.Enums.EEntityType.Video,
                Url = $"https://youtube.com/watch?v={i:D11}",
                AddedAt = baseTime.AddMinutes(i),
                CompletedAt = baseTime.AddMinutes(i % 2 == 0 ? totalSubmissions - i : i),
            });
        }

        await dbContext.SaveChangesAsync();

        var config = new OrderingConfiguration<Submission>
        {
            DefaultOrdering = [new TestOrderByProperty("addedAt", true), new TestOrderByProperty("id", true)],
            RequiredProperty = new TestOrderByProperty("id", true),
            PropertyMap = new(StringComparer.OrdinalIgnoreCase)
            {
                ["id"] = static s => s.Id,
                ["addedAt"] = static s => s.AddedAt,
                ["completedAt"] = static s => s.CompletedAt,
            }
        };

        // Act 1
        var resultOrderedByAddedAt = await dbContext.Submissions
            .OrderByWithConfiguration([new TestOrderByProperty("addedAt", false)], config)
            .ToListAsync();

        // Assert 1
        await Assert.That(resultOrderedByAddedAt).IsNotEmpty();
        await Assert.That(resultOrderedByAddedAt.Select(static x => x.AddedAt)).IsInOrder();

        // Act 2
        var resultOrderedByCompletedAt = await dbContext.Submissions
            .OrderByWithConfiguration([new TestOrderByProperty("completedAt", false)], config)
            .ToListAsync();

        // Assert 2
        await Assert.That(resultOrderedByCompletedAt).IsNotEmpty();
        await Assert.That(resultOrderedByCompletedAt.Select(static x => x.CompletedAt)).IsInOrder();
    }
}