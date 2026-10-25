namespace AtlasTaskFlow.Models;

public class Comment
{
    public int CommentId { get; set; }
    public int TaskId { get; set; }
    public int UserId { get; set; }
    public string MessageText { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
