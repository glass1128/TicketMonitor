namespace TicketmasterMonitor.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? EventId { get; set; }
        public DateTime? EventDate { get; set; }
        public string? VenueInformation { get; set; }
        public bool? Monitoring { get; set; }
        public bool? Cancelled { get; set; }
        public bool? Postponed { get; set; }
        public string? Url { get; set; }
    }
}
