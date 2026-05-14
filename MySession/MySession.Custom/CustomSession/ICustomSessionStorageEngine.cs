namespace MySession.Custom.CustomSession
{
    public interface ICustomSessionStorageEngine
    {
        void Commit(string id, Dictionary<string, byte[]> store);
        Dictionary<string, byte[]> Load(string id);
    }
}
