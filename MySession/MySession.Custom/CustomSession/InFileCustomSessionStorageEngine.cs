using System.Text.Json;

namespace MySession.Custom.CustomSession
{
    public class InFileCustomSessionStorageEngine : ICustomSessionStorageEngine
    {
        private readonly string _directoryPath;

        public InFileCustomSessionStorageEngine(string directoryPath)
        {
            _directoryPath = directoryPath;
        }

        public void Commit(string id, Dictionary<string, byte[]> store)
        {
            string filePath = Path.Combine(_directoryPath, id);
            using FileStream fileStream = new FileStream(filePath, FileMode.Create);
            using StreamWriter streamWriter = new StreamWriter(fileStream);
            streamWriter.Write(JsonSerializer.Serialize(store));
        }

        public Dictionary<string, byte[]> Load(string id)
        {
            string filePath = Path.Combine(_directoryPath, id);
            if (!File.Exists(filePath))
            {
                return new Dictionary<string, byte[]>();
            }
            using FileStream fileStream = new FileStream(filePath, FileMode.Open);
            using StreamReader streamReader = new StreamReader(fileStream);
            var json = streamReader.ReadToEnd();
            return JsonSerializer.Deserialize<Dictionary<string, byte[]>>(json) ?? new Dictionary<string, byte[]>();
        }
    }
}
