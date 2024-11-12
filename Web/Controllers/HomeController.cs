using Microsoft.AspNetCore.Mvc;
using Web.Services;
using Web.ViewModels;

namespace Web.Controllers;

public class HomeController : Controller
{
    private readonly BlogService _blogService;
    private readonly CategoryService _categoryService;
    private readonly LinkService _linkService;
    private readonly PhotoService _photoService;

    public HomeController(BlogService blogService, PhotoService photoService, CategoryService categoryService,
        LinkService linkService)
    {
        _blogService = blogService;
        _photoService = photoService;
        _categoryService = categoryService;
        _linkService = linkService;
    }

    public IActionResult Index()
    {
        return View(new HomeViewModel
        {
            RandomPhoto = _photoService.GetRandomPhoto(),
            TopPost = _blogService.GetTopOnePost(),
            FeaturedPosts = _blogService.GetFeaturedPosts(),
            FeaturedPhotos = _photoService.GetFeaturedPhotos(),
            FeaturedCategories = _categoryService.GetFeaturedCategories(),
            Links = _linkService.GetAll()
        });
    }
}