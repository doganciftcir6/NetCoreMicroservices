using FreeCourse.IdentityServer.Dtos;
using FreeCourse.IdentityServer.Models;
using FreeCourse.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Linq;
using System.Threading.Tasks;
using static IdentityServer4.IdentityServerConstants;

namespace FreeCourse.IdentityServer.Controllers
{
    //buradaki policyname aslında accesstoken olacak.
    //Aslında claim bazlı bir rolleme var gibi oluyor. Role değil tokena bakıyor.
    [Authorize(LocalApi.PolicyName)]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        //kullanıcıyı kaydedebilmem için usermanager gerekiyor.
        private readonly UserManager<ApplicationUser> _userManager;
        public UserController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(SignupDto signupDto)
        {
            //bu işi automapper ile yapabilirdik ama tek bir alan için gerek yok
            var user = new ApplicationUser()
            {
                UserName = signupDto.UserName,
                Email = signupDto.Email,
                City = signupDto.City,
            };
            //oluşacak olan kullanıcı bilgilerini ilk parametrede veriyorum ikinci parametrede ise passwordu veriyorum bu password otomatik olaarak hashlenerek database'e kaydedilecek.
            var result = await _userManager.CreateAsync(user, signupDto.Password);
            if (!result.Succeeded)
            {
                //hata varsa hataların sadece descriptionlarını bir liste olarak dönelim responseta.
                return BadRequest(Response<Shared.Dtos.NoContent>.Fail(result.Errors.Select(x => x.Description).ToList(), 400));
            }

            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> GetUser()
        {
            //bu endpointte parametre almama gerek yok çünkü zaten token gelecek token içinde gelen sub bilgisini alıcaz. Onun içinde userid bilgisi var zaten.
            //tokendan payloaddaki datalar bana bir claim nesnesi olarak gelir. Claim nesnesi key value şeklind olur Type Value şeklinde tutulur Type'ı key gibi düşünebiliriz. Datalar value içinde. Yani buradaki Type değeri tokenin içinde payloadda bulunan sub valuesi ise : den sonra gelen userid bilgisi oluyor.
            var userIdClaim = User.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub);
            //eğer yok ise
            if (userIdClaim == null)
            {
                return BadRequest();
            }

            var user = await _userManager.FindByIdAsync(userIdClaim.Value);
            if (user == null)
            {
                return BadRequest();
            }
            //ben bana gelen user datasındaki tüm alanları istemiyorum bana sadece bazı alanlar gelsin. Dto oluşturulabilir.
            return Ok(new { Id = user.Id, UserName = user.UserName, Email = user.Email, City = user.City });
        }
    }
}
