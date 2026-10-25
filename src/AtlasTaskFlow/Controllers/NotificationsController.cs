using AtlasTaskFlow.Services;
using Microsoft.AspNetCore.Mvc;

namespace AtlasTaskFlow.Controllers;

public class NotificationsController : AtlasController
{
    public NotificationsController(AtlasStore store, CurrentUserContext current) : base(store, current) { }

    public IActionResult Index() => View(Store.GetNotifications(Current.User.UserId, Current.Team.TeamId));

    /// <summary>Marks a notification read and opens the task it is about.</summary>
    public IActionResult Open(int id)
    {
        var notification = Store.GetNotifications(Current.User.UserId, Current.Team.TeamId).FirstOrDefault(n => n.NotificationId == id);
        if (notification is null) return NotFound();

        Store.MarkNotificationRead(id, Current.User.UserId);
        return notification.TaskId is { } taskId && Store.GetTask(taskId) is not null
            ? RedirectToAction("Details", "Tasks", new { id = taskId })
            : RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult MarkAllRead()
    {
        Store.MarkAllNotificationsRead(Current.User.UserId, Current.Team.TeamId);
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Unread count for the bell. The page polls this so the bell updates without a refresh.</summary>
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult Summary() => Json(new { unread = Store.GetUnreadCount(Current.User.UserId, Current.Team.TeamId) });
}
