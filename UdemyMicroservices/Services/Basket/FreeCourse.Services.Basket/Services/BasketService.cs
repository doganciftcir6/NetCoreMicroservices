using FreeCourse.Services.Basket.Dtos;
using FreeCourse.Shared.Dtos;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace FreeCourse.Services.Basket.Services
{
    public class BasketService : IBasketService
    {
        //redisservice ile iletişime geç çünkü redisle bağlantı kurmak lazım
        private readonly RedisService _redisService;
        public BasketService(RedisService redisService)
        {
            _redisService = redisService;
        }

        public async Task<Response<bool>> Delete(string userId)
        {
            //silme işlemini userId keyine göre yapalım kullanıcının sepetini silmesi için
            //silme başırıylıysa true değilse false döner
            var status = await _redisService.GetDb().KeyDeleteAsync(userId);
            return status ? Response<bool>.Success(204) : Response<bool>.Fail("Basket not found", 404);
        }

        public async Task<Response<BasketDto>> GetBasket(string userId)
        {
            //userId keye sahip bir data var mı varsa bana ver diyorum.
            var existBasket = await _redisService.GetDb().StringGetAsync(userId);
            if (String.IsNullOrEmpty(existBasket))
            {
                //bu userıdye sahip bir sepet yok.
                return Response<BasketDto>.Fail("Basket not found", 404);
            }
            //bana bir RedisValue geliyor onu BasketDto türüne deserialize edelim.
            return Response<BasketDto>.Success(JsonSerializer.Deserialize<BasketDto>(existBasket), 200);
        }

        public async Task<Response<bool>> SaveOrUpdate(BasketDto basketDto)
        {
            //keyimin userId olduğunu söylüyorum çünkü bu kullanıcının sepetini ekleyecek veya güncelleyecek ve Dtoyu string bir veriye dönüştürüyoru mserialize ederek.
            //bu ture ya da false döner true ise ya kayıt ya update yapmış false ise hiç bir şey yapamamış.
            var status = await _redisService.GetDb().StringSetAsync(basketDto.UserId, JsonSerializer.Serialize(basketDto));
            return status ? Response<bool>.Success(204) : Response<bool>.Fail("Basket could not update or save", 500);
        }
    }
}
