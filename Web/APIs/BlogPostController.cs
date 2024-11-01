using AutoMapper;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Extensions;
using Web.Services;
using Web.ViewModels.Blog;
using Web.ViewModels.QueryFilters;
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
    public ApiResponsePaged<Post> GetList([FromQuery] PostQueryParameters param)
    {
        var pagedList = _postService.GetPagedList(param);
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

    // FIXME: This parameter for adding articles is incorrect, we need to change it to DTO
    [HttpPost]
    public ApiResponse<Post> Add(PostCreationDto dto,
        [FromServices] CategoryService categoryService)
    {
        var post = _mapper.Map<Post>(dto);
        var category = categoryService.GetById(dto.CategoryId);
        if (category == null) return ApiResponse.BadRequest($"Category {dto.CategoryId} does not exist!");

        post.Id = Guid.NewGuid().ToString();
        post.CreationTime = DateTime.Now;
        post.LastModifiedTime = DateTime.Now;

        var categories = new List<Category> { category };
        var parent = category.Parent;
        while (parent != null)
        {
            categories.Add(parent);
            parent = parent.Parent;
        }

        categories.Reverse();
        post.Categories = string.Join(",", categories.Select(a => a.Id));
        return new ApiResponse<Post>(_postService.InsertOrUpdate(post));
    }

    [HttpPut("{id}")]
    public ApiResponse<Post> Update(string id, PostUpdateDto dto)
    {
        var post = _postService.GetById(id);
        if (post == null) return ApiResponse.NotFound($"Blog {id} does not exist");
        // mapper.Map(source) creates a completely new object
        // mapper.Map(source, dest) modifies the source object
        post = _mapper.Map(dto, post);
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
        return ApiResponse.Ok(new
        {
            imgUrl,
            imgName = Path.GetFileNameWithoutExtension(imgUrl)
        });
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