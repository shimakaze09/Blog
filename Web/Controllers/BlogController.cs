using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

public class BlogController : Controller
{
    // GET
    public IActionResult List()
    {
        return View();
    }
}