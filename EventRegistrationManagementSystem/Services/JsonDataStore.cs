using System.Text.Json;
using System.Text.Json.Serialization;
using EventRegistrationManagementSystem.Models;
using EventRegistrationManagementSystem.Exceptions;

namespace EventRegistrationManagementSystem.Services
{
    public class JsonDataStore : IDataStore
    {
        private readonly string _filePath;

        private readonly JsonSerializerOptions _options = new()
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public JsonDataStore(string filePath)
        {
            _filePath = filePath;
        }

        public AppData Load()
        {
            try
            {
                if (!File.Exists(_filePath))
                    return new AppData();

                string json = File.ReadAllText(_filePath);

                if (string.IsNullOrWhiteSpace(json))
                    return new AppData();

                return JsonSerializer.Deserialize<AppData>(json, _options)
                       ?? new AppData();
            }
            catch (JsonException ex)
            {
                throw new DataPersistenceException(
                    "The JSON file contains invalid data.", ex);
            }
            catch (IOException ex)
            {
                throw new DataPersistenceException(
                    "Unable to read the data file.", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new DataPersistenceException(
                    "Permission denied while reading the data file.", ex);
            }
        }

        public void Save(AppData data)
        {
            try
            {
                string? directory = Path.GetDirectoryName(_filePath);

                if (!string.IsNullOrWhiteSpace(directory))
                    Directory.CreateDirectory(directory);

                string json = JsonSerializer.Serialize(data, _options);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex) when (
                ex is IOException ||
                ex is UnauthorizedAccessException ||
                ex is JsonException)
            {
                throw new DataPersistenceException(
                    "Unable to save application data.", ex);
            }
        }
    }
}