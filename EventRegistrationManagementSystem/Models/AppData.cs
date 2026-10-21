

using System.Collections.Generic;

namespace EventRegistrationManagementSystem.Models
{
    public class AppData
    {
        public List<Event> Events { get; set; } = new();
        public List<Participant> Participants { get; set; } = new();
        public List<Registration> Registrations { get; set; } = new();
    }
}