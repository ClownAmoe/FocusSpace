using Microsoft.AspNetCore.Mvc;

namespace FocusSpace.Api.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
    }
}