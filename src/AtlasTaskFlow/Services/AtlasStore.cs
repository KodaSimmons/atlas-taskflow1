using AtlasTaskFlow.Models;

namespace AtlasTaskFlow.Services;

/// <summary>
/// In-memory data store for Sprint 4. It keeps the same shape as the planned Azure SQL schema so it can be
/// swapped for Entity Framework Core when persistence arrives. Data resets when the app restarts.
/// </summary>
public class AtlasStore
{
    private readonly object _gate = new();
    private readonly List<User> _users = new();
    private readonly List<Team> _teams = new();
    private readonly List<TeamMember> _members = new();
    private readonly List<Project> _projects = new();
    private readonly List<TaskItem> _tasks = new();
    private readonly List<Comment> _comments = new();
    private readonly List<Attachment> _attachments = new();
    private readonly List<Notification> _notifications = new();
    private readonly List<ActivityEntry> _activity = new();
    private int _nextId = 1;

    private int NextId() => _nextId++;

    // ---------- Reads ----------

    public IReadOnlyList<User> GetUsers() { lock (_gate) return _users.ToList(); }

    public User? GetUser(int userId) { lock (_gate) return _users.FirstOrDefault(u => u.UserId == userId); }

    public Team? GetTeam(int teamId) { lock (_gate) return _teams.FirstOrDefault(t => t.TeamId == teamId); }

    public IReadOnlyList<Team> GetTeamsForUser(int userId)
    {
        lock (_gate)
        {
            var teamIds = _members.Where(m => m.UserId == userId).Select(m => m.TeamId).ToHashSet();
            return _teams.Where(t => teamIds.Contains(t.TeamId)).OrderBy(t => t.TeamName).ToList();
        }
    }

    public TeamMember? GetMembership(int teamId, int userId)
    {
        lock (_gate) return _members.FirstOrDefault(m => m.TeamId == teamId && m.UserId == userId);
    }

    public IReadOnlyList<TeamMember> GetTeamMembers(int teamId)
    {
        lock (_gate) return _members.Where(m => m.TeamId == teamId).ToList();
    }

    public IReadOnlyList<Project> GetProjects(int teamId)
    {
        lock (_gate)
        {
            return _projects.Where(p => p.TeamId == teamId)
                .OrderByDescending(p => p.IsDefaultTasksProject).ThenBy(p => p.ProjectName).ToList();
        }
    }

    public Project? GetProject(int projectId) { lock (_gate) return _projects.FirstOrDefault(p => p.ProjectId == projectId); }

    public IReadOnlyList<TaskItem> GetTasksForTeam(int teamId)
    {
        lock (_gate)
        {
            var projectIds = _projects.Where(p => p.TeamId == teamId).Select(p => p.ProjectId).ToHashSet();
            return _tasks.Where(t => projectIds.Contains(t.ProjectId)).OrderBy(t => t.DueDate).ToList();
        }
    }

    public TaskItem? GetTask(int taskId) { lock (_gate) return _tasks.FirstOrDefault(t => t.TaskId == taskId); }

    /// <summary>Returns the team that owns a task, so callers can enforce team isolation.</summary>
    public int? GetTeamIdForTask(int taskId)
    {
        lock (_gate)
        {
            var task = _tasks.FirstOrDefault(t => t.TaskId == taskId);
            return task is null ? null : _projects.FirstOrDefault(p => p.ProjectId == task.ProjectId)?.TeamId;
        }
    }

    public IReadOnlyList<Comment> GetComments(int taskId)
    {
        lock (_gate) return _comments.Where(c => c.TaskId == taskId).OrderBy(c => c.CreatedAt).ToList();
    }

    public IReadOnlyList<Attachment> GetAttachments(int taskId)
    {
        lock (_gate) return _attachments.Where(a => a.TaskId == taskId).OrderBy(a => a.UploadedAt).ToList();
    }

    public Attachment? GetAttachment(int attachmentId)
    {
        lock (_gate) return _attachments.FirstOrDefault(a => a.AttachmentId == attachmentId);
    }

    public IReadOnlyList<Notification> GetNotifications(int userId, int teamId)
    {
        lock (_gate)
        {
            return _notifications.Where(n => n.UserId == userId && n.TeamId == teamId)
                .OrderByDescending(n => n.CreatedAt).ToList();
        }
    }

    public int GetUnreadCount(int userId, int teamId)
    {
        lock (_gate) return _notifications.Count(n => n.UserId == userId && n.TeamId == teamId && !n.IsRead);
    }

