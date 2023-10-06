using FreeCourse.Web.Models.Order;
using FreeCourse.Web.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace FreeCourse.Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        //geçmiş siparişleri göstermek için
        private readonly IBasketService _basketService;
        public OrderController(IOrderService orderService, IBasketService basketService)
        {
            _orderService = orderService;
            _basketService = basketService;
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var basket = await _basketService.GetAsync();
            //geçmiş siparişleri viewbag ile göndereceğiz
            ViewBag.basket = basket;
            return View(new CheckoutInfoInput());
        }
        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutInfoInput checkoutInfoInput)
        {
            //senkron iletişim için
            //var orderStatus = await _orderService.CreateOrderAsync(checkoutInfoInput);
            //asenkron iletişim için
            var orderStatus = await _orderService.SuspendOrder(checkoutInfoInput);
            if (!orderStatus.IsSuccessful)
            {
                //TempData deme sebebimiz çünkü get Checkout action metoduna yönlendirme yapıcaz hatayı
                //TempData["error"] = orderStatus.Error;
                //return RedirectToAction(nameof(Checkout));
                //veya Viewbag ile direkt buradan da view'a gönderebiliriz
                ViewBag.error = orderStatus.Error;
                return View();
            }
            //senkron iletişim için
            //return RedirectToAction(nameof(SuccessfulCheckout), new { orderId = orderStatus.OrderId });
            //asenkron iletişim için
            return RedirectToAction(nameof(SuccessfulCheckout), new { orderId = new Random().Next(1, 1000) });
        }

        public IActionResult SuccessfulCheckout(int orderId)
        {
            ViewBag.orderId = orderId;
            return View();
        }

        public async Task<IActionResult> CheckoutHistory()
        {
            return View(await _orderService.GetOrder());
        }
    }
}
