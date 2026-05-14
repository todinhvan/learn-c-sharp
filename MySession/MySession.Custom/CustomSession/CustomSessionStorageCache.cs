namespace MySession.Custom.CustomSession
{
    public class CustomSessionStorageCache
    {
        public ISession? Session { get; set; }

        public CustomSessionStorageCache(ILogger<CustomSessionStorageCache> logger) { 
            logger.LogInformation("CustomSessionStorageCache created");
        }
    }
}
