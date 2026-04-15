namespace CEMS.Models.ViewModels
{
    public class AttendanceViewModel
    {
        public int RegistrationId { get; set; }
        public int EventId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }
        public bool IsAttended { get; set; }
    }
}