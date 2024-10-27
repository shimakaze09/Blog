using FreeSql;
using Microsoft.AspNetCore.Mvc;
using Data.Models;
using Web.ViewModels;

namespace Web.Controllers;

public class BlogController : Controller
{
    private readonly IBaseRepository<Post> _postRepo;
    private readonly IBaseRepository<Category> _categoryRepo;
    public BlogController(IBaseRepository<Post> postRepo, IBaseRepository<Category> categoryRepo)
    {
        _postRepo = postRepo;
        _categoryRepo = categoryRepo;
    }

    public IActionResult List()
    {
        return View(new BlogList
        {
            Posts = _postRepo.Select.ToList()
        });
    }
}