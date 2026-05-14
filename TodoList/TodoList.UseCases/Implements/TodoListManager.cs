using Microsoft.EntityFrameworkCore;
using TodoList.Core.Entities;
using TodoList.Core.Exceptions;
using TodoList.Core.Models.Item;
using TodoList.Core.SeedWorks;

namespace TodoList.UseCases.Implements
{
    public class TodoListManager : ITodoListManager
    {
        private readonly IUnitOfWork _unitOfWork;

        public TodoListManager(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Item> CreateItemAsync(CreateItemDto dto)
        {
            try
            {
                Item item = new Item
                {
                    Id = Guid.NewGuid(),
                    Title = dto.Title,
                    Description = dto.Description,
                };

                _unitOfWork.ItemRepository.Add(item);
                await _unitOfWork.CompleteAsync();

                return item;
            }
            catch (Exception ex)
            {
                if (ex is DbUpdateException)
                {
                    throw new BadRequestException(ex.Message);
                }
                throw new InternalServerErrorException("Error in creating item");
            }
        }

        public async Task DeleteItemAsync(Guid id)
        {
            Item item = await GetItemAsync(id);
            try
            {
                _unitOfWork.ItemRepository.Remove(item);
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception ex)
            {
                if (ex is DbUpdateException)
                {
                    throw new BadRequestException(ex.Message);
                }
                throw new InternalServerErrorException("Error in deleting item");
            }
        }

        public async Task<Item> GetItemAsync(Guid id)
        {
            var item = await _unitOfWork.ItemRepository.GetByIdAsync(id);
            if (item == null)
            {
                throw new NotFoundException($"Item with id {id} not found");
            }
            return item;
        }

        public async Task<IEnumerable<Item>> GetItemsAsync()
        {
            return await _unitOfWork.ItemRepository.GetAllAsync();
        }

        public async Task<Item> UpdateItemAsync(Guid id, UpdateItemDto dto)
        {
            var item = await GetItemAsync(id);
            try
            {
                item.Title = dto.Title;
                item.Description = dto.Description;
                item.IsCompleted = dto.IsCompleted;
                await _unitOfWork.CompleteAsync();
                return item;
            }
            catch (Exception ex)
            {
                if (ex is DbUpdateException)
                {
                    throw new BadRequestException(ex.Message);
                }
                throw new InternalServerErrorException("Error in updating item");
            }
        }

        public async Task<Item> ToggleCompleteAsync(Guid id)
        {
            var item = await GetItemAsync(id);
            try
            {
                item.IsCompleted = !item.IsCompleted;
                await _unitOfWork.CompleteAsync();
                return item;
            }
            catch (Exception ex)
            {
                if (ex is DbUpdateException)
                {
                    throw new BadRequestException(ex.Message);
                }
                throw new InternalServerErrorException("Error in toggling item completion");
            }
        }
    }
}
