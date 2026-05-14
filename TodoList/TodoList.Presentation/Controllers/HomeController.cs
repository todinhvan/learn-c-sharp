using Microsoft.AspNetCore.Mvc;
using TodoList.Core.Models.Item;
using TodoList.Presentation.Models;
using TodoList.UseCases;

namespace TodoList.Presentation.Controllers
{
    public class HomeController : Controller
    {
        private readonly ITodoListManager _manager;

        public HomeController(ITodoListManager manager)
        {
            _manager = manager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var items = await _manager.GetItemsAsync();
            return View(items.Select(item => new ItemViewModel()
            {
                Id = item.Id,
                Title = item.Title,
                Description = item.Description,
                IsCompleted = item.IsCompleted,
                CreatedAt = item.CreatedAt
            }));
        }

        [HttpGet("Detail/{id}")]
        public async Task<IActionResult> Detail(Guid id)
        {
            var item = await _manager.GetItemAsync(id);
            return View(new ItemViewModel()
            {
                Id = item.Id,
                Title = item.Title,
                Description = item.Description,
                IsCompleted = item.IsCompleted,
                CreatedAt = item.CreatedAt
            });
        }

        [HttpGet("Add")]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost("Add")]
        public async Task<IActionResult> Add(CreateItemDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }
            await _manager.CreateItemAsync(dto);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var item = await _manager.GetItemAsync(id);
            var dto = new UpdateItemDto
            {
                Title = item.Title,
                Description = item.Description,
                IsCompleted = item.IsCompleted
            };
            ViewBag.Id = item.Id;
            return View(dto);
        }

        [HttpPost("Edit/{id}")]
        public async Task<IActionResult> Edit(Guid id, UpdateItemDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Id = id;
                return View(dto);
            }
            await _manager.UpdateItemAsync(id, dto);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _manager.DeleteItemAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("ToggleComplete/{id}")]
        public async Task<IActionResult> ToggleComplete(Guid id)
        {
            await _manager.ToggleCompleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
