namespace AuditService.Models;

public class AuditQueryDto
{
    public string? UserId { get; set; }
    
    public string? Action { get; set; }
    
    public string? ResourceType { get; set; }
    
    public string? ServiceName { get; set; }
    
    public DateTime? FromDate { get; set; }
    
    public DateTime? ToDate { get; set; }
    
    public int Page { get; set; } = 1;
    
    public int PageSize { get; set; } = 50;
    
    public string? SortBy { get; set; } = "CreatedAt";
    
    public string? SortOrder { get; set; } = "desc";
}
