using Microsoft.AspNetCore.Mvc;

namespace Pegasos.WEB.Portal.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
