namespace CEMS.Models.ViewModels
{
    public class MyRegistrationViewModel
    {
        public int RegistrationId { get; set; }
        public int EventId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }
    }
}