using System.Diagnostics.CodeAnalysis;

namespace MySession.Custom.CustomSession
{
    public class CustomSession(string id, ICustomSessionStorageEngine engine) : ISession
    {
        private readonly Dictionary<string, byte[]> _store = [];
        public bool IsAvailable
        {
            get
            {
                engine.Load(id);
                return true;
            }
        }

        public string Id => id;

        public IEnumerable<string> Keys => _store.Keys;

        public void Clear()
        {
            _store.Clear();
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            engine.Commit(id, _store);
        }

        public async Task LoadAsync(CancellationToken cancellationToken = default)
        {
            _store.Clear();
            var data = engine.Load(id);
            foreach (var item in data)
            {
                _store.Add(item.Key, item.Value);
            }
        }

        public void Remove(string key)
        {
            _store.Remove(key);
        }

        public void Set(string key, byte[] value)
        {
            _store[key] = value;
        }

        public bool TryGetValue(string key, [NotNullWhen(true)] out byte[]? value)
        {
            return _store.TryGetValue(key, out value);
        }
    }
}
