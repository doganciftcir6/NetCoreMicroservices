using FreeCourse.Shared.Dtos;
using FreeCourse.Web.Models.Basket;
using FreeCourse.Web.Services.Interface;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace FreeCourse.Web.Services
{
    public class BasketService : IBasketService
    {
        private readonly HttpClient _httpClient;
        private readonly IDiscountService _discountService;
        public BasketService(HttpClient httpClient, IDiscountService discountService)
        {
            _httpClient = httpClient;
            _discountService = discountService;
        }

        public async Task AddBasketItemAsync(BasketItemViewModel basketItemViewModel)
        {
            //sepeti önce bir alalım
            var basket = await GetAsync();
            if (basket != null)
            {
                //eklemek istenilen item daha önce var mı yok mu onu kontrol edelim
                if (!basket.BasketItems.Any(x => x.CourseId == basketItemViewModel.CourseId))
                {
                    //istenilen itemi sepete ekle
                    basket.BasketItems.Add(basketItemViewModel);
                }
            }
            else
            {
                //sepeti oluşturayım
                basket = new BasketViewModel();
                basket.BasketItems.Add(basketItemViewModel);
            }
            //sepeti son güncel haliyle güncelleyeyim
            await SaveOrUpdateAsync(basket);
        }

        public async Task<bool> ApplyDiscountAsync(string discountCode)
        {
            //burası DiscountApi microservis sonrası kodlanacak.
            //daha önce sepette indirim uygulandıysa onu iptal etmeliyiz
            await CancelApplyDiscountAsync();
            //sepeti al
            var basket = await GetAsync();
            if (basket == null)
            {
                return false;
            }
            //önce bir indirim alalım indirim var mı bu kullanıcı hakkında
            var hasDiscount = await _discountService.GetDiscountAsync(discountCode);
            if (hasDiscount == null)
            {
                return false;
            }
            //basket.DiscountRate = hasDiscount.Rate;
            //basket.DiscountCode = hasDiscount.Code;
            basket.ApplyDiscount(hasDiscount.Code, hasDiscount.Rate);
            //sepeti güncelle (indirim kodunu ve oranını sepetin içinde tutuyoruz)
            await SaveOrUpdateAsync(basket);
            return true;
        }

        public async Task<bool> CancelApplyDiscountAsync()
        {
            //burası DiscountApi microservis sonrası kodlanacak.
            //basketi al
            var basket = await GetAsync();
            if (basket == null || basket.DiscountCode == null)
            {
                return false;
            }
            //kullanıcının o anda girmiş olduğu kodu ve uygulanan ratei null'a çek
            //basket.DiscountRate = null;
            //basket.DiscountCode = null;
            basket.CancelDiscount();
            //sonra basketi tekrar güncelle
            await SaveOrUpdateAsync(basket);
            return true;
        }

        public async Task<bool> DeleteAsync()
        {
            var result = await _httpClient.DeleteAsync("baskets");
            return result.IsSuccessStatusCode;
        }

        public async Task<BasketViewModel> GetAsync()
        {
            var response = await _httpClient.GetAsync("baskets");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            //içindeki datayı oku ve gelen datayı BasketViewModel'a deserilize et.
            var basketViewModel = await response.Content.ReadFromJsonAsync<Response<BasketViewModel>>();
            return basketViewModel.Data;
        }

        public async Task<bool> RemoveBasketItemAsync(string courseId)
        {
            //direkt yukarıdaki get metotu ile basketi al ve içinden item silicez.
            var basket = await GetAsync();
            if (basket == null)
            {
                return false;
            }
            //basketin içerisinden ilgili kursu bulduk
            var deleteBasketItem = basket.BasketItems.FirstOrDefault(x => x.CourseId == courseId);
            if (deleteBasketItem == null)
            {
                return false;
            }
            //bu kursu basketitemlardan silelim
            var deleteItemResult = basket.BasketItems.Remove(deleteBasketItem);
            if (!deleteItemResult)
            {
                return false;
            }
            if (!basket.BasketItems.Any())
            {
                //basketin sepetin içi boşsa basketıtem hiç yoksa indirim koduunuda null'a çekelim
                //sepette hiç bir şey yok indirim uygulanmasın
                basket.DiscountCode = null;
            }
            //get ile almış olduğum basketin içinde gerekli değişikliği yaptıktan sonra tekrar update ettim.
            return await SaveOrUpdateAsync(basket);
        }

        public async Task<bool> SaveOrUpdateAsync(BasketViewModel basketViewModel)
        {
            //gönderdiğim datayı direkt olarak serlize yapsın jsona ve apiye json bilgi göndersin.
            //PostAsJsonAsync ile
            var response = await _httpClient.PostAsJsonAsync<BasketViewModel>("baskets", basketViewModel);
            return response.IsSuccessStatusCode;
        }
    }
}
