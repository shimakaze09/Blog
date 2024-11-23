using Data.Models;
using FreeSql;
using Microsoft.AspNetCore.Mvc;
using Web.Contrib.SiteMessage;
using Web.Services;
using Web.ViewModels.Blog;
using Web.ViewModels.QueryFilters;

namespace Web.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class BlogController : Controller
{
    private readonly IBaseRepository<Category> _categoryRepo;
    private readonly CategoryService _categoryService;
    private readonly ConfigService _configService;
    private readonly MessageService _messages;
    private readonly IBaseRepository<Post> _postRepo;
    private readonly PostService _postService;

    public BlogController(IBaseRepository<Post> postRepo, IBaseRepository<Category> categoryRepo,
        PostService postService,
        MessageService messages,
        CategoryService categoryService,
        ConfigService configService)
    {
        _postRepo = postRepo;
        _categoryRepo = categoryRepo;
        _postService = postService;
        _messages = messages;
        _categoryService = categoryService;
        _configService = configService;
    }

    public async Task<IActionResult> List(int categoryId = 0, int page = 1, int pageSize = 6,
        string sortType = "asc", string sortBy = "-CreationTime")
    {
        var currentCategory = categoryId == 0
            ? new Category { Id = 0, Name = "All" }
            : await _categoryRepo.Where(a => a.Id == categoryId).FirstAsync();

        if (currentCategory == null)
        {
            _messages.Error($"Category {categoryId} not exists!");
            return RedirectToAction(nameof(List));
        }

        if (!currentCategory.Visible)
        {
            _messages.Warning($"Category {categoryId} not visible!");
            return RedirectToAction(nameof(List));
        }

        return View(new BlogListViewModel
        {
            CurrentCategory = currentCategory,
            CurrentCategoryId = categoryId,
            CategoryNodes = await _categoryService.GetNodes(),
            SortType = sortType,
            SortBy = sortBy,
            Posts = await _postService.GetPagedList(new PostQueryParameters
            {
                CategoryId = categoryId,
                Page = page,
                PageSize = pageSize,
                SortBy = sortType == "desc" ? $"-{sortBy}" : sortBy
            })
        });
    }

    [Route("/p/{slug}")]
    public async Task<IActionResult> PostBySlug(string slug)
    {
        var p = await _postRepo.Where(a => a.Slug == slug).FirstAsync();
        return await Post(p?.Id ?? "");
    }

    public async Task<IActionResult> Post(string id)
    {
        var post = await _postService.GetById(id);

        if (post == null)
        {
            _messages.Error($"Article {id} not exists!");
            return RedirectToAction(nameof(List));
        }

        if (!post.IsPublish)
        {
            _messages.Warning($"Article {id} not published!");
            return RedirectToAction(nameof(List));
        }

        var viewName = "Post.FrontendRender";
        if (_configService["default_render"] == "backend") viewName = "Post.BackendRender";

        return View(viewName, await _postService.GetPostViewModel(post));
    }

    public IActionResult RandomPost()
    {
        var posts = _postRepo.Where(a => a.IsPublish).ToList();
        var rndPost = posts[Random.Shared.Next(posts.Count)];
        _messages.Info($"Randomly recommended article <b>{rndPost.Title}</b> for you!" +
                       $"<span class='ps-3'><a href=\"{Url.Action(nameof(RandomPost))}\">Try again</a></span>");
        return RedirectToAction(nameof(Post), new { id = rndPost.Id });
    }

    public IActionResult Temp()
    {
        return View();
    }
}