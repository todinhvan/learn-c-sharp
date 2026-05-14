namespace MySession.Custom.CustomSession
{
    public interface ICustomSessionStorage
    {
        ISession Create();
        ISession Get(string id);
    }
}
