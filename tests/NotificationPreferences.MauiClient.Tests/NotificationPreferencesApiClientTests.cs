using System.Net;
using NotificationPreferences.Client.Services;

namespace NotificationPreferences.MauiClient.Tests;

public class NotificationPreferencesApiClientTests
{
    private const string Json = """
        [
          {
            "id": 1,
            "code": "ACCOUNT",
            "displayName": "Account",
            "topics": [
              {
                "id": 10,
                "code": "account.security",
                "displayName": "Security alerts",
                "description": "Sign-ins and password changes.",
                "categoryId": 1
              }
            ]
          },
          {
            "id": 2,
            "code": "MARKETING",
            "displayName": "Marketing",
            "topics": [
              { "id": 20, "code": "marketing.promotions", "displayName": "Promotions", "description": null, "categoryId": 2 }
            ]
          }
        ]
        """;

    private static (NotificationPreferencesApiClient Client, StubHttpMessageHandler Handler) CreateClient(
        HttpStatusCode statusCode,
        string content)
    {
        var handler = new StubHttpMessageHandler(statusCode, content);
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://localhost:5001/")
        };

        return (new NotificationPreferencesApiClient(httpClient), handler);
    }

    [Fact]
    public async Task DeserialisesCategoriesAndTopics()
    {
        var (client, _) = CreateClient(HttpStatusCode.OK, Json);

        var categories = await client.GetCategoriesWithTopicsAsync();

        Assert.Equal(2, categories.Count);
        Assert.Equal("ACCOUNT", categories[0].Code);
        Assert.Equal("account.security", categories[0].Topics[0].Code);
        Assert.Equal(1, categories[0].Topics[0].CategoryId);
        Assert.Null(categories[1].Topics[0].Description);
    }

    [Fact]
    public async Task RequestsTheDocumentedEndpoint()
    {
        var (client, handler) = CreateClient(HttpStatusCode.OK, Json);

        await client.GetCategoriesWithTopicsAsync();

        Assert.Equal(
            "https://localhost:5001/api/notification-preferences/categories-with-topics",
            handler.RequestedUri?.ToString());
    }

    [Fact]
    public async Task EmptyJsonArrayYieldsNoCategories()
    {
        var (client, _) = CreateClient(HttpStatusCode.OK, "[]");

        var categories = await client.GetCategoriesWithTopicsAsync();

        Assert.Empty(categories);
    }

    [Fact]
    public async Task NonSuccessStatusThrows()
    {
        var (client, _) = CreateClient(HttpStatusCode.InternalServerError, "");

        await Assert.ThrowsAsync<HttpRequestException>(() => client.GetCategoriesWithTopicsAsync());
    }

    [Fact]
    public async Task MalformedPayloadThrowsInsteadOfReturningNulls()
    {
        var (client, _) = CreateClient(HttpStatusCode.OK, "{ not json");

        await Assert.ThrowsAsync<System.Text.Json.JsonException>(
            () => client.GetCategoriesWithTopicsAsync());
    }
}
