using AutoMapper;
using GHTK.Api.Models;
using GHTK.Infrastructure.Entities;
using GHTK.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GHTK.Api.Controllers
{
    [Route("services/[controller]")]
    [ApiController]
    public class ShipmentController(IOrderRepository orderRepository, IMapper mapper) : ControllerBase
    {
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateOrder(GhtkCreateOrderRequest model)
        {
            var partnerId = HttpContext.User.FindFirstValue("PartnerId");
            if (string.IsNullOrEmpty(partnerId)) {
                return Unauthorized();
            }

            Order order = mapper.Map<Order>(model);
            order.PartnerId = partnerId;
            await orderRepository.CreateOrderAsync(order);

            var response = new GhtkCreateOrderResponse
            {
                Success = true,

                Order = new OrderResponse
                {
                    PartnerId = partnerId,
                    Label = "label",
                    Area = 1,
                    Fee = 1.0,
                    InsuranceFee = 1.0,
                    TrackingId = order.TrackingId,
                    EstimatedPickTime = "2021-01-01T00:00:00Z",
                    EstimatedDeliverTime = "2021-01-01T00:00:00Z",
                    Products = order.Products.Select(p => new GhtkProduct()
                    {
                        Name = p.Name,
                        Quantity = p.Quantity,
                        Weight = p.Weight,
                        ProductCode = p.ProductCode
                    }).ToArray(),
                    StatusId = order.Status
                }
            };

            return Ok(response);
        }

        [HttpGet("v2/{id}")]
        [Authorize]
        public async Task<IActionResult> GetOrderStatus(string id)
        {
            var partnerId = HttpContext.User.FindFirstValue("PartnerId");
            if (string.IsNullOrEmpty(partnerId))
            {
                return Unauthorized();
            }

            var order = await orderRepository.FindOrderAsync(id, partnerId);
            if (order == null)
            {
                return NotFound(new ApiResponse()
                {
                    Success = false,
                    Message = "Tracking Id not found"
                });
            }

            var response = new GhtkOrderStatusResponse()
            {
                Success = true,
                Order = new GhtkStatusOrder
                {
                    LabelId = "label_id",
                    PartnerId = partnerId,
                    Status = order.Status,
                    StatusText = "status_text",
                    Created = DateTimeOffset.Now,
                    Modified = DateTimeOffset.Now,
                    Message = "message",
                    PickDate = DateTimeOffset.Now,
                    DeliverDate = DateTimeOffset.Now,
                    CustomerFullname = "customer_fullname",
                    CustomerTel = "customer_tel",
                    Address = order.Address,
                    StorageDay = 1,
                    ShipMoney = 1,
                    Insurance = 1,
                    Value = order.Value
                }
            };

            return Ok(response);
        }
        [HttpPost("cancel/{id}")]
        [Authorize]
        public async Task<IActionResult> CancelOrder(string id)
        {
            var partnerId = HttpContext.User.FindFirstValue("PartnerId");
            if (string.IsNullOrEmpty(partnerId))
            {
                return Unauthorized();
            }

            var order = await orderRepository.FindOrderAsync(id, partnerId);
            if (order == null)
            {
                return NotFound(new ApiResponse()
                {
                    Success = false,
                    Message = "Tracking Id not found"
                });
            }

            await orderRepository.CancelOrderAsync(order.TrackingId, partnerId);

            return Ok(new ApiResponse()
            {
                Success = true,
                Message = "Order canceled"
            });
        }
    }
}
