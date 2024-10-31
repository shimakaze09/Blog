using FreeSql;
using Microsoft.AspNetCore.Mvc;
using Data.Models;
using Web.Services;
using Web.ViewModels;

namespace Web.Controllers;

public class HomeController : Controller
{
    private readonly BlogService _blogService;
    private readonly PhotoService _photoService;
    private readonly CategoryService _categoryService;

    public HomeController(BlogService blogService, PhotoService photoService, CategoryService categoryService)
    {
        _blogService = blogService;
        _photoService = photoService;
        _categoryService = categoryService;
    }

    public IActionResult Index()
    {
        return View(new HomeViewModel
        {
            TopPost = _blogService.GetTopOnePost(),
            FeaturedPosts = _blogService.GetFeaturedPostRows(),
            FeaturedPhotos = _photoService.GetFeaturedPhotos(),
            FeaturedCategories = _categoryService.GetFeaturedCategories()
        });
    }
}