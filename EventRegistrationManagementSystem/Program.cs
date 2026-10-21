using System.Globalization;
using EventRegistrationManagementSystem.Models;
using EventRegistrationManagementSystem.Services;
using EventRegistrationManagementSystem.Exceptions;

namespace EventRegistrationManagementSystem
{
    public class Program
    {
        private static AppData data = new();
        private static IDataStore dataStore = null!;
        private static EventService eventService = null!;
        private static ParticipantService participantService = null!;
        private static RegistrationService registrationService = null!;
        private static ReportService reportService = null!;

        public static void Main(string[] args)
        {
            try
            {
                string filePath = Path.Combine(
                    AppContext.BaseDirectory,
                    "event-data.json");

                dataStore = new JsonDataStore(filePath);
                data = dataStore.Load();

                eventService = new EventService(data, dataStore);
                participantService = new ParticipantService(data, dataStore);
                registrationService = new RegistrationService(data, dataStore);
                reportService = new ReportService(data);

                Console.WriteLine(
                    "\n===== EVENT REGISTRATION MANAGEMENT SYSTEM =====");

                Console.WriteLine($"Loaded events: {data.Events.Count}");
                Console.WriteLine($"Loaded participants: {data.Participants.Count}");
                Console.WriteLine($"Loaded registrations: {data.Registrations.Count}");

                MainMenu();
            }
            catch (DataPersistenceException ex)
            {
                Console.WriteLine($"Storage error: {ex.Message}");
                Console.WriteLine($"Details: {ex.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }

        private static void MainMenu()
        {
            bool running = true;

            while (running)
            {
                Console.WriteLine("\n========== MAIN MENU ==========");
                Console.WriteLine("1. Event Management");
                Console.WriteLine("2. Participant Management");
                Console.WriteLine("3. Registration Management");
                Console.WriteLine("4. Search / View Relationships");
                Console.WriteLine("5. Reports");
                Console.WriteLine("6. Save Data");
                Console.WriteLine("0. Save and Exit");
                Console.Write("Enter choice: ");

                string choice = Console.ReadLine() ?? string.Empty;

                try
                {
                    switch (choice)
                    {
                        case "1":
                            EventMenu();
                            break;

                        case "2":
                            ParticipantMenu();
                            break;

                        case "3":
                            RegistrationMenu();
                            break;

                        case "4":
                            SearchMenu();
                            break;

                        case "5":
                            ReportMenu();
                            break;

                        case "6":
                            dataStore.Save(data);
                            Console.WriteLine("Data saved successfully.");
                            break;

                        case "0":
                            dataStore.Save(data);
                            Console.WriteLine("Data saved. Goodbye!");
                            running = false;
                            break;

                        default:
                            Console.WriteLine("Invalid choice. Try again.");
                            break;
                    }
                }
                catch (DataPersistenceException ex)
                {
                    Console.WriteLine($"Storage error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Operation failed: {ex.Message}");
                }
            }
        }

        private static void EventMenu()
        {
            bool back = false;

            while (!back)
            {
                Console.WriteLine("\n===== EVENT MANAGEMENT =====");
                Console.WriteLine("1. Create Event");
                Console.WriteLine("2. View All Events");
                Console.WriteLine("3. Retrieve Event by ID");
                Console.WriteLine("4. Search Event");
                Console.WriteLine("5. Update Event");
                Console.WriteLine("6. View Remaining Capacity");
                Console.WriteLine("7. Archive Event");
                Console.WriteLine("0. Back");
                Console.Write("Choice: ");

                switch (Console.ReadLine() ?? string.Empty)
                {
                    case "1":
                        CreateEvent();
                        break;

                    case "2":
                        DisplayEvents(eventService.GetAllEvents());
                        break;

                    case "3":
                        Event? found = eventService.GetEventById(
                            ReadInt("Event ID: "));

                        Console.WriteLine(found?.ToString() ?? "Event not found.");
                        break;

                    case "4":
                        DisplayEvents(eventService.SearchEvents(
                            ReadText("Search name or venue: ")));
                        break;

                    case "5":
                        UpdateEvent();
                        break;

                    case "6":
                        {
                            int id = ReadInt("Event ID: ");
                            Event? selected = eventService.GetEventById(id);

                            if (selected == null)
                                Console.WriteLine("Event not found.");
                            else
                                Console.WriteLine(
                                    $"Remaining seats: {eventService.GetRemainingCapacity(id)}");

                            break;
                        }

                    case "7":
                        {
                            int id = ReadInt("Event ID to archive: ");
                            eventService.ArchiveEvent(id, out string message);
                            Console.WriteLine(message);
                            break;
                        }

                    case "0":
                        back = true;
                        break;

                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }
            }
        }

        private static void ParticipantMenu()
        {
            bool back = false;

            while (!back)
            {
                Console.WriteLine("\n===== PARTICIPANT MANAGEMENT =====");
                Console.WriteLine("1. Add Participant");
                Console.WriteLine("2. View All Participants");
                Console.WriteLine("3. Retrieve Participant by ID");
                Console.WriteLine("4. Search Participant");
                Console.WriteLine("5. Update Participant");
                Console.WriteLine("0. Back");
                Console.Write("Choice: ");

                switch (Console.ReadLine() ?? string.Empty)
                {
                    case "1":
                        AddParticipant();
                        break;

                    case "2":
                        DisplayParticipants(participantService.GetAllParticipants());
                        break;

                    case "3":
                        {
                            Participant? p = participantService.GetParticipantById(
                                ReadInt("Participant ID: "));

                            Console.WriteLine(p?.ToString() ?? "Participant not found.");
                            break;
                        }

                    case "4":
                        DisplayParticipants(participantService.SearchParticipants(
                            ReadText("Search name or email: ")));
                        break;

                    case "5":
                        UpdateParticipant();
                        break;

                    case "0":
                        back = true;
                        break;

                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }
            }
        }

        private static void RegistrationMenu()
        {
            bool back = false;

            while (!back)
            {
                Console.WriteLine("\n===== REGISTRATION MANAGEMENT =====");
                Console.WriteLine("1. Register Participant for Event");
                Console.WriteLine("2. View All Registrations");
                Console.WriteLine("3. Retrieve Registration by ID");
                Console.WriteLine("4. Cancel Registration");
                Console.WriteLine("5. Mark Attendance");
                Console.WriteLine("0. Back");
                Console.Write("Choice: ");

                switch (Console.ReadLine() ?? string.Empty)
                {
                    case "1":
                        {
                            int registrationId = ReadInt("Registration ID: ");
                            int eventId = ReadInt("Event ID: ");
                            int participantId = ReadInt("Participant ID: ");
                            decimal amount = ReadDecimal("Amount paid: ");

                            registrationService.RegisterParticipant(
                                registrationId,
                                eventId,
                                participantId,
                                amount,
                                out string message);

                            Console.WriteLine(message);
                            break;
                        }

                    case "2":
                        DisplayRegistrations(
                            registrationService.GetAllRegistrations());
                        break;

                    case "3":
                        {
                            Registration? r =
                                registrationService.GetRegistrationById(
                                    ReadInt("Registration ID: "));

                            DisplayRegistration(r);
                            break;
                        }

                    case "4":
                        registrationService.CancelRegistration(
                            ReadInt("Registration ID to cancel: "),
                            out string cancelMessage);

                        Console.WriteLine(cancelMessage);
                        break;

                    case "5":
                        registrationService.MarkAttendance(
                            ReadInt("Registration ID: "),
                            out string attendanceMessage);

                        Console.WriteLine(attendanceMessage);
                        break;

                    case "0":
                        back = true;
                        break;

                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }
            }
        }

        private static void SearchMenu()
        {
            bool back = false;

            while (!back)
            {
                Console.WriteLine("\n===== SEARCH / VIEW RELATIONSHIPS =====");
                Console.WriteLine("1. Participants registered for an event");
                Console.WriteLine("2. Events registered by a participant");
                Console.WriteLine("0. Back");
                Console.Write("Choice: ");

                switch (Console.ReadLine() ?? string.Empty)
                {
                    case "1":
                        {
                            int id = ReadInt("Event ID: ");

                            if (eventService.GetEventById(id) == null)
                            {
                                Console.WriteLine("Event not found.");
                                break;
                            }

                            reportService.ShowParticipantsForEvent(id);
                            break;
                        }

                    case "2":
                        {
                            int id = ReadInt("Participant ID: ");

                            if (participantService.GetParticipantById(id) == null)
                            {
                                Console.WriteLine("Participant not found.");
                                break;
                            }

                            reportService.ShowParticipantEvents(id);
                            break;
                        }

                    case "0":
                        back = true;
                        break;

                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }
            }
        }

        private static void ReportMenu()
        {
            bool back = false;

            while (!back)
            {
                Console.WriteLine("\n===== REPORTS =====");
                Console.WriteLine("1. Registrations and Participants per Event");
                Console.WriteLine("2. Remaining Capacity and Full Events");
                Console.WriteLine("3. Payments by Event and Overall");
                Console.WriteLine("4. Attendance Count and Percentage");
                Console.WriteLine("5. Registration Summary and Fee Statistics");
                Console.WriteLine("0. Back");
                Console.Write("Choice: ");

                switch (Console.ReadLine() ?? string.Empty)
                {
                    case "1":
                        reportService.ShowRegistrationsPerEvent();
                        break;

                    case "2":
                        reportService.ShowCapacityReport();
                        break;

                    case "3":
                        reportService.ShowPaymentReport();
                        break;

                    case "4":
                        reportService.ShowAttendanceReport();
                        break;

                    case "5":
                        reportService.ShowRegistrationSummary();
                        break;

                    case "0":
                        back = true;
                        break;

                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }
            }
        }

        private static void CreateEvent()
        {
            int id = ReadInt("Event ID: ");
            string name = ReadText("Event name: ");
            string venue = ReadText("Venue: ");
            DateTime date = ReadDate("Date and time (yyyy-MM-dd HH:mm): ");
            int capacity = ReadInt("Capacity: ");
            decimal fee = ReadDecimal("Registration fee: ");

            Event item = new Event(id, name, venue, date, capacity, fee);

            eventService.CreateEvent(item, out string message);
            Console.WriteLine(message);
        }

        private static void UpdateEvent()
        {
            int id = ReadInt("Event ID: ");
            string venue = ReadText("New venue: ");
            DateTime date = ReadDate("New date and time (yyyy-MM-dd HH:mm): ");
            int capacity = ReadInt("New capacity: ");
            decimal fee = ReadDecimal("New registration fee: ");

            eventService.UpdateEvent(
                id, venue, date, capacity, fee, out string message);

            Console.WriteLine(message);
        }

        private static void AddParticipant()
        {
            int id = ReadInt("Participant ID: ");
            string name = ReadText("Full name: ");
            string email = ReadText("Email: ");
            string phone = ReadText("Phone number (10-15 digits): ");

            Participant p = new Participant(id, name, email, phone);

            participantService.AddParticipant(p, out string message);
            Console.WriteLine(message);
        }

        private static void UpdateParticipant()
        {
            int id = ReadInt("Participant ID: ");
            string name = ReadText("New full name: ");
            string email = ReadText("New email: ");
            string phone = ReadText("New phone number: ");

            participantService.UpdateParticipant(
                id, name, email, phone, out string message);

            Console.WriteLine(message);
        }

        private static void DisplayEvents(List<Event> events)
        {
            if (events.Count == 0)
            {
                Console.WriteLine("No events found.");
                return;
            }

            foreach (Event e in events)
                Console.WriteLine(e);
        }

        private static void DisplayParticipants(List<Participant> participants)
        {
            if (participants.Count == 0)
            {
                Console.WriteLine("No participants found.");
                return;
            }

            foreach (Participant p in participants)
                Console.WriteLine(p);
        }

        private static void DisplayRegistrations(
            List<Registration> registrations)
        {
            if (registrations.Count == 0)
            {
                Console.WriteLine("No registrations found.");
                return;
            }

            foreach (Registration r in registrations)
                DisplayRegistration(r);
        }

        private static void DisplayRegistration(Registration? r)
        {
            if (r == null)
            {
                Console.WriteLine("Registration not found.");
                return;
            }

            Event? e = data.Events.FirstOrDefault(x => x.EventId == r.EventId);
            Participant? p = data.Participants.FirstOrDefault(
                x => x.ParticipantId == r.ParticipantId);

            Console.WriteLine(
                $"Registration ID: {r.RegistrationId} | " +
                $"Event: {e?.EventName ?? "Unknown"} | " +
                $"Participant: {p?.FullName ?? "Unknown"} | " +
                $"Date: {r.RegistrationDate:dd-MM-yyyy HH:mm} | " +
                $"Status: {r.Status} | Paid: {r.AmountPaid:C}");
        }

        private static int ReadInt(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);

                if (int.TryParse(Console.ReadLine(), out int value))
                    return value;

                Console.WriteLine("Enter a valid whole number.");
            }
        }

        private static decimal ReadDecimal(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);

                if (decimal.TryParse(
                    Console.ReadLine(),
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out decimal value))
                    return value;

                Console.WriteLine("Enter a valid amount, e.g. 250.00.");
            }
        }

        private static string ReadText(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string value = Console.ReadLine() ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(value))
                    return value.Trim();

                Console.WriteLine("This field cannot be empty.");
            }
        }

        private static DateTime ReadDate(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);

                if (DateTime.TryParseExact(
                    Console.ReadLine(),
                    "yyyy-MM-dd HH:mm",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime value))
                    return value;

                Console.WriteLine("Use the format yyyy-MM-dd HH:mm.");
            }
        }
    }
}