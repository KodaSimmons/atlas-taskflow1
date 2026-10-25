using AtlasTaskFlow.Services;
using Microsoft.AspNetCore.Mvc;

namespace AtlasTaskFlow.Controllers;

/// <summary>
/// The "Viewing as" and team switchers. Until real sign-in arrives in Sprint 6, this lets a tester act as
/// the team leader or a team member to try both sides of the workflow.
/// </summary>
public class DemoController : AtlasController
{
    public DemoController(AtlasStore store, CurrentUserContext current) : base(store, current) { }

    [HttpPost]
    public IActionResult SwitchUser(int userId)
    {
        var user = Store.GetUser(userId);
        if (user is null) return NotFound();

        Response.Cookies.Append(CurrentUserContext.UserCookie, user.UserId.ToString(), CookieOptions());
        // The new user may not belong to the current team, so let CurrentUserContext pick their first team.
        Response.Cookies.Delete(CurrentUserContext.TeamCookie);
        FlashSuccess($"You're now viewing Atlas as {user.FullName}.");
        return RedirectToAction("Index", "Home");
    }

    /// <summary>US-04: switch workspace without signing out.</summary>
    [HttpPost]
    public IActionResult SwitchTeam(int teamId)
    {
        var team = Current.Teams.FirstOrDefault(t => t.TeamId == teamId);
        if (team is null) return NotFound();

        Response.Cookies.Append(CurrentUserContext.TeamCookie, team.TeamId.ToString(), CookieOptions());
        FlashSuccess($"Switched to {team.TeamName.TrimEnd('.')}.");
        return RedirectToAction("Index", "Home");
    }

    private static CookieOptions CookieOptions() => new()
    {
        HttpOnly = true,
        SameSite = SameSiteMode.Lax,
        Secure = true,
        IsEssential = true,
        Expires = DateTimeOffset.UtcNow.AddDays(30)
    };
}
