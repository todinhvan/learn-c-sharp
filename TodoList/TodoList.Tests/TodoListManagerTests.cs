using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using TodoList.Core.Entities;
using TodoList.Core.Exceptions;
using TodoList.Core.Models.Item;
using TodoList.Core.Repositories;
using TodoList.Core.SeedWorks;
using TodoList.UseCases.Implements;
using Xunit;

namespace TodoList.Tests
{
    public class TodoListManagerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IItemRepository> _mockItemRepository;
        private readonly TodoListManager _manager;

        public TodoListManagerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockItemRepository = new Mock<IItemRepository>();
            _mockUnitOfWork.Setup(u => u.ItemRepository).Returns(_mockItemRepository.Object);
            _manager = new TodoListManager(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task CreateItemAsync_ShouldReturnItem_WhenSuccessful()
        {
            // Arrange
            var dto = new CreateItemDto { Title = "Test Title", Description = "Test Description" };
            
            // Act
            var result = await _manager.CreateItemAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Title", result.Title);
            Assert.Equal("Test Description", result.Description);
            _mockItemRepository.Verify(r => r.Add(It.IsAny<Item>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateItemAsync_ShouldThrowBadRequestException_OnDbUpdateException()
        {
            // Arrange
            var dto = new CreateItemDto { Title = "Test Title" };
            _mockUnitOfWork.Setup(u => u.CompleteAsync()).ThrowsAsync(new DbUpdateException("DB Error"));

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _manager.CreateItemAsync(dto));
        }

        [Fact]
        public async Task GetItemAsync_ShouldReturnItem_WhenExists()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var item = new Item { Id = itemId, Title = "Test" };
            _mockItemRepository.Setup(r => r.GetByIdAsync(itemId)).ReturnsAsync(item);

            // Act
            var result = await _manager.GetItemAsync(itemId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(itemId, result.Id);
        }

        [Fact]
        public async Task GetItemAsync_ShouldThrowNotFoundException_WhenDoesNotExist()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            _mockItemRepository.Setup(r => r.GetByIdAsync(itemId)).ReturnsAsync((Item?)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _manager.GetItemAsync(itemId));
        }

        [Fact]
        public async Task GetItemsAsync_ShouldReturnAllItems()
        {
            // Arrange
            var items = new List<Item> { new Item { Title = "1" }, new Item { Title = "2" } };
            _mockItemRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(items);

            // Act
            var result = await _manager.GetItemsAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task UpdateItemAsync_ShouldUpdateAndReturnItem_WhenSuccessful()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var item = new Item { Id = itemId, Title = "Old Title", IsCompleted = false };
            var dto = new UpdateItemDto { Title = "New Title", Description = "Desc", IsCompleted = true };
            _mockItemRepository.Setup(r => r.GetByIdAsync(itemId)).ReturnsAsync(item);

            // Act
            var result = await _manager.UpdateItemAsync(itemId, dto);

            // Assert
            Assert.Equal("New Title", result.Title);
            Assert.Equal("Desc", result.Description);
            Assert.True(result.IsCompleted);
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateItemAsync_ShouldThrowNotFoundException_WhenItemNotFound()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var dto = new UpdateItemDto { Title = "New Title" };
            _mockItemRepository.Setup(r => r.GetByIdAsync(itemId)).ReturnsAsync((Item?)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _manager.UpdateItemAsync(itemId, dto));
        }

        [Fact]
        public async Task UpdateItemAsync_ShouldThrowBadRequestException_OnDbUpdateException()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var item = new Item { Id = itemId, Title = "Old Title" };
            var dto = new UpdateItemDto { Title = "New Title" };
            _mockItemRepository.Setup(r => r.GetByIdAsync(itemId)).ReturnsAsync(item);
            _mockUnitOfWork.Setup(u => u.CompleteAsync()).ThrowsAsync(new DbUpdateException("DB Error"));

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _manager.UpdateItemAsync(itemId, dto));
        }

        [Fact]
        public async Task ToggleCompleteAsync_ShouldToggleStatusAndReturnItem()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var item = new Item { Id = itemId, Title = "Title", IsCompleted = false };
            _mockItemRepository.Setup(r => r.GetByIdAsync(itemId)).ReturnsAsync(item);

            // Act
            var result = await _manager.ToggleCompleteAsync(itemId);

            // Assert
            Assert.True(result.IsCompleted);
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteItemAsync_ShouldRemoveItem_WhenSuccessful()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var item = new Item { Id = itemId, Title = "Title" };
            _mockItemRepository.Setup(r => r.GetByIdAsync(itemId)).ReturnsAsync(item);

            // Act
            await _manager.DeleteItemAsync(itemId);

            // Assert
            _mockItemRepository.Verify(r => r.Remove(item), Times.Once);
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteItemAsync_ShouldThrowNotFoundException_WhenItemNotFound()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            _mockItemRepository.Setup(r => r.GetByIdAsync(itemId)).ReturnsAsync((Item?)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _manager.DeleteItemAsync(itemId));
        }

        [Fact]
        public async Task DeleteItemAsync_ShouldThrowBadRequestException_OnDbUpdateException()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var item = new Item { Id = itemId, Title = "Title" };
            _mockItemRepository.Setup(r => r.GetByIdAsync(itemId)).ReturnsAsync(item);
            _mockUnitOfWork.Setup(u => u.CompleteAsync()).ThrowsAsync(new DbUpdateException("DB Error"));

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _manager.DeleteItemAsync(itemId));
        }
    }
}
