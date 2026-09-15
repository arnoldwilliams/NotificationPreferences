# Push Notification Preference Screen — Implementation Notes

Status of the work to display notification categories and topics on a .NET MAUI push
notification option screen.

- Repository: `arnoldwilliams/NotificationPreferences`
- Branch: `feature/notification-categories-topics`
- Framework: .NET 10 (solution, API, and MAUI client)
- Latest commit: `663099d` — *Add MAUI push notification preference screen*

## 1. The API (pre-existing, re-verified)

The repository, service, and controller that return the combined payload were already in place
before this work. They were re-verified rather than rewritten.

| Layer | File | Responsibility |
| --- | --- | --- |
| Controller | `src/NotificationPreferences.Api/Controllers/NotificationPreferencesController.cs` | `GET /api/notification-preferences/categories-with-topics` |
| Service | `src/NotificationPreferences.Api/Services/NotificationPreferencesService.cs` | Entity → DTO mapping |
| Repository | `src/NotificationPreferences.Api/Repositories/NotificationPreferencesRepository.cs` | EF Core query joining both tables |
| Data | `src/NotificationPreferences.Api/Data/` | `DbContext`, entities, mappings |

### Endpoint

```
GET /api/notification-preferences/categories-with-topics
```

Returns one grouped structure: each category carries its own active topics. Both
`NotificationCategories.IsActive` and `NotificationTopics.IsActive` are filtered, so disabled
rows never reach the client.

```json
[
  {
    "id": 1,
    "code": "ACCOUNT",
    "displayName": "Account & Security",
    "topics": [
      {
        "id": 1,
        "code": "SECURITY_ALERTS",
        "displayName": "Sign-in alerts",
        "description": "Get notified about new sign-ins to your account.",
        "categoryId": 1
      }
    ]
  }
]
```

## 2. The MAUI client (added)

The client is split in two so that everything except the XAML view and platform storage is
testable without a device or emulator.

### `src/NotificationPreferences.Client` — plain `net10.0`

No MAUI dependency, so it can be unit tested as an ordinary library.

| File | Responsibility |
| --- | --- |
| `Services/NotificationPreferencesApiClient.cs` | HTTP call to the endpoint |
| `Services/INotificationSubscriptionStore.cs` | Per-topic opt-in state, keyed by topic code |
| `ViewModels/NotificationPreferenceViewModel.cs` | Load/refresh, grouping, save summary |
| `ViewModels/TopicPreferenceViewModel.cs` | One topic plus its toggle |
| `Models/NotificationCategoryGroup.cs` | `CollectionView` group shape |

### `src/NotificationPreferences.MauiClient` — the screen

| File | Responsibility |
| --- | --- |
| `Views/NotificationPreferencePage.xaml` | Grouped `CollectionView`, one `Switch` per topic |
| `Views/NotificationPreferencePage.xaml.cs` | Code-behind |
| `Services/NotificationSubscriptionStore.cs` | MAUI `Preferences`-backed store implementation |
| `ApiConfiguration.cs` | API base address |
| `MauiProgram.cs` | DI registration |

### Screen behaviour

- Grouped `CollectionView`: category name as the group header, one row per topic.
- Each row shows the display name, an optional description, and a two-way bound `Switch`.
- `IsSubscribed` is deliberately **not** part of the DTO; `TopicPreferenceViewModel` adds it so
  the API payload stays free of per-user state.
- Toggle state is keyed by `NotificationTopicDto.Code`, which is stable and unique
  (within and in practice across categories) — not by `DisplayName`.
- Loading, error, and empty states are exposed as bindable properties, including a retry prompt
  on failure.

### Dependency injection

```csharp
builder.Services.AddSingleton<INotificationSubscriptionStore, NotificationSubscriptionStore>();

builder.Services.AddHttpClient<INotificationPreferencesApiClient, NotificationPreferencesApiClient>(
    client => client.BaseAddress = new Uri(ApiConfiguration.BaseAddress));

builder.Services.AddTransient<NotificationPreferenceViewModel>();
builder.Services.AddTransient<NotificationPreferencePage>();
builder.Services.AddSingleton<AppShell>();
```

## 3. Schema

