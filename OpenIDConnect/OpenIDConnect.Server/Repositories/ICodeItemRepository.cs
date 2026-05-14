using OpenIDConnect.Server.Models;

namespace OpenIDConnect.Server.Repositories
{
    public interface ICodeItemRepository
    {
        CodeItem? FindByCode(string code);
        void Add(string code, CodeItem codeItem);
        void Delete(string code);
    }
}
