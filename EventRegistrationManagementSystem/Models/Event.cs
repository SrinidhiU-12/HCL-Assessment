namespace EventRegistrationManagementSystem.Models
{
    public class Event
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string Venue { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public int Capacity { get; set; }
        public decimal RegistrationFee { get; set; }
        public bool IsArchived { get; set; } = false;

        public Event() { }

        public Event(
            int eventId,
            string eventName,
            string venue,
            DateTime eventDate,
            int capacity,
            decimal registrationFee)
        {
            EventId = eventId;
            EventName = eventName;
            Venue = venue;
            EventDate = eventDate;
            Capacity = capacity;
            RegistrationFee = registrationFee;
        }

        public override string ToString()
        {
            return $"ID: {EventId} | Name: {EventName} | " +
                   $"Venue: {Venue} | Date: {EventDate:dd-MM-yyyy HH:mm} | " +
                   $"Capacity: {Capacity} | Fee: {RegistrationFee:C}";
        }
    }
}