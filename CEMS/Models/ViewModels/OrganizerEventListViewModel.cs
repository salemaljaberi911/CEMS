namespace CEMS.Models.ViewModels
{
    public class OrganizerEventListViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public string Status { get; set; } = string.Empty;

        public int RegistrationCount { get; set; }
        public int RemainingSeats => Capacity - RegistrationCount;
        public bool IsUpcoming => EventDate >= DateTime.Now;

        public byte[]? ImageData { get; set; }
        public string? ImageContentType { get; set; }
    }
}