using Microsoft.AspNetCore.Mvc;
using NotificationPreferences.Api.Services;
using NotificationPreferences.Contracts.Dtos;

namespace NotificationPreferences.Api.Controllers;

[ApiController]
[Route("api/notification-preferences")]
[Produces("application/json")]
public class NotificationPreferencesController : ControllerBase
{
    private readonly INotificationPreferencesService _service;

    public NotificationPreferencesController(INotificationPreferencesService service)
    {
        _service = service;
    }

    /// <summary>
    /// Returns active notification categories with their active topics, ready to bind to
    /// the grouped push notification options screen.
    /// </summary>
    /// <response code="200">Category/topic groups, ordered by category display name.</response>
    [HttpGet("categories-with-topics")]
    [ProducesResponseType(typeof(IReadOnlyList<NotificationCategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<NotificationCategoryDto>>> GetCategoriesWithTopics(
        CancellationToken cancellationToken)
    {
        var result = await _service.GetCategoryTopicsAsync(cancellationToken);
        return Ok(result);
    }
}