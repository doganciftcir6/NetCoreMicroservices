using FreeCourse.Shared.Dtos;
using FreeCourse.Shared.Services;
using FreeCourse.Web.Models.FakePayment;
using FreeCourse.Web.Models.Order;
using FreeCourse.Web.Services.Interface;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace FreeCourse.Web.Services
{
    public class OrderService : IOrderService
    {
        private readonly HttpClient _httpClient;
        private readonly IPaymentService _paymentService;
        private readonly IBasketService _basketService;
        private readonly ISharedIdentityService _sharedIdentityService;
        public OrderService(HttpClient httpClient, IPaymentService paymentService, IBasketService basketService, ISharedIdentityService sharedIdentityService)
        {
            _httpClient = httpClient;
            _paymentService = paymentService;
            _basketService = basketService;
            _sharedIdentityService = sharedIdentityService;
        }

        public async Task<OrderCreatedViewModel> CreateOrderAsync(CheckoutInfoInput checkoutInfoInput)
        {
            //burada olduğu gibi senkron olarak birden fazla microservis ile haberleşiyorsak mutlaka orada Saga gibi patternları kullanmak gerekiyor. Çünkü burada Disstrubute Transaction durumu olabilir bu durumu yönetmeliyiz. Diyelim 4 microservise istek yapıyoruz veritabanlarında işlem yapabilmek için bir tanesinde problem olursa diğer 3 taneyi iptal edecek yeni istekler göndermem gerekiyor. Diyelim ödeme yapıldı ama sipariş oluşamadı orada işte retry mekanizmaların vs farklı mekanizmaların olması lazım. 
            //önce sepetteki dataları alması lazımki bunları sipariş olarak gönderelim
            var basket = await _basketService.GetAsync();
            //ödeme oluşturalım
            var paymentInfoInput = new PaymentInfoInput()
            {
                CardName = checkoutInfoInput.CardName,
                CardNumber = checkoutInfoInput.CardNumber,
                Expiration = checkoutInfoInput.Expiration,
                CVV = checkoutInfoInput.CVV,
                TotalPrice = basket.TotalPrice
            };
            //ödeme yap
            var responsePayment = await _paymentService.ReceivePaymentAsync(paymentInfoInput);
            if (!responsePayment)
            {
                return new OrderCreatedViewModel() { Error = "Ödeme alınmaadı", IsSuccessful = false };
            }
            //ödeme tamam artık siparişi oluşturabiliriz
            var orderCreateInput = new OrderCreateInput()
            {
                BuyerId = _sharedIdentityService.GetUserId,
                Address = new AddressCreateInput() { Province = checkoutInfoInput.Province, District = checkoutInfoInput.District, Street = checkoutInfoInput.Street, Line = checkoutInfoInput.Line, ZipCode = checkoutInfoInput.ZipCode },
            };
            basket.BasketItems.ForEach(basketItem =>
            {
                //courseservise bağlanılıp pictureurl alınabilir ama gerk yok boş gitsin
                var orderItem = new OrderItemCreateInput() { ProdcutId = basketItem.CourseId, Price = basketItem.GetCurrentPrice, PictureUrl = "", ProductName = basketItem.CourseName};
                //burada  orderCreateInput.OrderItems = new List<OrderItems> dememek için 
                //OrderCreateInput un ctorunda yaptık bu işi
                orderCreateInput.OrderItems.Add(orderItem);
            });
            //isteği at
            var response = await _httpClient.PostAsJsonAsync<OrderCreateInput>("orders", orderCreateInput);
            if (!response.IsSuccessStatusCode)
            {
                //gerçek hayatta burada Sipariş oluşturulamadı demeyiz bir sorun oldu deriz ve kesin bir loglama atarız ve 5 sn sonra tekrar istek atacak bir retry mekanizması yazabiliriz
                return new OrderCreatedViewModel() { Error = "Sipariş oluşturulamadı", IsSuccessful = false };
            }
            //ödeme gerçekleşti sipariş oluştu
            var orderCreatedViewModel = await response.Content.ReadFromJsonAsync<Response<OrderCreatedViewModel>>();
            orderCreatedViewModel.Data.IsSuccessful = true;
            //seepti boşaltabiliriz
            await _basketService.DeleteAsync();
            return orderCreatedViewModel.Data;
        }

        public async Task<List<OrderViewModel>> GetOrder()
        {
            //microservisten geriye response olarak döndüğü için burada da response olarak yakalamak gerekir.
            //deserilaze olabilmesi için.
            var response = await _httpClient.GetFromJsonAsync<Response<List<OrderViewModel>>>("orders");
            return response.Data;
        }
        //asenkron iletişim senaryosu için
        public async Task<OrderSuspendViewModel> SuspendOrder(CheckoutInfoInput checkoutInfoInput)
        {
            //önce sepetteki dataları alması lazımki bunları sipariş olarak gönderelim
            var basket = await _basketService.GetAsync();
            //ödeme tamam artık siparişi oluşturabiliriz
            var orderCreateInput = new OrderCreateInput()
            {
                BuyerId = _sharedIdentityService.GetUserId,
                Address = new AddressCreateInput() { Province = checkoutInfoInput.Province, District = checkoutInfoInput.District, Street = checkoutInfoInput.Street, Line = checkoutInfoInput.Line, ZipCode = checkoutInfoInput.ZipCode },
            };
            basket.BasketItems.ForEach(basketItem =>
            {
                //courseservise bağlanılıp pictureurl alınabilir ama gerk yok boş gitsin
                var orderItem = new OrderItemCreateInput() { ProdcutId = basketItem.CourseId, Price = basketItem.GetCurrentPrice, PictureUrl = "", ProductName = basketItem.CourseName };
                //burada  orderCreateInput.OrderItems = new List<OrderItems> dememek için 
                //OrderCreateInput un ctorunda yaptık bu işi
                orderCreateInput.OrderItems.Add(orderItem);
            });
  
            //ödeme oluşturalım
            var paymentInfoInput = new PaymentInfoInput()
            {
                CardName = checkoutInfoInput.CardName,
                CardNumber = checkoutInfoInput.CardNumber,
                Expiration = checkoutInfoInput.Expiration,
                CVV = checkoutInfoInput.CVV,
                TotalPrice = basket.TotalPrice,
                //asenktron iletişim için ekledik
                Order = orderCreateInput,
            };
            //ödeme yap
            var responsePayment = await _paymentService.ReceivePaymentAsync(paymentInfoInput);
            if (!responsePayment)
            {
                return new OrderSuspendViewModel() { Error = "Ödeme alınmaadı", IsSuccessful = false };
            }
            //ödeme başarılıysa sepeti boşalt
            await _basketService.DeleteAsync();
            return new OrderSuspendViewModel() { IsSuccessful = true };
        }
    }
}
