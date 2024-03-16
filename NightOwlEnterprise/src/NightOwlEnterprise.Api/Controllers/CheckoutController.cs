using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace NightOwlEnterprise.Api.Controllers;

[Route("[controller]")]
[ApiExplorerSettings(IgnoreApi = true)]
public class CheckoutController : Controller
{
    private readonly IOptions<StripeCredential> _stripeCredentials;

    public CheckoutController(IOptions<StripeCredential> stripeCredentials)
    {
        _stripeCredentials = stripeCredentials;
    }
    
    // GET
    public IActionResult Index()
    {
        ViewData["stripe-publishable-key"]= _stripeCredentials.Value.PublishableKey;
        return View();
    }
}