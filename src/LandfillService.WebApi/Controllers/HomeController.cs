using System.Web.Mvc;

namespace LandfillService.WebApi.Controllers
{
    /// <summary>
    /// Service documentation controller
    /// </summary>
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }
    }
}
