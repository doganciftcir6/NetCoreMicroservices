using FreeCourse.Web.Models.PhotoStocks;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace FreeCourse.Web.Services.Interface
{
    public interface IPhotoStockService
    {
        Task<PhotoStockViewModel> UploadPhoto(IFormFile photo);
        Task<bool> DeletePhoto(string photoUrl);
    }
}
