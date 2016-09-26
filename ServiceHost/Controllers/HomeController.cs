using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ServiceHost.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/
        [OutputCache(Duration = 60 * 60 * 4/*4 hours*/, VaryByParam = "none")]
        public ActionResult Index()
        {
            return View();
        }

        [OutputCache(Duration = 60 * 60 * 4/*4 hours*/, VaryByParam = "name")]
        public ActionResult Method()
        {
            return View();
        }
    }
}
