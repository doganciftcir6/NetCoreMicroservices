using IdentityServer4.Validation;
using System.Linq;
using System.Threading.Tasks;

namespace FreeCourse.IdentityServer.Services
{
    public class TokenExchangeExtensionGrantValidator : IExtensionGrantValidator
    {
        //akış ismini verelim, OAtuth 2.0 ın isimlendirme standartını kullan
        public string GrantType => "urn:ietf:params:oauth:grant-type:token-exchange";

        //gelen tokeni doğrulama işlemi yapmak için
        private readonly ITokenValidator _tokenValidator;
        public TokenExchangeExtensionGrantValidator(ITokenValidator tokenValidator)
        {
            _tokenValidator = tokenValidator;
        }

        public async Task ValidateAsync(ExtensionGrantValidationContext context)
        {
            //önce requesti ham olarak tamamen al
            var requestRaw = context.Request.Raw.ToString();
            //request içerisinden tokeni al
            var token = context.Request.Raw.Get("subject_token");
            if (string.IsNullOrEmpty(token))
            {
                context.Result = new GrantValidationResult(IdentityServer4.Models.TokenRequestErrors.InvalidRequest, "token missing");
                return;
            }
            //tokeni doğrula, ömrünü imzasını kontrol
            var tokenValidateResult = await _tokenValidator.ValidateAccessTokenAsync(token);
            if (tokenValidateResult.IsError)
            {
                context.Result = new GrantValidationResult(IdentityServer4.Models.TokenRequestErrors.InvalidGrant, "token invalid");
                return;
            }
            //bu tokendan payloaddaki sub keywordunu kullanıcıidsini alalım
            //tokenda mutlaka sub değeri olmalı yani mutlaka kullanıcıya özgü bir token olmalı resource owner akış tipiyle alınmış olmalı
            var subjectClaim = tokenValidateResult.Claims.FirstOrDefault(c => c.Type == "sub");
            if(subjectClaim == null)
            {
                context.Result = new GrantValidationResult(IdentityServer4.Models.TokenRequestErrors.InvalidGrant, "token must contain sub value");
                return;
            }
            //geriye yeni tokeni dönebiliriz
            //kullanıcının idsini ver, accesstoken olduğunu söyle, token içindeki claimleri ekle.
            context.Result = new GrantValidationResult(subjectClaim.Value, "access_token", tokenValidateResult.Claims);
            return;
        }
    }
}
