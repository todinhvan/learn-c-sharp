using Microsoft.AspNetCore.Mvc;
using VanBlog.Api.Services;
using VanBlog.Core.Models.Common;
using VanBlog.Core.Models.Content;

namespace VanBlog.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly IPostsService _service;
        public PostsController(IPostsService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<ApiSuccessResponseDto<PostDto>>> CreatePost([FromBody] CreateUpdatePostRequestDto dto)
        {
            var data = await _service.CreatePostAsync(dto);
            return Ok(new ApiSuccessResponseDto<PostDto>
            {
                StatusCode = System.Net.HttpStatusCode.Created,
                Message = "Post created successfully",
                Data = data
            });
        }

        [HttpGet]
        public async Task<ActionResult<ApiSuccessResponseDto<PaginationResponseDto<PostInListDto>>>> GetPagedPosts([FromQuery] PaginationRequestDto dto)
        {
            var data = await _service.GetPagedPostsAsync(dto);
            return Ok(new ApiSuccessResponseDto<PaginationResponseDto<PostInListDto>>
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Message = "Posts retrieved successfully",
                Data = data
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiSuccessResponseDto<PostDto>>> GetPostById(Guid id)
        {
            var data = await _service.GetPostAsync(id);
            return Ok(new ApiSuccessResponseDto<PostDto>
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Message = "Post retrieved successfully",
                Data = data
            });
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiSuccessResponseDto<PostDto>>> UpdatePost(Guid id, [FromBody] CreateUpdatePostRequestDto dto)
        {
            var data = await _service.UpdatePostAsync(id, dto);
            return Ok(new ApiSuccessResponseDto<PostDto>
            {
                StatusCode = System.Net.HttpStatusCode.Accepted,
                Message = "Post updated successfully",
                Data = data
            });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiSuccessResponseDto<object?>>> DeletePost(Guid id)
        {
            await _service.DeletePostAsync(id);
            return Ok(new ApiSuccessResponseDto<string>
            {
                StatusCode = System.Net.HttpStatusCode.NoContent,
                Message = "Post deleted successfully",
            });
        }
    }
}