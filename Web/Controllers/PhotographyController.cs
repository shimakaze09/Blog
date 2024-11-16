using Contrib.SiteMessage;
using Microsoft.AspNetCore.Mvc;
using Web.Services;
using Web.ViewModels.Photography;

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

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
    {
        return View(new PhotographyViewModel
        {
            Photos = await _photoService.GetPagedList(page, pageSize)
        });
    }

    public async Task<IActionResult> Photo(string id)
    {
        var photo = await _photoService.GetById(id);
        if (photo == null)
        {
            _messages.Error($"Photo {id} does not exist!");
            return RedirectToAction(nameof(Index));
        }

        return View(photo);
    }

    public async Task<IActionResult> Next(string id)
    {
        var item = await _photoService.GetNext(id);
        if (item == null)
        {
            _messages.Warning("No next photo available~");
            return RedirectToAction(nameof(Photo), new { id });
        }

        return RedirectToAction(nameof(Photo), new { id = item.Id });
    }

    public async Task<IActionResult> Previous(string id)
    {
        var item = await _photoService.GetPrevious(id);
        if (item == null)
        {
            _messages.Warning("No previous photo available~");
            return RedirectToAction(nameof(Photo), new { id });
        }

        return RedirectToAction(nameof(Photo), new { id = item.Id });
    }

    public async Task<IActionResult> RandomPhoto()
    {
        var item = await _photoService.GetRandomPhoto();
        if (item == null)
        {
            _messages.Error("No photos available, please upload some first!");
            return RedirectToAction("Index", "Home");
        }

        _messages.Info($"Randomly recommended the photo <b>{item.Title}</b> to you!" +
                       $"<spanclass='ps-3'><a href=\"{Url.Action(nameof(RandomPhoto))}\">Try again</a></span>");
        return RedirectToAction(nameof(Photo), new { id = item.Id });
    }
}