using OpenIDConnect.Server.Models;

namespace OpenIDConnect.Server.Repositories
{
    public class InMemoryCodeItemRepository : ICodeItemRepository
    {
        private readonly Dictionary<string, CodeItem> _items = [];
        public void Add(string code, CodeItem codeItem)
        {
            _items[code] = codeItem;
        }

        public void Delete(string code)
        {
            _items.Remove(code);
        }

        public CodeItem? FindByCode(string code)
        {
            return _items.TryGetValue(code, out var codeItem) ? codeItem : null;
        }
    }
}
