using Microsoft.AspNetCore.Mvc;

namespace GLMS.Web_POE.Controllers
{
	public class HomeController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
