using FreeCourse.Services.PhotoStock.dtos;
using FreeCourse.Shared.ControllerBases;
using FreeCourse.Shared.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FreeCourse.Services.PhotoStock.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhotosController : CustomBaseController
    {
        //CancellationToken alma amacımız buraya bir fotoğraf geldiğinde kaydetme işlemi 20 saniye sürüyor diyelim. Eğer bu enpointi çağıran yer işlemi sonlandırırsa buradaki fotoğraf kaydetme olayıda sonlansın devam etmesin o yüzden. Diyelim ki kullanıcı tarayıcıya geldi fotoğrafı seçti fotoğraf kaydoluyor arka planda 1 dakika sürdüğünü düşünelim. 30. saniyede eğerki tarayıcıyı kapatırsa CancellationToken otomatik bir şekilde tetiklenecek ve işlemi devam ettirmeyecek bu güzel bir best practice. Yoksa kapatmazsa dosyayı kaydetmeye belli bir süre kadar devam eder sistem. Böyle bir durumda CancellationToken otomatik olarak tetiklenecek ve benim async olan fotoğraf kaydetme işlemimi sonlandıracak. Async bir işlemi ancak hata fırlatarak sonlandırabiliriz. İşte CancellationToken’da hata fırlatarak işlemi sonlandırıyor.
        [HttpPost]
        public async Task<IActionResult> PhotoSave(IFormFile photo, CancellationToken cancellationToken)
        {
            if (photo != null && photo.Length > 0)
            {
                //demekki dosya var kullanıcı dosya göndermiş
                //burda dosya ismini random yapmıyoruz zaten bu endpointi kullanan microservis dosya ismini randomlamış bir şekilde gönderecek.
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/photos", photo.FileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    //eğer kullanıcı tarayıcıyı kaparsa veya istek yarıda kesilirse cancellationToken hata fırlatacak ve kopyalama işlemi gerçekleşmeyecek. 
                    await photo.CopyToAsync(stream, cancellationToken);
                }
                //fotoğraf kaydedildikten sonra nasıl  bir path döneceğim, bunu bu endpointi kullanan microservise dönücem.
                var returnPath = photo.FileName;
                //geriye bir dto dönelim.
                PhotoDto photoDto = new() { Url = returnPath };
                return CreateActionResultInstance(Response<PhotoDto>.Success(photoDto, 200));
            }
            return CreateActionResultInstance(Response<PhotoDto>.Fail("photo is empty", 400));
        }

        [HttpDelete]
        public IActionResult PhotoDeelete(string photoUrl)
        {
            //Combine metotu yan yana  vermiş olduğumuz pathleri birleştiriyor işi bu.
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/photos", photoUrl);
            if (!System.IO.File.Exists(path))
            {
                //path yok ise
                return CreateActionResultInstance(Response<NoContent>.Fail("photo not found", 404));
            }
            //dosyayı sil
            System.IO.File.Delete(path);
            return CreateActionResultInstance(Response<NoContent>.Success(204));
        }
    }
}
