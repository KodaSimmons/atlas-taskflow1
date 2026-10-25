using System.ComponentModel.DataAnnotations;

namespace AtlasTaskFlow.Models;

/// <summary>A task plus the names the views need to show it.</summary>
public record TaskCard(TaskItem Task, string ProjectName, IReadOnlyList<User> Assignees);

public record ProjectSummary(Project Project, int OpenTasks, int CompletedTasks, int TotalTasks)
{
    public int PercentComplete => TotalTasks == 0 ? 0 : CompletedTasks * 100 / TotalTasks;
}

public record CalendarDay(DateTime Date, bool InMonth, bool IsToday, IReadOnlyList<TaskCard> Tasks);

public class CalendarMonth
{
    public DateTime Month { get; init; }
    public IReadOnlyList<CalendarDay> Days { get; init; } = Array.Empty<CalendarDay>();
    public string PreviousMonthKey => Month.AddMonths(-1).ToString("yyyy-MM");
    public string NextMonthKey => Month.AddMonths(1).ToString("yyyy-MM");
}

public class DashboardViewModel
{
    public IReadOnlyList<TaskCard> MyTasks { get; init; } = Array.Empty<TaskCard>();
    public IReadOnlyList<TaskCard> ReviewQueue { get; init; } = Array.Empty<TaskCard>();
    public int PoolCount { get; init; }
    public int OverdueCount { get; init; }
    public IReadOnlyList<ProjectSummary> Projects { get; init; } = Array.Empty<ProjectSummary>();
    public CalendarMonth Calendar { get; init; } = new();
    public IReadOnlyList<ActivityEntry> RecentActivity { get; init; } = Array.Empty<ActivityEntry>();
}

public class TaskListViewModel
{
    public string Filter { get; init; } = "mine";
    public IReadOnlyList<TaskCard> Tasks { get; init; } = Array.Empty<TaskCard>();
}

public class TaskFormViewModel
{
    public int? TaskId { get; set; }

    [Required(ErrorMessage = "Give the task a title.")]
    [StringLength(100, ErrorMessage = "Keep the title under 100 characters.")]
    public string Title { get; set; } = "";

    [StringLength(2000, ErrorMessage = "Keep the description under 2000 characters.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Pick a due date.")]
    [DataType(DataType.Date)]
    [Display(Name = "Due date")]
    public DateTime? DueDate { get; set; }

    [Required(ErrorMessage = "Pick a project.")]
    [Display(Name = "Project")]
    public int? ProjectId { get; set; }

    [Display(Name = "Assign to")]
    public List<int> AssigneeIds { get; set; } = new();

    // Options for the form; not posted back.
    public IReadOnlyList<Project> ProjectOptions { get; set; } = Array.Empty<Project>();
    public IReadOnlyList<User> MemberOptions { get; set; } = Array.Empty<User>();
}

public record CommentView(Comment Comment, User? Author);

public record AttachmentView(Attachment Attachment, User? Uploader);

public class TaskDetailsViewModel
{
    public required TaskItem Task { get; init; }
    public required Project Project { get; init; }
    public User? Creator { get; init; }
    public IReadOnlyList<User> Assignees { get; init; } = Array.Empty<User>();
    public IReadOnlyList<CommentView> Comments { get; init; } = Array.Empty<CommentView>();
    public IReadOnlyList<AttachmentView> References { get; init; } = Array.Empty<AttachmentView>();
    public IReadOnlyList<AttachmentView> Deliverables { get; init; } = Array.Empty<AttachmentView>();
    public bool CanManage { get; init; }
    public bool IsAssignee { get; init; }
}

public class ProjectBoardViewModel
{
    public required Project Project { get; init; }
    public IReadOnlyDictionary<TaskItemStatus, IReadOnlyList<TaskCard>> Columns { get; init; } =
        new Dictionary<TaskItemStatus, IReadOnlyList<TaskCard>>();
}

public class ProjectFormViewModel
{
    [Required(ErrorMessage = "Give the project a name.")]
    [StringLength(80, ErrorMessage = "Keep the name under 80 characters.")]
    [Display(Name = "Project name")]
    public string ProjectName { get; set; } = "";

    [StringLength(500, ErrorMessage = "Keep the description under 500 characters.")]
    public string? Description { get; set; }
}

public record TeamMemberView(User User, TeamMember Membership, int OpenTasks);
