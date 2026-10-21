namespace EventRegistrationManagementSystem.Models
{
    public abstract class Person
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public abstract string GetDetails();
    }
}