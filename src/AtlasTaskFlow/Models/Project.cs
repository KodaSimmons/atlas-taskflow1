namespace AtlasTaskFlow.Models;

public class Project
{
    public int ProjectId { get; set; }
    public int TeamId { get; set; }
    public string ProjectName { get; set; } = "";
    public string? Description { get; set; }

    /// <summary>True for the "Tasks" project created automatically with each team (FR-2.1).</summary>
    public bool IsDefaultTasksProject { get; set; }
}
