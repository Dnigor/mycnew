using System.Web.Mvc;

namespace myCustomers.Web.Controllers
{
    public class ErrorController : Controller
    {
        [HttpGet]
        public ActionResult Unknown()
        {
            Response.StatusCode = 500;
            Response.TrySkipIisCustomErrors = true;
            return View();
        }

        [HttpGet]
        public ActionResult NotFound()
        {
            Response.StatusCode = 404;
            Response.TrySkipIisCustomErrors = true;
            return View();
        }
    }
}
