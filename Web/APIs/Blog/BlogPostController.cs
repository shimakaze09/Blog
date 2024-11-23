using AutoMapper;
using CodeLab.Share.Extensions;
using CodeLab.Share.ViewModels.Response;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Share.Utils;
using Web.Extensions;
using Web.Services;
using Web.ViewModels.Blog;
using Web.ViewModels.QueryFilters;

namespace Web.APIs.Blog;

/// <summary>
///     Blog Post Controller
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = ApiGroups.Blog)]
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
    public async Task<ApiResponsePaged<Post>> GetList([FromQuery] PostQueryParameters param)
    {
        // Set admin mode to true if the user is authenticated
        // TODO: Replace this with role-based admin determination later
        var adminMode = User.Identity?.IsAuthenticated ?? false;

        // Fetch paginated list of items
        var pagedList = await _postService.GetPagedList(param, adminMode);

        return new ApiResponsePaged<Post>
        {
            Message = "Get posts list",
            Data = pagedList.ToList(),
            Pagination = pagedList.ToPaginationMetadata()
        };
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<ApiResponse<Post>> Get(string id)
    {
        var post = await _postService.GetById(id);
        return post == null ? ApiResponse.NotFound() : new ApiResponse<Post>(post);
    }

    [HttpDelete("{id}")]
    public async Task<ApiResponse> Delete(string id)
    {
        var post = await _postService.GetById(id);
        if (post == null) return ApiResponse.NotFound($"Blog {id} does not exist");
        var rows = await _postService.Delete(id);
        return ApiResponse.Ok($"Deleted {rows} blog posts");
    }

    [HttpPost]
    public async Task<ApiResponse<Post>> Add(PostCreationDto dto,
        [FromServices] CategoryService categoryService)
    {
        var post = _mapper.Map<Post>(dto);
        var category = await categoryService.GetById(dto.CategoryId);
        if (category == null) return ApiResponse.BadRequest($"Category {dto.CategoryId} does not exist!");

        if (!string.IsNullOrWhiteSpace(dto.Slug) && !await _postService.CheckSlugAvailable(dto.Slug))
            return ApiResponse.BadRequest("The specified slug is already in use by another post!");

        post.Id = GuidUtils.GuidTo16String();
        post.CreationTime = DateTime.Now;
        post.LastUpdateTime = DateTime.Now;
        post.IsPublish = true;

        // Get the category hierarchy
        post.Categories = categoryService.GetCategoryBreadcrumb(category);

        return new ApiResponse<Post>(await _postService.InsertOrUpdateAsync(post));
    }

    [HttpPut("{id}")]
    public async Task<ApiResponse<Post>> Update(string id, PostUpdateDto dto)
    {
        var post = await _postService.GetById(id);
        if (post == null) return ApiResponse.NotFound($"Blog {id} does not exist");

        if (!string.IsNullOrWhiteSpace(dto.Slug))
            if (dto.Slug != post.Slug && !await _postService.CheckSlugAvailable(dto.Slug))
                return ApiResponse.BadRequest("The specified slug is already being used by another post!");

        // mapper.Map(source) gets a brand new object
        // mapper.Map(source, dest) modifies the dest object
        post = _mapper.Map(dto, post);
        post.LastUpdateTime = DateTime.Now;
        return new ApiResponse<Post>(await _postService.InsertOrUpdateAsync(post));
    }

    /// <summary>
    ///     Upload an image
    /// </summary>
    /// <param name="id">The ID of the blog post</param>
    /// <param name="file">The uploaded image file</param>
    /// <returns>The URL of the uploaded image</returns>
    [HttpPost("{id}/[action]")]
    public async Task<ApiResponse> UploadImage(string id, IFormFile file)
    {
        var post = await _postService.GetById(id);
        if (post == null) return ApiResponse.NotFound($"Blog {id} does not exist");
        var imgUrl = await _postService.UploadImage(post, file);
        return ApiResponse.Ok(new
        {
            imgUrl,
            imgName = Path.GetFileNameWithoutExtension(imgUrl)
        });
    }

    /// <summary>
    ///     Get images in the post
    /// </summary>
    /// <param name="id">The ID of the blog post</param>
    /// <returns>A list of image URLs</returns>
    [HttpGet("{id}/[action]")]
    public async Task<ApiResponse<List<string>>> Images(string id)
    {
        var post = await _postService.GetById(id);
        if (post == null) return ApiResponse.NotFound($"Blog {id} does not exist");
        return new ApiResponse<List<string>>(_postService.GetImages(post));
    }

    /// <summary>
    ///     Set as featured blog
    /// </summary>
    /// <param name="id">The ID of the blog post</param>
    /// <returns>An ApiResponse object containing FeaturedPost data</returns>
    [HttpPost("{id}/[action]")]
    public async Task<ApiResponse<FeaturedPost>> SetFeatured(string id)
    {
        var post = await _postService.GetById(id);
        return post == null
            ? ApiResponse.NotFound()
            : new ApiResponse<FeaturedPost>(await _blogService.AddFeaturedPost(post));
    }

    /// <summary>
    ///     Cancel featured blog
    /// </summary>
    /// <param name="id">The ID of the blog post</param>
    /// <returns>An ApiResponse object</returns>
    [HttpPost("{id}/[action]")]
    public async Task<ApiResponse> CancelFeatured(string id)
    {
        var post = await _postService.GetById(id);
        if (post == null) return ApiResponse.NotFound($"Blog {id} does not exist");
        var rows = await _blogService.DeleteFeaturedPost(post);
        return ApiResponse.Ok($"Deleted {rows} rows.");
    }

    /// <summary>
    ///     Set as top post (only one top post allowed)
    /// </summary>
    /// <param name="id">The ID of the blog post</param>
    /// <returns>An ApiResponse object containing TopPost data</returns>
    [HttpPost("{id}/[action]")]
    public async Task<ApiResponse<TopPost>> SetTop(string id)
    {
        var post = await _postService.GetById(id);
        if (post == null) return ApiResponse.NotFound($"Blog {id} does not exist");
        var (data, rows) = await _blogService.SetTopPost(post);
        return new ApiResponse<TopPost> { Data = data, Message = $"ok. Deleted {rows} old top posts." };
    }
}