using FreeSql;
using Microsoft.AspNetCore.Mvc;
using Contrib.SiteMessage;
using Data.Models;
using Web.Services;
using Web.ViewModels;

namespace Web.Controllers;

public class HomeController : Controller
{
    private readonly BlogService _blogService;
    private readonly PhotoService _photoService;
    private readonly CategoryService _categoryService;
    private readonly LinkService _linkService;
    private readonly Messages _messages;

    public HomeController(BlogService blogService, PhotoService photoService, CategoryService categoryService,
        LinkService linkService, Messages messages)
    {
        _blogService = blogService;
        _photoService = photoService;
        _categoryService = categoryService;
        _linkService = linkService;
        _messages = messages;
    }

    public IActionResult Index()
    {
        if (Request.QueryString.HasValue)
        {
            return BadRequest();
        }

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

    [HttpGet]
    public IActionResult Init([FromServices] ConfigService conf)
    {
        if (conf["is_init"] == "true")
        {
            _messages.Error("Initialization is complete!");
            return RedirectToAction(nameof(Index));
        }

        return View(new InitViewModel
        {
            Host = conf["host"]
        });
    }

    [HttpPost]
    public IActionResult Init([FromServices] ConfigService conf, [FromServices] IBaseRepository<User> userRepo,
        InitViewModel vm)
    {
        if (!ModelState.IsValid) return View();

        // Save configuration
        conf["host"] = vm.Host;
        conf["is_init"] = "true";

        // Create user
        // TODO: Store password in plain text for now, will change to MD5 encryption later
        userRepo.Insert(new User
        {
            Id = Guid.NewGuid().ToString(),
            Name = vm.Username,
            Password = vm.Password
        });

        _messages.Success("Initialization completed!");
        return RedirectToAction(nameof(Index));
    }
}