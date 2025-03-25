namespace LMS_API.Models
{
    public class BaseEntity
    {
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; } = null;
        public string CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
