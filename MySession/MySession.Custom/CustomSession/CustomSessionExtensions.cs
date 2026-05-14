namespace MySession.Custom.CustomSession
{
    public static class CustomSessionExtensions
    {
        public const string SessionKeyInCookie = "MY_SESSION_ID";

        public static ISession GetCustomSession(this HttpContext context)
        {
            string? sessionId = context.Request.Cookies[SessionKeyInCookie];
            var storageCache = context.RequestServices.GetRequiredService<CustomSessionStorageCache>();
            if (IsValidSessionId(sessionId))
            {
                if (storageCache.Session != null)
                {
                    return storageCache.Session;
                }
                storageCache.Session = context.RequestServices.GetRequiredService<ICustomSessionStorage>().Get(sessionId!);
                return storageCache.Session;
            }
            else
            {
                var session = context.RequestServices.GetRequiredService<ICustomSessionStorage>().Create();
                context.Response.Cookies.Append(SessionKeyInCookie, session.Id, new CookieOptions()
                {
                    HttpOnly = true,
                });
                return session;
            }
        }

        private static bool IsValidSessionId(string? sessionId)
        {
            return !string.IsNullOrEmpty(sessionId) && Guid.TryParse(sessionId, out _);
        }
    }
}
