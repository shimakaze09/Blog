using Contrib.SiteMessage;
using Data.Models;
using FreeSql;
using Microsoft.AspNetCore.Mvc;
using Web.Services;
using Web.ViewModels;

namespace Web.Controllers;

public class HomeController : Controller
{
    private readonly BlogService _blogService;
    private readonly CategoryService _categoryService;
    private readonly LinkService _linkService;
    private readonly Messages _messages;
    private readonly PhotoService _photoService;

    public HomeController(BlogService blogService, PhotoService photoService, CategoryService categoryService,
        LinkService linkService,
        Messages messages)
    {
        _blogService = blogService;
        _photoService = photoService;
        _categoryService = categoryService;
        _linkService = linkService;
        _messages = messages;
    }

    public async Task<IActionResult> Index()
    {
        if (Request.QueryString.HasValue) return BadRequest();

        return View(new HomeViewModel
        {
            RandomPhoto = await _photoService.GetRandomPhoto(),
            TopPost = await _blogService.GetTopOnePost(),
            FeaturedPosts = await _blogService.GetFeaturedPosts(),
            FeaturedPhotos = await _photoService.GetFeaturedPhotos(),
            FeaturedCategories = await _categoryService.GetFeaturedCategories(),
            Links = await _linkService.GetAll()
        });
    }

    [HttpGet]
    public IActionResult Init([FromServices] ConfigService conf)
    {
        if (conf["is_init"] == "true")
        {
            _messages.Error("Initialization already completed!");
            return RedirectToAction(nameof(Index));
        }

        return View(new InitViewModel
        {
            Host = conf["host"],
            DefaultRender = conf["default_render"]
        });
    }

    [HttpPost]
    public IActionResult Init([FromServices] ConfigService conf, [FromServices] IBaseRepository<User> userRepo,
        InitViewModel vm)
    {
        if (!ModelState.IsValid) return View();

        // Save configuration
        conf["host"] = vm.Host;
        conf["default_render"] = vm.DefaultRender;
        conf["is_init"] = "true";

        // Create user
        // TODO: Temporarily storing plain text password, should be changed to MD5 encryption later
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