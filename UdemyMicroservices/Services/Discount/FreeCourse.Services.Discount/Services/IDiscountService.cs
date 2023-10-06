using FreeCourse.Shared.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FreeCourse.Services.Discount.Services
{
    public interface IDiscountService
    {
        //dto oluşturup maplemeyle uğraşmayalım ama
        //normalde olması gereken dto oluşturmak ve onu geriye dönmek
        //kurs uzamasın diye dto oluşturmadık.
        Task<Response<List<Models.Discount>>> GetAllAsync();
        Task<Response<Models.Discount>> GetByIdAsync(int id);
        Task<Response<NoContent>> SaveAsync(Models.Discount discount);
        Task<Response<NoContent>> UpdateAsync(Models.Discount discount);
        Task<Response<NoContent>> DeleteAsync(int id);
        //userid ile beraber indirim kodu göndereyim bu indirim koduna ait bir kullanıcı var mı yok mu
        Task<Response<Models.Discount>> GetByCodeAndUserIdAsync(string code, string userId);
    }
}
