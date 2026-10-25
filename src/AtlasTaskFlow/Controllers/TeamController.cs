using AtlasTaskFlow.Models;
using AtlasTaskFlow.Services;
using Microsoft.AspNetCore.Mvc;

namespace AtlasTaskFlow.Controllers;

public class TeamController : AtlasController
{
    public TeamController(AtlasStore store, CurrentUserContext current) : base(store, current) { }

    public IActionResult Index()
    {
        var tasks = Store.GetTasksForTeam(Current.Team.TeamId);
        var members = Store.GetTeamMembers(Current.Team.TeamId)
            .Select(m => new TeamMemberView(
                Store.GetUser(m.UserId)!,
                m,
                tasks.Count(t => t.AssigneeIds.Contains(m.UserId) && t.Status != TaskItemStatus.Completed)))
            .OrderBy(v => v.Membership.Role).ThenBy(v => v.User.FullName)
            .ToList();
        return View(members);
    }

    /// <summary>FT-10: a leader grants or removes manager permissions for a member.</summary>
    [HttpPost]
    public IActionResult ToggleManager(int userId)
    {
        if (Current.Membership.Role != TeamRole.Leader)
        {
            FlashError("Only the Team Leader can change permissions.");
            return RedirectToAction(nameof(Index));
        }

        var member = Store.GetMembership(Current.Team.TeamId, userId);
        if (member is null || member.Role == TeamRole.Leader) return NotFound();

        Store.SetManagerPermissions(Current.Team.TeamId, userId, !member.HasManagerPermissions);
        var name = Store.GetUser(userId)!.FullName;
        FlashSuccess(member.HasManagerPermissions ? $"{name} can now manage and approve tasks." : $"{name} is back to standard member permissions.");
        return RedirectToAction(nameof(Index));
    }
}
