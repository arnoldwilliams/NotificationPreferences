namespace NotificationPreferences.MauiClient;

/// <summary>
/// Base address of the NotificationPreferences API.
/// </summary>
/// <remarks>
/// The Android emulator reaches the host machine through 10.0.2.2, not localhost, and plain
/// HTTP to that address is blocked by the platform's cleartext policy. Development builds
/// therefore point at a local HTTPS API; change this to the deployed API for anything else.
/// </remarks>
public static class ApiConfiguration
{
    public const string BaseAddress = "https://10.0.2.2:5001/";
}
