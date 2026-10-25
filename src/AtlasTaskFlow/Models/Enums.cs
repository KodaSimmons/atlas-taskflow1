using System.ComponentModel.DataAnnotations;

namespace AtlasTaskFlow.Models;

/// <summary>The five task statuses from the Sprint 3 design (FR-2.2).</summary>
public enum TaskItemStatus
{
    [Display(Name = "Not Assigned")] NotAssigned,
    [Display(Name = "Assigned")] Assigned,
    [Display(Name = "In Progress")] InProgress,
    [Display(Name = "Needs Review")] NeedsReview,
    [Display(Name = "Completed")] Completed
}

public enum TeamRole
{
    Leader,
    Member
}

public enum AttachmentType
{
    Reference,
    FinalDeliverable
}
