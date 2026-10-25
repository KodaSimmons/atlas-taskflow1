using AtlasTaskFlow.Models;

namespace AtlasTaskFlow.Services;

/// <summary>
/// Works out who is using the app and which team they are working in.
///
/// Sprint 4 has no login yet (authentication is Sprint 6), so the person is picked with the
/// "Viewing as" switcher and remembered in a cookie. When ASP.NET Core Identity is added, only this
/// class needs to change: it will read the signed-in user instead of the cookie.
/// </summary>
public class CurrentUserContext
{
    public const string UserCookie = "atlas_user";
    public const string TeamCookie = "atlas_team";

    public CurrentUserContext(AtlasStore store, IHttpContextAccessor accessor)
    {
        var cookies = accessor.HttpContext?.Request.Cookies;
        var users = store.GetUsers();

        User = (int.TryParse(cookies?[UserCookie], out var userId) ? store.GetUser(userId) : null) ?? users.First();
        Teams = store.GetTeamsForUser(User.UserId);

        // Only accept a team the user actually belongs to, so a stale or edited cookie can't expose another team.
        Team = (int.TryParse(cookies?[TeamCookie], out var teamId) ? Teams.FirstOrDefault(t => t.TeamId == teamId) : null)
               ?? Teams.First();
        Membership = store.GetMembership(Team.TeamId, User.UserId)!;
        AllUsers = users;
    }

    public User User { get; }
    public Team Team { get; }
    public TeamMember Membership { get; }
    public IReadOnlyList<Team> Teams { get; }
    public IReadOnlyList<User> AllUsers { get; }

    public bool CanManage => Membership.CanManage;
}
