using System.Text.RegularExpressions;
using EventRegistrationManagementSystem.Models;

namespace EventRegistrationManagementSystem.Services
{
    public class ParticipantService
    {
        private readonly AppData _data;
        private readonly IDataStore _dataStore;

        public ParticipantService(AppData data, IDataStore dataStore)
        {
            _data = data;
            _dataStore = dataStore;
        }

        private void SaveChanges() => _dataStore.Save(_data);

        private static bool IsValidEmail(string email)
        {
            return Regex.IsMatch(
                email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        private static bool IsValidPhone(string phone)
        {
            return Regex.IsMatch(phone, @"^\+?[0-9]{10,15}$");
        }

        public bool AddParticipant(
            Participant participant,
            out string message)
        {
            if (participant.ParticipantId <= 0)
            {
                message = "Participant ID must be positive.";
                return false;
            }

            if (_data.Participants.Any(p =>
                p.ParticipantId == participant.ParticipantId))
            {
                message = "Participant ID already exists.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(participant.FullName))
            {
                message = "Participant name is required.";
                return false;
            }

            string email = participant.Email.Trim();
            string phone = participant.PhoneNumber.Trim();

            if (!IsValidEmail(email))
            {
                message = "Enter a valid email address.";
                return false;
            }

            if (!IsValidPhone(phone))
            {
                message = "Enter a valid phone number (10-15 digits).";
                return false;
            }

            if (_data.Participants.Any(p =>
                string.Equals(p.Email, email,
                    StringComparison.OrdinalIgnoreCase)))
            {
                message = "Email address is already registered.";
                return false;
            }

            participant.FullName = participant.FullName.Trim();
            participant.Email = email;
            participant.PhoneNumber = phone;

            _data.Participants.Add(participant);
            SaveChanges();

            message = "Participant added successfully.";
            return true;
        }

        public List<Participant> GetAllParticipants()
        {
            return _data.Participants
                .OrderBy(p => p.FullName)
                .ToList();
        }

        public Participant? GetParticipantById(int participantId)
        {
            return _data.Participants.FirstOrDefault(p =>
                p.ParticipantId == participantId);
        }

        public List<Participant> SearchParticipants(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return new List<Participant>();

            searchText = searchText.Trim();

            return _data.Participants
                .Where(p =>
                    p.FullName.Contains(
                        searchText, StringComparison.OrdinalIgnoreCase) ||
                    p.Email.Contains(
                        searchText, StringComparison.OrdinalIgnoreCase))
                .OrderBy(p => p.FullName)
                .ToList();
        }

        public bool UpdateParticipant(
            int participantId,
            string fullName,
            string email,
            string phoneNumber,
            out string message)
        {
            Participant? participant = GetParticipantById(participantId);

            if (participant == null)
            {
                message = "Participant not found.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(fullName))
            {
                message = "Participant name is required.";
                return false;
            }

            email = email.Trim();
            phoneNumber = phoneNumber.Trim();

            if (!IsValidEmail(email))
            {
                message = "Enter a valid email address.";
                return false;
            }

            if (!IsValidPhone(phoneNumber))
            {
                message = "Enter a valid phone number.";
                return false;
            }

            if (_data.Participants.Any(p =>
                p.ParticipantId != participantId &&
                string.Equals(p.Email, email,
                    StringComparison.OrdinalIgnoreCase)))
            {
                message = "Email address is already used by another participant.";
                return false;
            }

            participant.FullName = fullName.Trim();
            participant.Email = email;
            participant.PhoneNumber = phoneNumber;

            SaveChanges();

            message = "Participant updated successfully.";
            return true;
        }
    }
}