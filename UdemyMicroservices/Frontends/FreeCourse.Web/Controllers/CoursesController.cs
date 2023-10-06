using FreeCourse.Shared.Services;
using FreeCourse.Web.Models.Catalog;
using FreeCourse.Web.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace FreeCourse.Web.Controllers
{
    [Authorize]
    public class CoursesController : Controller
    {
        private readonly ICatalogService _catalogService;
        //token içindeki userid için
        private readonly ISharedIdentityService _sharedIdentityService;
        public CoursesController(ICatalogService catalogService, ISharedIdentityService sharedIdentityService)
        {
            _catalogService = catalogService;
            _sharedIdentityService = sharedIdentityService;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(await _catalogService.GetAllCourseByUserIdAsync(_sharedIdentityService.GetUserId));
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            //önce tüm kategorileri alıp bir selectliste çevirelim
            var categories = await _catalogService.GetAllCategoriesAsync();
            //kullanıcıya name alanı gözükecek ama ben arkada id bilgisini tutuyor olacağım
            ViewBag.categoryList = new SelectList(categories, "Id", "Name");
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CourseCreateInput courseCreateInput)
        {
            //önce tüm kategorileri alıp bir selectliste çevirelim
            var categories = await _catalogService.GetAllCategoriesAsync();
            if (!ModelState.IsValid)
            {
                //hata olursa categoryseleclistteki veriler kaybolmasın
                ViewBag.categoryList = new SelectList(categories, "Id", "Name");
                return View();
            }
            courseCreateInput.UserId = _sharedIdentityService.GetUserId;
            await _catalogService.CreateCourseAsync(courseCreateInput);
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Update(string id)
        {
            var course = await _catalogService.GetByCourseId(id);
            var categories = await _catalogService.GetAllCategoriesAsync();

            if (course == null)
            {
                //mesaj göster
                RedirectToAction(nameof(Index));
            }
            ViewBag.categoryList = new SelectList(categories, "Id", "Name", course.Id);
            CourseUpdateInput courseUpdateInput = new CourseUpdateInput
            {
                Id = course.Id,
                Name = course.Name,
                Description = course.Description,
                Price = course.Price,
                Feature = course.Feature,
                CategoryId = course.CategoryId,
                UserId = course.UserId,
                Picture = course.Picture,
            };
            return View(courseUpdateInput);
        }
        [HttpPost]
        public async Task<IActionResult> Update(CourseUpdateInput courseUpdateInput)
        {
            var categories = await _catalogService.GetAllCategoriesAsync();
            ViewBag.categoryList = new SelectList(categories, "Id", "Name", courseUpdateInput.Id);
            if (!ModelState.IsValid)
            {
                return View();
            }
            await _catalogService.UpdateCourseAsync(courseUpdateInput);
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            await _catalogService.DeleteCourseAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
