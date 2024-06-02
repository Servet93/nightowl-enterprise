using Microsoft.AspNetCore.Mvc;

namespace NightOwlEnterprise.Api.Controllers;

[Route("[controller]")]
[ApiExplorerSettings(IgnoreApi = true)]
public class NotificationController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}