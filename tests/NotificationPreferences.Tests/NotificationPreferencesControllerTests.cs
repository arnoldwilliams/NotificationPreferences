using Microsoft.AspNetCore.Mvc;
using NotificationPreferences.Api.Controllers;
using NotificationPreferences.Api.Repositories;
using NotificationPreferences.Api.Services;
using NotificationPreferences.Contracts.Dtos;

namespace NotificationPreferences.Tests;

public class NotificationPreferencesControllerTests : IDisposable
{
    private readonly TestDbContextFactory _factory = new();

    [Fact]
    public async Task GetCategoriesWithTopics_ReturnsOkWithGroupedDtos()
    {
        using var context = _factory.CreateSeededContext(
        [
            TestData.Category(1, "SECURITY", "Security", topics:
            [
                TestData.Topic(10, "LOGIN_ALERTS", "Login alerts", "Login activity"),
            ]),
        ]);

        var controller = new NotificationPreferencesController(
            new NotificationPreferencesService(new NotificationPreferencesRepository(context)));

        var response = await controller.GetCategoriesWithTopics(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var payload = Assert.IsAssignableFrom<IReadOnlyList<NotificationCategoryDto>>(okResult.Value);

        var category = Assert.Single(payload);
        var topic = Assert.Single(category.Topics);
        Assert.Equal("LOGIN_ALERTS", topic.Code);
    }

    [Fact]
    public void Controller_IsExposedAtExpectedRoute()
    {
        var route = typeof(NotificationPreferencesController)
            .GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.RouteAttribute), inherit: false)
            .Cast<Microsoft.AspNetCore.Mvc.RouteAttribute>()
            .Single();

        Assert.Equal("api/notification-preferences", route.Template);
    }

    public void Dispose() => _factory.Dispose();
}
