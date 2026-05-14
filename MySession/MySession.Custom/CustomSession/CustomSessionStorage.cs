namespace MySession.Custom.CustomSession
{
    public class CustomSessionStorage(ICustomSessionStorageEngine engine) : ICustomSessionStorage
    {
        private readonly Dictionary<string, ISession> _sessions = [];

        public ISession Create()
        {
            string sessionId = Guid.NewGuid().ToString();
            var session = new CustomSession(sessionId, engine);
            _sessions.Add(sessionId, session);
            return session;
        }

        public ISession Get(string id)
        {
            if (_sessions.ContainsKey(id))
            {
                return _sessions[id];
            }
            var session = new CustomSession(id, engine);
            _sessions.Add(id, session);
            return session;
        }
    }
}
