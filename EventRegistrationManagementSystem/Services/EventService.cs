using EventRegistrationManagementSystem.Models;

namespace EventRegistrationManagementSystem.Services
{
    public class EventService
    {
        private readonly AppData _data;
        private readonly IDataStore _dataStore;

        public EventService(AppData data, IDataStore dataStore)
        {
            _data = data;
            _dataStore = dataStore;
        }

        private void SaveChanges() => _dataStore.Save(_data);

        private int GetActiveRegistrationCount(int eventId)
        {
            return _data.Registrations.Count(r =>
                r.EventId == eventId &&
                r.Status != RegistrationStatus.Cancelled);
        }

        public bool CreateEvent(Event newEvent, out string message)
        {
            if (newEvent.EventId <= 0)
            {
                message = "Event ID must be positive.";
                return false;
            }

            if (_data.Events.Any(e => e.EventId == newEvent.EventId))
            {
                message = "Event ID already exists.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(newEvent.EventName) ||
                string.IsNullOrWhiteSpace(newEvent.Venue))
            {
                message = "Event name and venue are required.";
                return false;
            }

            if (newEvent.EventDate <= DateTime.Now)
            {
                message = "Event date must be in the future.";
                return false;
            }

            if (newEvent.Capacity <= 0)
            {
                message = "Capacity must be greater than zero.";
                return false;
            }

            if (newEvent.RegistrationFee < 0)
            {
                message = "Registration fee cannot be negative.";
                return false;
            }

            newEvent.EventName = newEvent.EventName.Trim();
            newEvent.Venue = newEvent.Venue.Trim();
            newEvent.IsArchived = false;

            _data.Events.Add(newEvent);
            SaveChanges();

            message = "Event created successfully.";
            return true;
        }

        public List<Event> GetAllEvents()
        {
            return _data.Events
                .Where(e => !e.IsArchived)
                .OrderBy(e => e.EventDate)
                .ToList();
        }

        public Event? GetEventById(int eventId)
        {
            return _data.Events.FirstOrDefault(e =>
                e.EventId == eventId && !e.IsArchived);
        }

        public List<Event> SearchEvents(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return new List<Event>();

            searchText = searchText.Trim();

            return _data.Events
                .Where(e => !e.IsArchived &&
                    (e.EventName.Contains(
                         searchText, StringComparison.OrdinalIgnoreCase) ||
                     e.Venue.Contains(
                         searchText, StringComparison.OrdinalIgnoreCase)))
                .OrderBy(e => e.EventName)
                .ToList();
        }

        public bool UpdateEvent(
            int eventId,
            string venue,
            DateTime eventDate,
            int capacity,
            decimal fee,
            out string message)
        {
            Event? existingEvent = GetEventById(eventId);

            if (existingEvent == null)
            {
                message = "Event not found.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(venue))
            {
                message = "Venue cannot be empty.";
                return false;
            }

            if (eventDate <= DateTime.Now)
            {
                message = "Event date must be in the future.";
                return false;
            }

            if (capacity <= 0)
            {
                message = "Capacity must be greater than zero.";
                return false;
            }

            if (fee < 0)
            {
                message = "Fee cannot be negative.";
                return false;
            }

            int activeCount = GetActiveRegistrationCount(eventId);

            if (capacity < activeCount)
            {
                message = $"Capacity cannot be less than {activeCount} active registrations.";
                return false;
            }

            existingEvent.Venue = venue.Trim();
            existingEvent.EventDate = eventDate;
            existingEvent.Capacity = capacity;
            existingEvent.RegistrationFee = fee;

            SaveChanges();

            message = "Event updated successfully.";
            return true;
        }

        public int GetRemainingCapacity(int eventId)
        {
            Event? selectedEvent = GetEventById(eventId);

            if (selectedEvent == null)
                return 0;

            return Math.Max(
                0,
                selectedEvent.Capacity -
                GetActiveRegistrationCount(eventId));
        }

        public bool ArchiveEvent(int eventId, out string message)
        {
            Event? selectedEvent = GetEventById(eventId);

            if (selectedEvent == null)
            {
                message = "Event not found.";
                return false;
            }

            if (GetActiveRegistrationCount(eventId) > 0)
            {
                message = "Cannot archive an event with active registrations.";
                return false;
            }

            selectedEvent.IsArchived = true;
            SaveChanges();

            message = "Event archived successfully.";
            return true;
        }
    }
}