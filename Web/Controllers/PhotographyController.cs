using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

public class PhotographyController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}