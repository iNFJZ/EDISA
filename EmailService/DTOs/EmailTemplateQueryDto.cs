namespace EmailService.DTOs
{
    public class EmailTemplateQueryDto
    {
        public string? Name { get; set; }
        public string? Language { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = "Name";
        public bool SortDescending { get; set; } = false;
    }
}
