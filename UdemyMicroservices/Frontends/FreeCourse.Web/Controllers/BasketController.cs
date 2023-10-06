using FreeCourse.Web.Models.Basket;
using FreeCourse.Web.Models.Discount;
using FreeCourse.Web.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace FreeCourse.Web.Controllers
{
    [Authorize]
    public class BasketController : Controller
    {
        //sepete kurs ekleyeceğimden dolayı ICatalogService
        private readonly ICatalogService _catalogService;
        private readonly IBasketService _basketService;
        public BasketController(ICatalogService catalogService, IBasketService basketService)
        {
            _catalogService = catalogService;
            _basketService = basketService;
        }

        public async Task<IActionResult> Index()
        {
            //kullanıcının sepetini oluşturacağız burada
            return View(await _basketService.GetAsync());
        }
        public async Task<IActionResult> AddBasketItem(string courseId)
        {
            //baskete item eklemek
            //önce kursu al
            var course = await _catalogService.GetByCourseId(courseId);
            //bu kurstan bir BasketItem oluştur
            var basketItem = new BasketItemViewModel { CourseId = course.Id, CourseName = course.Name, Price = course.Price };
            await _basketService.AddBasketItemAsync(basketItem);
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> DeleteBasketItem(string courseId)
        {
            //basket itemi basketten silelim
            await _basketService.RemoveBasketItemAsync(courseId);
            return RedirectToAction(nameof(Index));
        }
        
        public async Task<IActionResult> ApplyDiscount(DiscountApplyInput discountApplyInput)
        {
            if (!ModelState.IsValid)
            {
                //indexe yönlendirdiğim için hatayı index sayfasında göstermek adına tempdata
                TempData["discountError"] = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).First();
                return RedirectToAction(nameof(Index));
            }
            var discountStatus = await _basketService.ApplyDiscountAsync(discountApplyInput.Code);
            //sepet sayfasında indirim kodunu girdikten sonra uygula dediğinde kullanıcıyı
            //tekrar index sayfasına yönlendiricem dolayısıyla buradaki discountStatus
            //bilgisini Index action metotuna taşımam lazım o yüzden TempData kullanıyoruz.
            TempData["discountStatus"] = discountStatus;
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> CancelApplyDiscount()
        {
            await _basketService.CancelApplyDiscountAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
