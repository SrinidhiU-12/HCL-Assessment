using EventRegistrationManagementSystem.Models;
using EventRegistrationManagementSystem.Utilities;

namespace EventRegistrationManagementSystem.Services
{
    public class ReportService
    {
        private readonly AppData _data;

        public ReportService(AppData data)
        {
            _data = data;
        }

        private List<Event> GetActiveEvents()
        {
            return GenericHelper.Filter(
                _data.Events,
                e => !e.IsArchived);
        }

        public void ShowRegistrationsPerEvent()
        {
            Console.WriteLine("\n===== REGISTRATIONS AND PARTICIPANTS PER EVENT =====");

            foreach (var item in GetActiveEvents()
                .GroupJoin(
                    _data.Registrations,
                    e => e.EventId,
                    r => r.EventId,
                    (e, registrations) => new
                    {
                        Event = e,
                        Registrations = registrations
                            .Where(r => r.Status != RegistrationStatus.Cancelled)
                            .ToList()
                    })
                .OrderBy(x => x.Event.EventName))
            {
                Console.WriteLine(
                    $"\nEvent: {item.Event.EventName} (ID: {item.Event.EventId})");

                Console.WriteLine(
                    $"Active registrations: {item.Registrations.Count}");

                foreach (var registration in item.Registrations)
                {
                    Participant? participant = _data.Participants
                        .FirstOrDefault(p =>
                            p.ParticipantId == registration.ParticipantId);

                    Console.WriteLine(
                        $"Registration ID: {registration.RegistrationId} | " +
                        $"Participant: {participant?.FullName ?? "Unknown"} | " +
                        $"Status: {registration.Status} | " +
                        $"Paid: {registration.AmountPaid:C}");
                }

                if (item.Registrations.Count == 0)
                    Console.WriteLine("No active registrations.");
            }
        }

        public void ShowCapacityReport()
        {
            Console.WriteLine("\n===== EVENT CAPACITY REPORT =====");

            var report = GetActiveEvents()
                .Select(e =>
                {
                    int count = _data.Registrations.Count(r =>
                        r.EventId == e.EventId &&
                        r.Status != RegistrationStatus.Cancelled);

                    return new
                    {
                        e.EventId,
                        e.EventName,
                        e.Capacity,
                        Registered = count,
                        Remaining = Math.Max(0, e.Capacity - count),
                        IsFull = count >= e.Capacity
                    };
                })
                .OrderBy(e => e.EventName)
                .ToList();

            foreach (var item in report)
            {
                Console.WriteLine(
                    $"{item.EventName} | " +
                    $"Registered: {item.Registered}/{item.Capacity} | " +
                    $"Remaining: {item.Remaining}");
            }

            Console.WriteLine("\n===== FULL EVENTS =====");

            var fullEvents = report.Where(e => e.IsFull).ToList();

            if (fullEvents.Count == 0)
            {
                Console.WriteLine("There are no full events.");
                return;
            }

            foreach (var item in fullEvents)
            {
                Console.WriteLine(
                    $"{item.EventName} ({item.Registered}/{item.Capacity})");
            }
        }

        public void ShowPaymentReport()
        {
            Console.WriteLine("\n===== PAYMENT REPORT =====");

            var report = GetActiveEvents()
                .GroupJoin(
                    _data.Registrations,
                    e => e.EventId,
                    r => r.EventId,
                    (e, registrations) => new
                    {
                        e.EventId,
                        e.EventName,
                        TotalPayments = registrations.Sum(r => r.AmountPaid)
                    })
                .OrderBy(e => e.EventName)
                .ToList();

            foreach (var item in report)
            {
                Console.WriteLine(
                    $"{item.EventName}: {item.TotalPayments:C}");
            }

            decimal overall = _data.Registrations
                .Sum(r => r.AmountPaid);

            Console.WriteLine($"Overall payments recorded: {overall:C}");
            Console.WriteLine(
                "Note: Totals include cancelled registrations because refunds are not tracked.");
        }

        public void ShowAttendanceReport()
        {
            Console.WriteLine("\n===== ATTENDANCE REPORT =====");

            foreach (var e in GetActiveEvents().OrderBy(e => e.EventName))
            {
                var registrations = _data.Registrations
                    .Where(r => r.EventId == e.EventId &&
                        r.Status != RegistrationStatus.Cancelled)
                    .ToList();

                int total = registrations.Count;
                int attended = registrations.Count(r =>
                    r.Status == RegistrationStatus.Attended);

                double percentage = total == 0
                    ? 0
                    : attended * 100.0 / total;

                Console.WriteLine(
                    $"{e.EventName} | Registered: {total} | " +
                    $"Attended: {attended} | " +
                    $"Attendance: {percentage:F2}%");
            }
        }

        public void ShowRegistrationSummary()
        {
            Console.WriteLine("\n===== REGISTRATION SUMMARY BY EVENT =====");

            var grouped = _data.Registrations
                .GroupBy(r => r.EventId)
                .Select(g => new
                {
                    EventId = g.Key,
                    Total = g.Count(),
                    Active = g.Count(r =>
                        r.Status != RegistrationStatus.Cancelled),
                    Cancelled = g.Count(r =>
                        r.Status == RegistrationStatus.Cancelled),
                    AveragePayment = g.Average(r => r.AmountPaid)
                })
                .OrderBy(g => g.EventId);

            foreach (var item in grouped)
            {
                Event? e = _data.Events.FirstOrDefault(
                    x => x.EventId == item.EventId);

                Console.WriteLine(
                    $"{e?.EventName ?? "Unknown event"} | " +
                    $"Total: {item.Total} | Active: {item.Active} | " +
                    $"Cancelled: {item.Cancelled} | " +
                    $"Average payment: {item.AveragePayment:C}");
            }

            var events = GetActiveEvents();

            if (events.Count > 0)
            {
                Console.WriteLine(
                    $"Lowest event fee: {events.Min(e => e.RegistrationFee):C}");

                Console.WriteLine(
                    $"Highest event fee: {events.Max(e => e.RegistrationFee):C}");

                Console.WriteLine(
                    $"Average event fee: {events.Average(e => e.RegistrationFee):C}");
            }
        }

        public void ShowParticipantEvents(int participantId)
        {
            var events = _data.Registrations
                .Where(r => r.ParticipantId == participantId &&
                    r.Status != RegistrationStatus.Cancelled)
                .Join(
                    GetActiveEvents(),
                    r => r.EventId,
                    e => e.EventId,
                    (r, e) => new { Event = e, Registration = r })
                .OrderBy(x => x.Event.EventDate)
                .ToList();

            if (events.Count == 0)
            {
                Console.WriteLine("No active event registrations found.");
                return;
            }

            foreach (var item in events)
            {
                Console.WriteLine(
                    $"{item.Event} | Status: {item.Registration.Status}");
            }
        }

        public void ShowParticipantsForEvent(int eventId)
        {
            var registrations = _data.Registrations
                .Where(r => r.EventId == eventId &&
                    r.Status != RegistrationStatus.Cancelled)
                .Join(
                    _data.Participants,
                    r => r.ParticipantId,
                    p => p.ParticipantId,
                    (r, p) => new { Participant = p, Registration = r })
                .OrderBy(x => x.Participant.FullName)
                .ToList();

            if (registrations.Count == 0)
            {
                Console.WriteLine("No active participants found for this event.");
                return;
            }

            foreach (var item in registrations)
            {
                Console.WriteLine(
                    $"{item.Participant} | Status: {item.Registration.Status}");
            }
        }
    }
}