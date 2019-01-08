using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;

using DemoProject.Models;
using CLMS.Framework.Utilities;

namespace DemoProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public HomeController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {
            DebugHelper.Log(DebugMessageType.Error, "Index", "Test");

            return View();
        }

        public IActionResult About()
        {
            string webRootPath = _hostingEnvironment.WebRootPath;
            string contentRootPath = _hostingEnvironment.ContentRootPath;

            var errr = new ExceptionHandler();

            try
            {
                var email = Email.FetchSmtpSettings();
                throw new Exception();
            } catch (Exception ex)
            {
                ViewData["Message"] = $"{errr.HandleException(ex).OriginalExceptionMessage} webRootPath";
            }

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = $"Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
