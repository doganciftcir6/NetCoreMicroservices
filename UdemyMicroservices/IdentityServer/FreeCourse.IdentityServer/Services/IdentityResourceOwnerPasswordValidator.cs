using FreeCourse.IdentityServer.Models;
using IdentityModel;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FreeCourse.IdentityServer.Services
{
    public class IdentityResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        //pasword ve email doğrulama için usermanager gerekli
        private readonly UserManager<ApplicationUser> _userManager;

        public IdentityResourceOwnerPasswordValidator(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            //önce dbde böyle bir emaile sahip kullanıcı var mı kontrol
            //ben bu context.UserName içerisinde emai göndericem username'i kullanmıcam bu isteğe bağlı ama genelde email tercih edilir
            //bu context içindeki UserName ve Password kullanıcının token almak için gönderdiği bilgiler oluyor.
            var existUser = await _userManager.FindByEmailAsync(context.UserName);
            if (existUser == null)
            {
                //böyle bir email yok, IdentityServerin döneceği bir response zaten var ama ben bu response'a birde ek yapmak istiyorum.
                var errors = new Dictionary<string, object>();
                errors.Add("errors", new List<string> { "Email veya şifreniz yanlış" });
                context.Result.CustomResponse = errors;
                //return ile beraber geriye dönüyorum bakın hiç bir şey dönmüyorum
                return;
            }
            //şimdi birde passwordunu kontrol edelim
            var passwordCheck = await _userManager.CheckPasswordAsync(existUser, context.Password);
            if (passwordCheck == false)
            {
                //password yanlış, IdentityServerin döneceği bir response zaten var ama ben bu response'a birde ek yapmak istiyorum.
                var errors = new Dictionary<string, object>();
                errors.Add("errors", new List<string> { "Email veya şifreniz yanlış" });
                context.Result.CustomResponse = errors;
                //return ile beraber geriye dönüyorum bakın hiç bir şey dönmüyorum
                return;
            }
            //veritabanında böyle bir email var ve şifreside doğru
            //constextin resultunu doldruucaz. İçine Bu logim olan kullanıcının id bilgisini koyabiliriz. Sonrasında ise buradaki akış tipini belirtelim biizim akış tipimiz şuan ResourceOwnerCrenditialstı bunun kısaltılması Password olarak geçer.
            context.Result = new GrantValidationResult(existUser.Id.ToString(), OidcConstants.AuthenticationMethods.Password);
        }
    }
}
