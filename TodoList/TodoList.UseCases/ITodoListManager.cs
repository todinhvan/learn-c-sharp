using TodoList.Core.Entities;
using TodoList.Core.Models.Item;

namespace TodoList.UseCases
{
    public interface ITodoListManager
    {
        Task<Item> GetItemAsync(Guid id);
        Task<IEnumerable<Item>> GetItemsAsync();
        Task<Item> CreateItemAsync(CreateItemDto dto);
        Task<Item> UpdateItemAsync(Guid id, UpdateItemDto dto);
        Task<Item> ToggleCompleteAsync(Guid id);
        Task DeleteItemAsync(Guid id);
    }
}