    public IReadOnlyList<ActivityEntry> GetRecentActivity(int teamId, int count)
    {
        lock (_gate) return _activity.Where(a => a.TeamId == teamId).OrderByDescending(a => a.CreatedAt).Take(count).ToList();
    }

    // ---------- Writes ----------

    public User AddUser(string fullName, string email)
    {
        lock (_gate)
        {
            var user = new User { UserId = NextId(), FullName = fullName, Email = email };
            _users.Add(user);
            return user;
        }
    }

    /// <summary>Creates a team, makes the creator its leader, and adds the default "Tasks" project (FR-2.1).</summary>
    public Team CreateTeam(string teamName, int createdByUserId)
    {
        lock (_gate)
        {
            var team = new Team { TeamId = NextId(), TeamName = teamName, CreatedByUserId = createdByUserId };
            _teams.Add(team);
            _members.Add(new TeamMember { TeamMemberId = NextId(), TeamId = team.TeamId, UserId = createdByUserId, Role = TeamRole.Leader });
            _projects.Add(new Project
            {
                ProjectId = NextId(),
                TeamId = team.TeamId,
                ProjectName = "Tasks",
                Description = "General tasks that don't belong to a specific project.",
                IsDefaultTasksProject = true
            });
            return team;
        }
    }

    public TeamMember AddMember(int teamId, int userId, TeamRole role)
    {
        lock (_gate)
        {
            var member = new TeamMember { TeamMemberId = NextId(), TeamId = teamId, UserId = userId, Role = role };
            _members.Add(member);
            return member;
        }
    }

    public void SetManagerPermissions(int teamId, int userId, bool value)
    {
        lock (_gate)
        {
            var member = _members.FirstOrDefault(m => m.TeamId == teamId && m.UserId == userId);
            if (member is not null) member.HasManagerPermissions = value;
        }
    }

    public Project AddProject(int teamId, string name, string? description)
    {
        lock (_gate)
        {
            var project = new Project { ProjectId = NextId(), TeamId = teamId, ProjectName = name, Description = description };
            _projects.Add(project);
            return project;
        }
    }

    public TaskItem AddTask(TaskItem task)
    {
        lock (_gate)
        {
            task.TaskId = NextId();
            _tasks.Add(task);
            return task;
        }
    }

    /// <summary>Runs a change against a task while holding the store lock.</summary>
    public void UpdateTask(int taskId, Action<TaskItem> change)
    {
        lock (_gate)
        {
            var task = _tasks.First(t => t.TaskId == taskId);
            change(task);
        }
    }

    public void DeleteTask(int taskId)
    {
        lock (_gate)
        {
            _tasks.RemoveAll(t => t.TaskId == taskId);
            _comments.RemoveAll(c => c.TaskId == taskId);
            _attachments.RemoveAll(a => a.TaskId == taskId);
            foreach (var n in _notifications.Where(n => n.TaskId == taskId)) n.TaskId = null;
        }
    }

    public Comment AddComment(int taskId, int userId, string text)
    {
        lock (_gate)
        {
            var comment = new Comment { CommentId = NextId(), TaskId = taskId, UserId = userId, MessageText = text };
            _comments.Add(comment);
            return comment;
        }
    }

    public Attachment AddAttachment(Attachment attachment)
    {
        lock (_gate)
        {
            attachment.AttachmentId = NextId();
            _attachments.Add(attachment);
            return attachment;
        }
    }

    public void AddNotification(int userId, int teamId, int? taskId, string message)
    {
        lock (_gate)
        {
            _notifications.Add(new Notification { NotificationId = NextId(), UserId = userId, TeamId = teamId, TaskId = taskId, Message = message });
        }
    }

    public void MarkNotificationRead(int notificationId, int userId)
    {
        lock (_gate)
        {
            var n = _notifications.FirstOrDefault(x => x.NotificationId == notificationId && x.UserId == userId);
            if (n is not null) n.IsRead = true;
        }
    }

    public void MarkAllNotificationsRead(int userId, int teamId)
    {
        lock (_gate)
        {
            foreach (var n in _notifications.Where(n => n.UserId == userId && n.TeamId == teamId)) n.IsRead = true;
        }
    }

    public void AddActivity(int teamId, int? taskId, string message)
    {
        lock (_gate) _activity.Add(new ActivityEntry { TeamId = teamId, TaskId = taskId, Message = message });
    }
}
