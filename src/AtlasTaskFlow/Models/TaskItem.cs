namespace AtlasTaskFlow.Models;

/// <summary>
/// A unit of work. Named TaskItem rather than Task so it does not clash with System.Threading.Tasks.Task.
/// </summary>
public class TaskItem
{
    public int TaskId { get; set; }
    public int ProjectId { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public TaskItemStatus Status { get; set; } = TaskItemStatus.NotAssigned;
    public DateTime DueDate { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    /// <summary>Zero, one or many assignees (FR-2.3). Stands in for the TaskAssignee junction table.</summary>
    public List<int> AssigneeIds { get; set; } = new();

    public bool IsOverdue(DateTime today) => Status != TaskItemStatus.Completed && DueDate.Date < today.Date;
}
