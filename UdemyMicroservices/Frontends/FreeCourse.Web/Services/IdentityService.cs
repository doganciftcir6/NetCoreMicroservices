using FreeCourse.Shared.Dtos;
using FreeCourse.Web.Models;
using FreeCourse.Web.Services.Interface;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace FreeCourse.Web.Services
{
    public class IdentityService : IIdentityService
    {
        //IdentityServer yani bir microservice istek atacağımdan dolayı httpclient gerek
        private readonly HttpClient _httpClient;
        //httpcontextaccessor ile Cookie'ye erişebiliyor olacağım
        private readonly IHttpContextAccessor _contextAccessor;
        //Identityserverin baseurli için ServiceApiSettings'e, clientid clientsecret için ClientSettings gerek.
        private readonly ClientSettings _clientSettings;
        private readonly ServiceApiSettings _serviceApiSettings;
        public IdentityService(HttpClient httpClient, IHttpContextAccessor contextAccessor, IOptions<ClientSettings> clientSettings, IOptions<ServiceApiSettings> serviceApiSettings)
        {
            _httpClient = httpClient;
            _contextAccessor = contextAccessor;
            _clientSettings = clientSettings.Value;
            _serviceApiSettings = serviceApiSettings.Value;
        }

        public async Task<TokenResponse> GetAccessTokenByRefreshToken()
        {
            //burada refreshtoken ile tokenalam endpointine istek atıp yeni bir accesstoken ve refrestoken alıp bu bigileri mevcut cookiedeki bilgilerle değiştirip yeni bir cookie oluşturup bu cookieyi kullanıcının tarayıcısına tanımlaylalım.
            var disco = await _httpClient.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = _serviceApiSettings.IdentityBaseUri,
                Policy = new DiscoveryPolicy { RequireHttps = false }
            });
            //artık disco değişkenimin içinde tüm IdentityServer endpointlerim var.
            if (disco.IsError)
            {
                throw disco.Exception;
            }
            //artık refleshtoken ile beraber yeni bir accesstoken alacağım, önce cookieden refreshtokeni al
            var refreshToken = await _contextAccessor.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken);
            //refreshtokeni aldık şimdi refreshtokenrequest oluşturalım
            //TokenEndpoint yani token alma endpointinde aynı zamanda refreshtoken gönderip yeni bir accesstoken alabiliyorsun.
            RefreshTokenRequest refreshTokenRequest = new RefreshTokenRequest()
            {
                ClientId = _clientSettings.WebClientForUser.ClientId,
                ClientSecret = _clientSettings.WebClientForUser.ClientSecret,
                RefreshToken = refreshToken,
                Address = disco.TokenEndpoint
            };
            //refreshtoken ile accesstoken alma isteğini gerçekleştirelim
            var token = await _httpClient.RequestRefreshTokenAsync(refreshTokenRequest);
            if (token.IsError)
            {
                //loglama istersek burada yapabiliriz
                return null;
            }
            //tokenlerin hepsini bir değişkende tutalım, bunlar yeni verilerim set edeceğim veriler.
            var authenticationTokens = new List<AuthenticationToken>
            {
                new AuthenticationToken { Name= OpenIdConnectParameterNames.AccessToken, Value = token.AccessToken},
                new AuthenticationToken { Name= OpenIdConnectParameterNames.RefreshToken, Value = token.RefreshToken},
                //Tokenin ömrünü belirtiyorum herhangi bir culture bilgisine bağlı olmadan yazıyoruz
                new AuthenticationToken { Name= OpenIdConnectParameterNames.ExpiresIn, Value = DateTime.Now.AddSeconds(token.ExpiresIn).ToString("o",CultureInfo.InvariantCulture)},
            };
            //elimde zaten bir cookie var sadece oradaki değerleri set edeceğim
            //önce elimdeki authentication propertylerini alayım
            var authenticationResult = await _contextAccessor.HttpContext.AuthenticateAsync();
            var properties = authenticationResult.Properties;
            //cookemin propertilerinin içerisine yeni verili tokenlerimi kaydettim.
            properties.StoreTokens(authenticationTokens);
            //artık Cookie oluşturabilirim., Cookie içerisinde zaten kullanıcı hakkında claimler vardı tekrar uğraşmaya gerek yok.
            await _contextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, authenticationResult.Principal, properties);
            //Cokkieyi güncelleyip tekrar oluşturmuş olduk geriye tokeni dönebiliriz.
            return token;
        }

        public async Task RevokeRefreshToken()
        {
            //burada refreshtokeni sileceğiz.
            var disco = await _httpClient.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = _serviceApiSettings.IdentityBaseUri,
                Policy = new DiscoveryPolicy { RequireHttps = false }
            });
            //artık disco değişkenimin içinde tüm IdentityServer endpointlerim var.
            if (disco.IsError)
            {
                throw disco.Exception;
            }
            //accesstokeni revoke edemeyiz o yüzden reflestokeni Cookiden alalım onu revoke edeceğiz
            var refreshToken = await _contextAccessor.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken);
            //şimdi tokenrevocationrequest oluşturacağız
            TokenRevocationRequest tokenRevocationRequest = new()
            {
                ClientId = _clientSettings.WebClientForUser.ClientId,
                ClientSecret = _clientSettings.WebClientForUser.ClientSecret,
                Address = disco.RevocationEndpoint,
                Token = refreshToken,
                TokenTypeHint = "refresh_token",
            };
            //artık isteği gerçekleştir
            await _httpClient.RevokeTokenAsync(tokenRevocationRequest);
        }

        public async Task<Response<bool>> SignIn(SignInInput signInInput)
        {
            //token alma endpointine gitmem lazım ilk olarak
            //öncelikle IdentityServerda Discovery endpointi vardı bu tüm endpointleri bize listeliyordu biz GetDiscoveryDocumentAsync() ile bu endpointe istek yapacağız ve istek yaparken IdentityServerimizin ayağa kalktığı base url'i vereceğiz ve Policy ksımında ise https kullanmadığımı belirteceğim.
            var disco = await _httpClient.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = _serviceApiSettings.IdentityBaseUri,
                Policy = new DiscoveryPolicy { RequireHttps = false }
            });
            //artık disco değişkenimin içinde tüm IdentityServer endpointlerim var.
            if (disco.IsError)
            {
                throw disco.Exception;
            }
            //disco bulundu hatam yok o zaman resource owner password akış tipi oluşturalım Bunun kısaltması Password olarak geçer. PasswordTokenRequest bizim loginli token alırken gönderdiğimiz body parametrelerine sahip bir sınıf. Bilgiler belli zaten onları veriyorum sadece Address kısmına hangi IdentityServer endpointine istek yapacağını belirtiyorum.
            var passwordTokenRequest = new PasswordTokenRequest
            {
                ClientId = _clientSettings.WebClientForUser.ClientId,
                ClientSecret = _clientSettings.WebClientForUser.ClientSecret,
                UserName = signInInput.Email,
                Password = signInInput.Password,
                Address = disco.TokenEndpoint,
            };
            //RequestPasswordTokenAsync() metotu bir istek gerçekleştiriyor. Ve parametre olarak resource owner password istiyor. Yukarıda bunu oluşturmuştuk verelim. Bu istek sonucunda bana bir token gelecek.
            var token = await _httpClient.RequestPasswordTokenAsync(passwordTokenRequest);
            if (token.IsError)
            {
                //hata varsa demekki ya username ya password yanlış IdentityServerda oluşturduğumuz hata mesajlarını getirmek istiyoruz. Tokenin içeriğini okuyalım.
                var responseContent = await token.HttpResponse.Content.ReadAsStringAsync();
                //string olan verilerimde  List<string> olan Errorlarımı ErrorDto sınıfına dönüştüreceğim, PropertyNameCaseInsensitive true diyerek küçük büyük harfe dikkat etme property isimlerinde diyorum.
                var errorDto = JsonSerializer.Deserialize<ErrorDto>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return Response<bool>.Fail(errorDto.Errors, 400);
            }

            //artık token elimde hiç bir hata yok, token içindeki userin bilgilerini alacağım
            //elimdeki accesstokeni vereceğim UserInfoRequest sınıfına ve address yani IdentityServerdaki UserInfo enpointine istek yapacak.
            var userInfoRequest = new UserInfoRequest
            {
                Token = token.AccessToken,
                Address = disco.UserInfoEndpoint
            };
            //arkasından isteği yapalım geriye user bilgileri dönecek
            var userInfo = await _httpClient.GetUserInfoAsync(userInfoRequest);
            if (userInfo.IsError)
            {
                throw userInfo.Exception;
                //buralarda loglamamızı yapabiliriz yapacaksak.
            }
            //artık elime user bilgileri var ve token bilgisi var bunları Cookie içerisine gömeceğim.
            //Cookie oluşturacağım bu Cookie birer Claim nesnelerinden meydana gelecek yani key value şeklindeki datalardan meydana gelecek.
            //usernamesini name claiminden roleünü ise role claiminden alacağını söyleyelim. Bunlar UserInfo endpointinden dönen responsetaki dataların keyleri oluyor. Bu sayede ben uygulamamda HttpContext.User.Identity.Name dediğimde buradaki name bilgisini alacak.
            //Cookie oluşurken hangi kimlikle oluşacak.
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(userInfo.Claims, CookieAuthenticationDefaults.AuthenticationScheme, "name", "role");
            //şimdi bir oluşacak Cookienin temelini belirleyelim
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            //Cookie artık benim bu vermiş olduğum claim nesneleri üzerinden oluşacak.
            //Şimdi accesstoken ve reflestokenide cookiede tutalım
            var authenticationProperties = new AuthenticationProperties();
            //StoreTokens() metotuna tokenleri göndericez o bizim için cookiede bu tokenları tutacak
            authenticationProperties.StoreTokens(new List<AuthenticationToken>
            {
                new AuthenticationToken { Name= OpenIdConnectParameterNames.AccessToken, Value = token.AccessToken},
                new AuthenticationToken { Name= OpenIdConnectParameterNames.RefreshToken, Value = token.RefreshToken},
                //Tokenin ömrünü belirtiyorum herhangi bir culture bilgisine bağlı olmadan yazıyoruz
                new AuthenticationToken { Name= OpenIdConnectParameterNames.ExpiresIn, Value = DateTime.Now.AddSeconds(token.ExpiresIn).ToString("o",CultureInfo.InvariantCulture)},
            });
            //artık tokenlarıda cookie içerisine koydum, beni hatırla durumunu test edelim, IsPersistent yani Session bazlı bir Cookie mi oluşturucaz yok bir ömrü olacak mı bu Cookienin, signInInput.IsRemember true ise kalıcı olacak yani 60 günse 60 gün kalıcı olarak cookie duracak diyoruz.
            authenticationProperties.IsPersistent = signInInput.IsRemember;
            //artık giriş işlemini gerçekleştirebilirim, verdiğim şema claim ve token bilgileriyle beraber signIn metotu kullanıcıyı login yapıyor ve kullanıcı için bir token oluşturuyor.
            await _contextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, authenticationProperties);
            return Response<bool>.Success(200);
        }
    }
}
