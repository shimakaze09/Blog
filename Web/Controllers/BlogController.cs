using Contrib.SiteMessage;
using Data.Models;
using FreeSql;
using Microsoft.AspNetCore.Mvc;
using Web.Services;
using Web.ViewModels.Blog;
using Web.ViewModels.QueryFilters;

namespace Web.Controllers;

public class BlogController : Controller
{
    private readonly IBaseRepository<Category> _categoryRepo;
    private readonly CategoryService _categoryService;
    private readonly Messages _messages;
    private readonly IBaseRepository<Post> _postRepo;
    private readonly PostService _postService;

    public BlogController(IBaseRepository<Post> postRepo, IBaseRepository<Category> categoryRepo,
        PostService postService,
        Messages messages,
        CategoryService categoryService)
    {
        _postRepo = postRepo;
        _categoryRepo = categoryRepo;
        _postService = postService;
        _messages = messages;
        _categoryService = categoryService;
    }

    public IActionResult List(int categoryId = 0, int page = 1, int pageSize = 5)
    {
        var categories = _categoryRepo.Where(a => a.Visible)
            .IncludeMany(a => a.Posts).ToList();
        categories.Insert(0, new Category { Id = 0, Name = "All", Posts = _postRepo.Select.ToList() });

        var currentCategory = categoryId == 0 ? categories[0] : _categoryService.GetById(categoryId);

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
            CurrentCategory = categoryId == 0 ? categories[0] : categories.First(a => a.Id == categoryId),
            CurrentCategoryId = categoryId,
            Categories = categories,
            CategoryNodes = _categoryService.GetNodes(),
            Posts = _postService.GetPagedList(new PostQueryParameters
            {
                CategoryId = categoryId,
                Page = page,
                PageSize = pageSize,
                OnlyPublished = true
            })
        });
    }

    public IActionResult Post(string id)
    {
        var post = _postService.GetById(id);

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

        return View(_postService.GetPostViewModel(post));
    }

    public IActionResult RandomPost()
    {
        var posts = _postRepo.Where(a => a.IsPublish).ToList();
        var rndPost = posts[new Random().Next(posts.Count)];
        _messages.Info($"Randomly recommended article <b>{rndPost.Title}</b> for you!");
        return RedirectToAction(nameof(Post), new { id = rndPost.Id });
    }

    public IActionResult Temp()
    {
        return View();
    }
}