namespace AtlasTaskFlow.Models;

public class Attachment
{
    public int AttachmentId { get; set; }
    public int TaskId { get; set; }
    public int UploadedByUserId { get; set; }
    public string FileName { get; set; } = "";
    public string ContentType { get; set; } = "application/octet-stream";
    public AttachmentType FileType { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// File bytes are held in memory for this sprint. Azure Blob Storage replaces this when persistence is added.
    /// </summary>
    public byte[] Content { get; set; } = Array.Empty<byte>();
}
