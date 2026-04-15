namespace CEMS.Models.ViewModels
{
    public class OrganizerDashboardViewModel
    {
        public int TotalEvents { get; set; }
        public int PendingEvents { get; set; }
        public int ApprovedEvents { get; set; }
        public int RejectedEvents { get; set; }

        public int UpcomingEvents { get; set; }
        public int PastEvents { get; set; }
        public int TotalRegistrations { get; set; }

        public string? MostRegisteredEventTitle { get; set; }
        public int MostRegisteredEventCount { get; set; }

        public string? NextUpcomingEventTitle { get; set; }
        public DateTime? NextUpcomingEventDate { get; set; }
    }
}