using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebAPI.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        public ActionResult CarRecall()
        {
            
            return View();
        }

        public ActionResult CarRecall2()
        {

            return View();
        }

        public ActionResult CarRecall3()
        {

            return View();
        }
    }
}
