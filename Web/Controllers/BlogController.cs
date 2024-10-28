using FreeSql;
using Microsoft.AspNetCore.Mvc;
using Data.Models;
using Web.ViewModels;
using Web.Services;
using X.PagedList;
using X.PagedList.Extensions;
using Contrib.SiteMessage;

namespace Web.Controllers;

public class BlogController : Controller
{
    private readonly Messages _messages;
    private readonly IBaseRepository<Post> _postRepo;
    private readonly IBaseRepository<Category> _categoryRepo;
    private readonly PostService _postService;

    public BlogController(IBaseRepository<Post> postRepo, IBaseRepository<Category> categoryRepo, PostService postService,
        Messages messages)
    {
        _postRepo = postRepo;
        _categoryRepo = categoryRepo;
        _postService = postService;
        _messages = messages;
    }

    public IActionResult List(int categoryId = 0, int page = 1, int pageSize = 5)
    {
        var categories = _categoryRepo.Select.IncludeMany(a => a.Posts).ToList();
        categories.Insert(0, new Category { Id = 0, Name = "All", Posts = _postRepo.Select.ToList() });
        List<Post> posts;
        if (categoryId == 0)
        {
            posts = _postRepo.Select
                .OrderByDescending(a => a.LastModifiedTime)
                .Include(a => a.Category)
                .ToList();
        }
        else
        {
            posts = _postRepo.Where(a => a.CategoryId == categoryId)
                .OrderByDescending(a => a.LastModifiedTime)
                .Include(a => a.Category)
                .ToList();
        }

        return View(new BlogListViewModel
        {
            CurrentCategory = categoryId == 0 ? categories[0] : categories.First(a => a.Id == categoryId),
            CurrentCategoryId = categoryId,
            Categories = categories,
            Posts = posts.ToPagedList(page, pageSize)
        });
    }

    public IActionResult Post(string id)
    {
        return View(_postService.GetPostViewModel(_postRepo.Where(a => a.Id == id)
            .Include(a => a.Category)
            .First()));
    }

    public IActionResult RandomPost()
    {
        var posts = _postRepo.Select.ToList();
        var rndPost = posts[new Random().Next(posts.Count)];
        _messages.Info($"Randomly recommended article <b>{rndPost.Title}</b> for you!");
        return RedirectToAction(nameof(Data.Models.Post), new { id = rndPost.Id });
    }

}