The client consumes the payload produced from the existing schema; no schema changes were made.
`tests/sql/schema.sql` holds the DDL plus seed data matching the real tables.

- `dbo.NotificationCategories` — `Id`, `Code` (unique), `DisplayName`, `IsActive`, `CreatedUtc`
- `dbo.NotificationTopics` — `Id`, `CategoryId` (FK), `Code`, `DisplayName`, `Description`
  (nullable), `IsActive`, `CreatedUtc`; unique on `(CategoryId, Code)`

## 4. Verification

### Build

Full solution builds with **0 errors**.

```
dotnet build NotificationPreferences.sln
```

### Tests

**38 tests pass**, 0 failures, across two suites. Tests exercise real code paths — no mocking
libraries are used. The API client is tested against a stub `HttpMessageHandler`, and the view
models against an in-memory store.

| Suite | Tests | Covers |
| --- | --- | --- |
| `tests/NotificationPreferences.Tests` | 16 | API repository, service, controller |
| `tests/NotificationPreferences.MauiClient.Tests` | 22 | Client, API client, view models |

Client-side cases:

*API client*
- Deserialises categories and topics
- Requests the documented endpoint
- Empty JSON array yields no categories
- Non-success status throws
- Malformed payload throws instead of returning nulls

*Topic toggle*
- Reflects stored subscription state on construction
- Only preloads the topic that was previously subscribed
- Persists toggle on under the topic code
- Persists toggle off for a topic that was previously subscribed
- Does not write to storage when the value is unchanged
- Carries code and display fields from the DTO
- Topic without a description hides the description row

*Screen view model*
- Groups topics under their category in API order
- Wraps each topic in its own toggle
- Clears busy and error state on success
- Surfaces a retryable error without throwing
- Successful reload replaces previous groups instead of appending
- Empty response reports no options and disables save
- Group reports its topic count singular and plural
- Save summarises the enabled topic count
- Save reports when everything is turned off
- Repeated load is ignored while one is in flight

### End-to-end check

The API was run against a live SQL Server 2022 instance seeded from `tests/sql/schema.sql`
(4 categories, 7 topics), and the **actual client library** was driven against that live
response rather than an assumed shape. Confirmed:

- Inactive categories and topics are excluded from the payload.
- The client deserialises the real response with no errors.
- Toggling two topics on, then constructing a fresh view model over the same store, reads the
  saved state back correctly.
- The save summary reflects the enabled topic count accurately.

Run the suites with:

```
dotnet test tests/NotificationPreferences.Tests
dotnet test tests/NotificationPreferences.MauiClient.Tests
```

Building the Android target additionally needs the `maui-android` workload, a JDK, and an
Android SDK (`platforms;android-36`, `build-tools;36.0.0`), with `ANDROID_HOME` and `JAVA_HOME`
set:

```
dotnet build src/NotificationPreferences.MauiClient/NotificationPreferences.MauiClient.csproj -f net10.0-android
```

## 5. Known limitations and next steps

### Subscription state is device-local only

Selections persist to MAUI `Preferences` and survive app restarts, but they are **not** sent to
the backend or to the push provider's topic registration. A user's choices therefore do not
follow them across devices or reinstalls.

`NotificationPreferenceViewModel.SaveCommand` is wired up as the hook for that call. Today it
only summarises the local selection. Closing this gap needs a subscription endpoint on the API
side plus a push-provider topic registration call on the client.

### Base address is development-only

`ApiConfiguration.BaseAddress` is `https://10.0.2.2:5001/`, which is how the Android emulator
reaches the host machine. It must be changed for a physical device or a deployed API.

### Host environment caveat

This development machine sets `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1`. Under it, the SQL
Server client throws `NotSupportedException: Globalization Invariant Mode is not supported` when
opening a connection. This is an environment quirk, not a code defect — the API connects
normally once the variable is unset. It is worth checking if the API ever fails to reach the
database in a container that inherits this setting.

## 6. Unrelated observation

The repository contains a tracked file named `Readme` (no extension) at the root, containing the
text `Test file`. It dates from the initial `main` commit and is unrelated to this work. It was
left untouched, but may be worth removing.
