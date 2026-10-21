using EventRegistrationManagementSystem.Models;

namespace EventRegistrationManagementSystem.Services
{
    public class RegistrationService
    {
        private readonly AppData _data;
        private readonly IDataStore _dataStore;

        public RegistrationService(AppData data, IDataStore dataStore)
        {
            _data = data;
            _dataStore = dataStore;
        }

        private void SaveChanges() => _dataStore.Save(_data);

        public bool RegisterParticipant(
            int registrationId,
            int eventId,
            int participantId,
            decimal amountPaid,
            out string message)
        {
            Event? selectedEvent = _data.Events.FirstOrDefault(e =>
                e.EventId == eventId && !e.IsArchived);

            if (selectedEvent == null)
            {
                message = "Event not found.";
                return false;
            }

            if (!_data.Participants.Any(p =>
                p.ParticipantId == participantId))
            {
                message = "Participant not found.";
                return false;
            }

            if (registrationId <= 0)
            {
                message = "Registration ID must be positive.";
                return false;
            }

            if (_data.Registrations.Any(r =>
                r.RegistrationId == registrationId))
            {
                message = "Registration ID already exists.";
                return false;
            }

            // Registration closes when the event starts.
            if (selectedEvent.EventDate <= DateTime.Now)
            {
                message = "Registration is closed for this event.";
                return false;
            }

            if (amountPaid < 0)
            {
                message = "Amount paid cannot be negative.";
                return false;
            }

            bool alreadyRegistered = _data.Registrations.Any(r =>
                r.EventId == eventId &&
                r.ParticipantId == participantId &&
                r.Status != RegistrationStatus.Cancelled);

            if (alreadyRegistered)
            {
                message = "Participant already has an active registration for this event.";
                return false;
            }

            int activeCount = _data.Registrations.Count(r =>
                r.EventId == eventId &&
                r.Status != RegistrationStatus.Cancelled);

            if (activeCount >= selectedEvent.Capacity)
            {
                message = "Event capacity is full.";
                return false;
            }

            Registration registration = new Registration(
                registrationId,
                eventId,
                participantId,
                DateTime.Now,
                RegistrationStatus.Registered,
                amountPaid);

            _data.Registrations.Add(registration);
            SaveChanges();

            message = "Participant registered successfully.";
            return true;
        }

        public List<Registration> GetAllRegistrations()
        {
            return _data.Registrations
                .OrderBy(r => r.RegistrationId)
                .ToList();
        }

        public Registration? GetRegistrationById(int registrationId)
        {
            return _data.Registrations.FirstOrDefault(r =>
                r.RegistrationId == registrationId);
        }

        public bool CancelRegistration(
            int registrationId,
            out string message)
        {
            Registration? registration =
                GetRegistrationById(registrationId);

            if (registration == null)
            {
                message = "Registration not found.";
                return false;
            }

            if (registration.Status == RegistrationStatus.Cancelled)
            {
                message = "Registration is already cancelled.";
                return false;
            }

            if (registration.Status == RegistrationStatus.Attended)
            {
                message = "An attended registration cannot be cancelled.";
                return false;
            }

            registration.Status = RegistrationStatus.Cancelled;
            SaveChanges();

            message = "Registration cancelled successfully. The seat is available again.";
            return true;
        }

        public bool MarkAttendance(
            int registrationId,
            out string message)
        {
            Registration? registration =
                GetRegistrationById(registrationId);

            if (registration == null)
            {
                message = "Registration not found.";
                return false;
            }

            bool eventExists = _data.Events.Any(e =>
                e.EventId == registration.EventId);

            bool participantExists = _data.Participants.Any(p =>
                p.ParticipantId == registration.ParticipantId);

            if (!eventExists || !participantExists)
            {
                message = "Registration references an invalid event or participant.";
                return false;
            }

            if (registration.Status == RegistrationStatus.Cancelled)
            {
                message = "Cannot record attendance for a cancelled registration.";
                return false;
            }

            if (registration.Status == RegistrationStatus.Attended)
            {
                message = "Attendance has already been recorded.";
                return false;
            }

            registration.Status = RegistrationStatus.Attended;
            SaveChanges();

            message = "Attendance recorded successfully.";
            return true;
        }

        public List<Participant> GetParticipantsForEvent(int eventId)
        {
            var participantIds = _data.Registrations
                .Where(r => r.EventId == eventId &&
                    r.Status != RegistrationStatus.Cancelled)
                .Select(r => r.ParticipantId)
                .ToHashSet();

            return _data.Participants
                .Where(p => participantIds.Contains(p.ParticipantId))
                .OrderBy(p => p.FullName)
                .ToList();
        }

        public List<Event> GetEventsForParticipant(int participantId)
        {
            var eventIds = _data.Registrations
                .Where(r => r.ParticipantId == participantId &&
                    r.Status != RegistrationStatus.Cancelled)
                .Select(r => r.EventId)
                .ToHashSet();

            return _data.Events
                .Where(e => eventIds.Contains(e.EventId))
                .OrderBy(e => e.EventDate)
                .ToList();
        }
    }
}