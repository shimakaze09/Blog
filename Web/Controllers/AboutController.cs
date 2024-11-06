using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

public class AboutController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}