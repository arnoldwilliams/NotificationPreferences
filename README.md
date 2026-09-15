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
src/NotificationPreferences.Contracts                  DTOs (shared with the MAUI client)
src/NotificationPreferences.Client                     API client + view models (net10.0, testable)
src/NotificationPreferences.MauiClient                 MAUI push notification options screen
tests/NotificationPreferences.Tests                     xUnit tests (API)
tests/NotificationPreferences.MauiClient.Tests          xUnit tests (client + view models)
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
## MAUI client

`src/NotificationPreferences.Client` holds the pieces that do not depend on MAUI — the HTTP
client, the view models, and the grouped-collection model — so they target `net10.0` and are
covered by ordinary unit tests. `src/NotificationPreferences.MauiClient` adds the XAML view and
the platform bits (preferences-backed storage, DI wiring).

The screen lives at `Views/NotificationPreferencePage.xaml`: a grouped `CollectionView` where
each category is a header and each topic is a row with a `Switch` bound two-way to
`IsSubscribed`.

```
src/NotificationPreferences.Client
  Services/NotificationPreferencesApiClient.cs   GET categories-with-topics
  Services/INotificationSubscriptionStore.cs     per-topic opt-in state
  ViewModels/NotificationPreferenceViewModel.cs  load/refresh, grouping, save summary
  ViewModels/TopicPreferenceViewModel.cs         one topic + its toggle
  Models/NotificationCategoryGroup.cs            CollectionView group shape
src/NotificationPreferences.MauiClient
  Views/NotificationPreferencePage.xaml          the screen
  Services/NotificationSubscriptionStore.cs      MAUI Preferences-backed store
  ApiConfiguration.cs                            API base address
  MauiProgram.cs                                 DI registration
```

`NotificationCategoryDto` is the group and `NotificationTopicDto` is the child row:

```xml
<CollectionView ItemsSource="{Binding Groups}" IsGrouped="True" SelectionMode="None">
    <CollectionView.GroupHeaderTemplate>
        <DataTemplate x:DataType="models:NotificationCategoryGroup">
            <Label Text="{Binding DisplayName}" FontAttributes="Bold" />
        </DataTemplate>
    </CollectionView.GroupHeaderTemplate>
    <CollectionView.ItemTemplate>
        <DataTemplate x:DataType="viewModels:TopicPreferenceViewModel">
            <Grid ColumnDefinitions="*,Auto" Padding="20,4">
                <Label Text="{Binding DisplayName}" />
                <Switch Grid.Column="1" IsToggled="{Binding IsSubscribed, Mode=TwoWay}" />
            </Grid>
        </DataTemplate>
    </CollectionView.ItemTemplate>
</CollectionView>
```

`NotificationTopicDto.Code` is the value to persist and register with the push provider; it is
stable and unique within (and in practice across) categories, unlike `DisplayName`.
`IsSubscribed` is not part of the DTO, so `TopicPreferenceViewModel` adds it and writes each
change to `INotificationSubscriptionStore` keyed by `Code`.

The store is device-local. A production build should also sync the selected codes to the backend
and to the push provider's topic registration, so a choice follows the user across devices and
reinstallations. `NotificationPreferenceViewModel.SaveCommand` is the hook for that call; today
it only summarises the local selection.

### Running the client

`ApiConfiguration.BaseAddress` points at `https://10.0.2.2:5001/` for a local Android emulator.
Update it for a device or a deployed API. Build and test on Linux:

```
dotnet build src/NotificationPreferences.MauiClient/NotificationPreferences.MauiClient.csproj -f net10.0-android
dotnet test tests/NotificationPreferences.MauiClient.Tests
```

Building the Android target needs the `maui-android` workload, a JDK, and an Android SDK
(`platforms;android-36`, `build-tools;36.0.0`), with `ANDROID_HOME` and `JAVA_HOME` set.