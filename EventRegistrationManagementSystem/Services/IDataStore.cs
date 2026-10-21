using EventRegistrationManagementSystem.Models;

namespace EventRegistrationManagementSystem.Services
{
    public interface IDataStore
    {
        AppData Load();
        void Save(AppData data);
    }
}