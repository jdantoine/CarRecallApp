using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebAPI.Controllers
{
    public class WebApiController : Controller
    {
        // GET: WebApi
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Bob()
        {
            return View();
        }
    }
}