using AutoMapper;
using VanBlog.Core.Domain.Content;
using VanBlog.Core.Models.Common;
using VanBlog.Core.Models.Content;
using VanBlog.Core.SeedWorks;
using VanBlog.Core.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace VanBlog.Api.Services.Implement
{
    public class PostsService : IPostsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PostsService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PostDto> CreatePostAsync(CreateUpdatePostRequestDto dto)
        {
            Post post = _mapper.Map<Post>(dto);
            _unitOfWork.PostRepository.Add(post);
            
            try
            {
                int result = await _unitOfWork.CompleteAsync();
                if (result <= 0)
                {
                    throw new BadRequestException("Failed to create post.");
                }

                return _mapper.Map<PostDto>(post);
            }
            catch (DbUpdateException ex)
            {
                throw new BadRequestException(ex.InnerException?.Message ?? ex.Message);
            }
        }

        public async Task DeletePostAsync(Guid id)
        {
            var post = await GetExistedPostAsync(id);
            _unitOfWork.PostRepository.Remove(post);
            
            try
            {
                var result = await _unitOfWork.CompleteAsync();
                if (result <= 0)
                {
                    throw new BadRequestException("Failed to delete post.");
                }
            }
            catch (DbUpdateException ex)
            {
                throw new BadRequestException(ex.InnerException?.Message ?? ex.Message);
            }
        }

        public async Task<PostDto> GetPostAsync(Guid id)
        {
            var post = await GetExistedPostAsync(id);
            return _mapper.Map<PostDto>(post);
        }

        public async Task<PaginationResponseDto<PostInListDto>> GetPagedPostsAsync(PaginationRequestDto dto)
        {
            return await _unitOfWork.PostRepository.GetPagedPostsAsync(dto);
        }

        public async Task<PostDto> UpdatePostAsync(Guid id, CreateUpdatePostRequestDto dto)
        {
            var post = await GetExistedPostAsync(id);
            post = _mapper.Map(dto, post);
            
            try
            {
                var result = await _unitOfWork.CompleteAsync();
                if (result <= 0)
                {
                    throw new BadRequestException("Failed to update post.");
                }
                return _mapper.Map<PostDto>(post);
            }
            catch (DbUpdateException ex)
            {
                throw new BadRequestException(ex.InnerException?.Message ?? ex.Message);
            }
        }

        private async Task<Post> GetExistedPostAsync(Guid id)
        {
            var post = await _unitOfWork.PostRepository.GetByIdAsync(id);
            if (post == null)
            {
                throw new NotFoundException($"Post with id {id} not found.");
            }
            return post;
        }
    }
}
