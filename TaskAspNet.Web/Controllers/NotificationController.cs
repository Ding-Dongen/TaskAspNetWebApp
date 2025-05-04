using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskAspNet.Business.Interfaces;
using TaskAspNet.Data.Entities;

namespace TaskAspNet.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationController(INotificationService notificationService, ILogger<NotificationController> logger) : ControllerBase
{
    private readonly INotificationService _notificationService = notificationService;
    private readonly ILogger<NotificationController> _logger = logger;

    [HttpGet]
    public async Task<ActionResult<List<NotificationEntity>>> GetNotifications()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var notifications = await _notificationService
                                        .GetNotificationsForUserAsync(userId, includeRead: true);

            return Ok(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching notifications.");
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        try
        {
            await _notificationService.MarkAsReadAsync(id);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification {NotificationId} as read.", id);
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    [HttpPost("mark-all-read")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            await _notificationService.MarkAllAsReadAsync(userId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read.");
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DismissNotification(int id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            await _notificationService.DismissNotificationForUserAsync(id, userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dismissing notification {NotificationId}.", id);
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    [HttpDelete("dismiss-all")]
    public async Task<IActionResult> DismissAllNotifications()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            await _notificationService.DismissAllNotificationsForUserAsync(userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dismissing all notifications.");
            return StatusCode(500, "An unexpected error occurred.");
        }
    }
}
