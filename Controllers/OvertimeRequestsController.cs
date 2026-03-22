using Microsoft.AspNetCore.Mvc;

namespace HRManagement.Controllers
{
    public class OvertimeRequestsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
