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
        var photo = _photoService.GetById(id);
        if (photo == null)
        {
            _messages.Error($"Photo {id} not exists.");
            return RedirectToAction(nameof(Index));
        }

        return View(photo);
    }

    public IActionResult RandomPhoto()
    {
        var item = _photoService.GetRandomPhoto();
        _messages.Info($"Randomly recommended a photo <b>{item.Title}</b> to you~");
        return RedirectToAction(nameof(Photo), new { id = item.Id });
    }
}