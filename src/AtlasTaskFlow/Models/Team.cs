namespace AtlasTaskFlow.Models;

public class Team
{
    public int TeamId { get; set; }
    public string TeamName { get; set; } = "";
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>Junction between a user and a team, carrying the user's role in that team.</summary>
public class TeamMember
{
    public int TeamMemberId { get; set; }
    public int TeamId { get; set; }
    public int UserId { get; set; }
    public TeamRole Role { get; set; }
    public bool HasManagerPermissions { get; set; }

    /// <summary>Leaders and delegated managers can create, edit, delete and approve tasks (FR-3.1, FR-3.3).</summary>
    public bool CanManage => Role == TeamRole.Leader || HasManagerPermissions;
}
