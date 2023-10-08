using FreeCourse.Web.Models.Discount;
using System.Threading.Tasks;

namespace FreeCourse.Web.Services.Interface
{
    public interface IDiscountService
    {
        Task<DiscountViewModel> GetDiscountAsync(string discountCode);
    } 
}
