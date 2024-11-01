using AutoMapper;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Extensions;
using Web.Services;
using Web.ViewModels.Blog;
using Web.ViewModels.Response;

namespace Web.Apis;

/// <summary>
///     Article
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "blog")]
public class BlogPostController : ControllerBase
{
    private readonly BlogService _blogService;
    private readonly IMapper _mapper;
    private readonly PostService _postService;

    public BlogPostController(PostService postService, BlogService blogService, IMapper mapper)
    {
        _postService = postService;
        _blogService = blogService;
        _mapper = mapper;
    }

    [AllowAnonymous]
    [HttpGet]
    public ApiResponsePaged<Post> GetList(int categoryId = 0, int page = 1, int pageSize = 10)
    {
        var pagedList = _postService.GetPagedList(categoryId, page, pageSize);
        return new ApiResponsePaged<Post>
        {
            Message = "Get posts list",
            Data = pagedList.ToList(),
            Pagination = pagedList.ToPaginationMetadata()
        };
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public ApiResponse<Post> Get(string id)
    {
        var post = _postService.GetById(id);
        return post == null ? ApiResponse.NotFound() : new ApiResponse<Post>(post);
    }

    [HttpDelete("{id}")]
    public ApiResponse Delete(string id)
    {
        var post = _postService.GetById(id);
        if (post == null) return ApiResponse.NotFound($"Blog {id} does not exist");
        var rows = _postService.Delete(id);
        return ApiResponse.Ok($"Deleted {rows} blog posts");
    }

    [HttpPost]
    public ApiResponse<Post> Add(Post post)
    {
        return new ApiResponse<Post>(_postService.InsertOrUpdate(post));
    }

    [HttpPut("{id}")]
    public ApiResponse<Post> Update(string id, PostUpdateDto dto)
    {
        var post = _postService.GetById(id);
        if (post == null) return ApiResponse.NotFound($"Blog {id} does not exist");
        post = _mapper.Map<Post>(dto);
        post.LastModifiedTime = DateTime.Now;
        return new ApiResponse<Post>(_postService.InsertOrUpdate(post));
    }

    /// <summary>
    ///     Upload image
    /// </summary>
    /// <param name="id">The ID of the blog post</param>
    /// <param name="file">The uploaded image file</param>
    /// <returns>The URL of the uploaded image</returns>
    [HttpPost("{id}/[action]")]
    public ApiResponse UploadImage(string id, IFormFile file)
    {
        var post = _postService.GetById(id);
        if (post == null) return ApiResponse.NotFound($"Blog {id} does not exist");
        var imgUrl = _postService.UploadImage(post, file);
        return ApiResponse.Ok(new { imgUrl });
    }

    /// <summary>
    ///     Gets images from a blog post
    /// </summary>
    /// <param name="id">The ID of the blog post</param>
    /// <returns>A list of image URLs</returns>
    [HttpGet("{id}/[action]")]
    public ApiResponse<List<string>> Images(string id)
    {
        var post = _postService.GetById(id);
        if (post == null) return ApiResponse.NotFound($"Blog {id} does not exist");
        return new ApiResponse<List<string>>(_postService.GetImages(post));
    }

    /// <summary>
    ///     Set as featured blog
    /// </summary>
    /// <param name="id">The ID of the blog post</param>
    /// <returns>An ApiResponse object containing FeaturedPost data</returns>
    [HttpPost("{id}/[action]")]
    public ApiResponse<FeaturedPost> SetFeatured(string id)
    {
        var post = _postService.GetById(id);
        return post == null
            ? ApiResponse.NotFound()
            : new ApiResponse<FeaturedPost>(_blogService.AddFeaturedPost(post));
    }

    /// <summary>
    ///     Cancel featured blog
    /// </summary>
    /// <param name="id">The ID of the blog post</param>
    /// <returns>An ApiResponse object</returns>
    [HttpPost("{id}/[action]")]
    public ApiResponse CancelFeatured(string id)
    {
        var post = _postService.GetById(id);
        if (post == null) return ApiResponse.NotFound($"Blog {id} does not exist");
        var rows = _blogService.DeleteFeaturedPost(post);
        return ApiResponse.Ok($"deleted {rows} rows.");
    }

    /// <summary>
    ///     Set as top post (can only have one top post)
    /// </summary>
    /// <param name="id">The ID of the blog post</param>
    /// <returns>An ApiResponse object containing TopPost data</returns>
    [HttpPost("{id}/[action]")]
    public ApiResponse<TopPost> SetTop(string id)
    {
        var post = _postService.GetById(id);
        if (post == null) return ApiResponse.NotFound($"Blog {id} does not exist");
        var (data, rows) = _blogService.SetTopPost(post);
        return new ApiResponse<TopPost> { Data = data, Message = $"ok. deleted {rows} old topPosts." };
    }
}