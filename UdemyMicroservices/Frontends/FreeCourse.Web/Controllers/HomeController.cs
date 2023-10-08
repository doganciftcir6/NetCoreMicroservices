using FreeCourse.Web.Exceptions;
using FreeCourse.Web.Models;
using FreeCourse.Web.Services.Interface;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace FreeCourse.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICatalogService _catalogService;
        public HomeController(ILogger<HomeController> logger, ICatalogService catalogService)
        {
            _logger = logger;
            _catalogService = catalogService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _catalogService.GetAllCourseAsync());
        }
        public async Task<IActionResult> Detail(string id)
        {
            return View(await _catalogService.GetByCourseId(id));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            //burası development ortamda çalışmaz
            //uygulamada bir hata fırlatıldığında default olarak burası çalışıyor
            //fırlatılan hatayı yakala
            var errorFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
            if(errorFeature != null && errorFeature.Error is UnAuthorizeException)
            {
                //UnAuthorizeException ise o zaman bu benim fırlattığım hata olur
                //bu hata varsa direkt olarak çıkış yaptılarım zaten çıkış yapıldığında
                //otomatik login sayfasına yönleniyor
                //kullanıcı 60 gün boyunca siteye hiç girmezse burası çalışır refreshtokendan dolayı
                return RedirectToAction(nameof(AuthController.Logout), "Auth");
            }
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
