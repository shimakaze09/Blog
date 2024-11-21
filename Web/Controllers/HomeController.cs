using Data.Models;
using FreeSql;
using Microsoft.AspNetCore.Mvc;
using Share.Extensions;
using Web.Contrib.SiteMessage;
using Web.Services;
using Web.ViewModels;
using Web.Extensions;

namespace Web.Controllers;

public class HomeController : Controller
{
    private readonly BlogService _blogService;
    private readonly CategoryService _categoryService;
    private readonly LinkService _linkService;
    private readonly MessageService _messages;
    private readonly PhotoService _photoService;
    private readonly ConfigService _conf;

    public HomeController(BlogService blogService, PhotoService photoService, CategoryService categoryService,
        LinkService linkService,
        MessageService messages, ConfigService conf)
    {
        _blogService = blogService;
        _photoService = photoService;
        _categoryService = categoryService;
        _linkService = linkService;
        _messages = messages;
        _conf = conf;
    }

    public async Task<IActionResult> Index()
    {
        if (Request.QueryString.HasValue) return BadRequest();

        var vm = new HomeViewModel
        {
            ChartVisible = _conf["home_chart_visible"] == "true",
            RandomPhotoVisible = _conf["home_random_photo_visible"] == "true",
            RandomPhoto = await _photoService.GetRandomPhoto(),
            TopPost = await _blogService.GetTopOnePost(),
            FeaturedPosts = await _blogService.GetFeaturedPosts(),
            FeaturedPhotos = await _photoService.GetFeaturedPhotos(),
            FeaturedCategories = await _categoryService.GetFeaturedCategories(),
            Links = await _linkService.GetAll()
        };

        if (HttpContext.Request.IsMobileBrowser())
        {
            vm.ChartVisible = false;
            vm.RandomPhotoVisible = false;
        }

        return View(vm);
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
    public IActionResult Init([FromServices] IBaseRepository<User> userRepo, InitViewModel vm)
    {
        if (_conf["is_init"] == "true")
        {
            _messages.Error("Initialization is complete!");
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid) return View();

        // Save configuration
        _conf["host"] = vm.Host;
        _conf["default_render"] = vm.DefaultRender;
        _conf["is_init"] = "true";

        // Create user
        // TODO: Use salted hash password https://www.ais.com/how-to-generate-a-jwt-token-using-net-6/
        userRepo.Insert(new User
        {
            Id = Guid.NewGuid().ToString(),
            Name = vm.Username,
            Password = vm.Password.ToSHA256()
        });

        _messages.Success("Initialization completed!");
        return RedirectToAction(nameof(Index));
    }
}