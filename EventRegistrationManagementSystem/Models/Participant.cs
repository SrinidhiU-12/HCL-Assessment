namespace EventRegistrationManagementSystem.Models
{
    public class Participant : Person
    {
        public int ParticipantId { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;

        public Participant() { }

        public Participant(
            int participantId,
            string fullName,
            string email,
            string phoneNumber)
        {
            ParticipantId = participantId;
            FullName = fullName;
            Email = email;
            PhoneNumber = phoneNumber;
        }

        public override string GetDetails()
        {
            return $"ID: {ParticipantId} | Name: {FullName} | " +
                   $"Email: {Email} | Phone: {PhoneNumber}";
        }

        public override string ToString() => GetDetails();
    }
}