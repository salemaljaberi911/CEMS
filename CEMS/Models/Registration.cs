using System.ComponentModel.DataAnnotations;

namespace CEMS.Models
{
    public class Registration
    {
        public int Id { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
        
        public bool IsAttended { get; set; } = false;
    }
}