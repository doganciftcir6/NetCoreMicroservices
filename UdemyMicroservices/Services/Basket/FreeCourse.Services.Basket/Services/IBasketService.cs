using FreeCourse.Services.Basket.Dtos;
using FreeCourse.Shared.Dtos;
using System.Threading.Tasks;

namespace FreeCourse.Services.Basket.Services
{
    public interface IBasketService
    {
        //kullanıcının sepetini döndüren metot
        Task<Response<BasketDto>> GetBasket(string userId);
        //insert update aynı anda yapacak eğer yoksa insert varsa update yapacak
        Task<Response<bool>> SaveOrUpdate(BasketDto basketDto);
        //kullanıcının sepetini silen metot
        Task<Response<bool>> Delete(string userId);
    }
}
