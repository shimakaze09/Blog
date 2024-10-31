using Contrib.SiteMessage;
using Microsoft.AspNetCore.Mvc;
using Web.Services;
using Web.ViewModels;

namespace Web.Controllers;

public class PhotographyController : Controller
{
    private readonly Messages _messages;
    private readonly PhotoService _photoService;

    public PhotographyController(PhotoService photoService, Messages messages)
    {
        _photoService = photoService;
        _messages = messages;
    }

    public IActionResult Index(int page = 1, int pageSize = 10)
    {
        return View(new PhotographyViewModel
        {
            Photos = _photoService.GetPagedList(page, pageSize)
        });
    }

    public IActionResult Photo(string id)
    {
        return View(_photoService.GetById(id));
    }

    public IActionResult RandomPhoto()
    {
        var item = _photoService.GetRandomPhoto();
        _messages.Info($"Randomly recommended a photo <b>{item.Title}</b> to you~");
        return RedirectToAction(nameof(Photo), new { id = item.Id });
    }
}