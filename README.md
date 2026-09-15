# NotificationPreferences

Backend for the MAUI push notification option screen. Returns the notification categories and
their topics as a single grouped payload.

## Layout

```
src/NotificationPreferences.Api
  Controllers/NotificationPreferencesController.cs   HTTP endpoint
  Services/NotificationPreferencesService.cs          Entity -> DTO mapping
  Repositories/NotificationPreferencesRepository.cs   EF Core query
  Data/                                               DbContext, entities, mappings
src/NotificationPreferences.Contracts                  DTOs (safe to reference from MAUI)
tests/NotificationPreferences.Tests                     xUnit tests
tests/sql/schema.sql                                    DDL + seed data matching the real tables
```

## Endpoint

`GET /api/notification-preferences/categories-with-topics`

```json
[
  {
    "id": 1,
    "code": "ACCOUNT",
    "displayName": "Account & Security",
    "topics": [
      { "id": 2, "code": "PASSWORD_CHANGES", "displayName": "Password changes", "description": null, "categoryId": 1 },
      { "id": 1, "code": "SECURITY_ALERTS", "displayName": "Sign-in alerts", "description": "...", "categoryId": 1 }
    ]
  }
]
```

Only active categories that contain at least one active topic are returned. Categories are ordered
by display name; topics within each category are ordered by display name.

## Configuration

Set the connection string `ConnectionStrings:NotificationPreferences` (see `appsettings.json`
for the local default). Override per environment, e.g.:

```
ConnectionStrings__NotificationPreferences="Server=...;Database=NotificationPreferences;..."
```

The EF model maps to the existing `dbo.NotificationCategories` and `dbo.NotificationTopics`
tables. No migrations are required and none should be generated against this database - schema
changes are applied outside the API. `tests/NotificationPreferences.Tests` asserts the model
produces DDL identical to `tests/sql/schema.sql`, so accidental drift fails the build.

## Build and test

```
dotnet build
dotnet test
```

## Binding on the MAUI side

`NotificationCategoryDto` is the group and `NotificationTopicDto` is the child row, which maps
directly to a grouped `CollectionView`:

```csharp
public class NotificationCategoryGroup : List<NotificationTopicDto>
{
    public string DisplayName { get; }

    public NotificationCategoryGroup(string displayName, IEnumerable<NotificationTopicDto> topics)
        : base(topics)
    {
        DisplayName = displayName;
    }
}
```

```xml
<CollectionView ItemsSource="{Binding Groups}" IsGrouped="True" SelectionMode="None">
    <CollectionView.GroupHeaderTemplate>
        <DataTemplate>
            <Label Text="{Binding DisplayName}" FontAttributes="Bold" />
        </DataTemplate>
    </CollectionView.GroupHeaderTemplate>
    <CollectionView.ItemTemplate>
        <DataTemplate x:DataType="dto:NotificationTopicDto">
            <Grid ColumnDefinitions="*,Auto" Padding="16,8">
                <VerticalStackLayout>
                    <Label Text="{Binding DisplayName}" />
                    <Label Text="{Binding Description}" FontSize="12" TextColor="Gray"
                           IsVisible="{Binding Description, Converter={StaticResource IsStringNotNullOrEmpty}}" />
                </VerticalStackLayout>
                <Switch Grid.Column="1" IsToggled="{Binding IsSubscribed}" />
            </Grid>
        </DataTemplate>
    </CollectionView.ItemTemplate>
</CollectionView>
```

`NotificationTopicDto.Code` is the value to persist and register with the push provider; it is
stable and unique within (and in practice across) categories, unlike `DisplayName`.

`IsSubscribed` above is not part of the DTO. Wrap the topic in a small MAUI view model that adds
the toggle state (hydrated from local preferences or a subscription endpoint), so the API payload
stays free of per-user state.
