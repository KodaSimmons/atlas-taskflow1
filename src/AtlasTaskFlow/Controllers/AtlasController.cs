using AtlasTaskFlow.Models;
using AtlasTaskFlow.Services;
using Microsoft.AspNetCore.Mvc;

namespace AtlasTaskFlow.Controllers;

/// <summary>Shared plumbing for the Atlas controllers: the current user/team and flash messages.</summary>
public abstract class AtlasController : Controller
{
    protected AtlasController(AtlasStore store, CurrentUserContext current)
    {
        Store = store;
        Current = current;
    }

    protected AtlasStore Store { get; }
    protected CurrentUserContext Current { get; }

    protected void FlashSuccess(string message) => TempData["Success"] = message;
    protected void FlashError(string message) => TempData["Error"] = message;

    protected IReadOnlyList<TaskCard> ToCards(IEnumerable<TaskItem> tasks)
    {
        var users = Store.GetUsers().ToDictionary(u => u.UserId);
        var projects = Store.GetProjects(Current.Team.TeamId).ToDictionary(p => p.ProjectId);
        return tasks.Select(t => new TaskCard(
                t,
                projects.TryGetValue(t.ProjectId, out var p) ? p.ProjectName : "",
                t.AssigneeIds.Where(users.ContainsKey).Select(id => users[id]).ToList()))
            .ToList();
    }
}
