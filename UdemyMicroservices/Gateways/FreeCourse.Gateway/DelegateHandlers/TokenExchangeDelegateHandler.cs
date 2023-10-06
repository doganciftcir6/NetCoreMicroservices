using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FreeCourse.Gateway.DelegateHandlers
{
    //burası isteğin arasına girip eski tokenı IdentityServera gönderip Discount ve FakePayment izinlerine sahip yeni tokeni IdentityServerdan alacak ve isteğin headerina ekleyecek
    public class TokenExchangeDelegateHandler : DelegatingHandler
    {
        //IdentityServer'a istek yapabilmek için
        private readonly HttpClient _httpClient;
        //appsettingsteki ClientId VE ClientSecret okumak için
        private readonly IConfiguration _configuration;
        //accesstokeni alalım
        private string _accessToken;
        public TokenExchangeDelegateHandler(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        //önce eski token ile yeni tokeni IdentityServerdan alalım
        private async Task<string> GetToken(string requestToken)
        {
            if (!string.IsNullOrEmpty(_accessToken))
            {
                //demekki accesstokeni doldurmuşuz
                return _accessToken;
            }
            //discoya bağlan
            //IdentityModel kütüphanesi gerekiyor.
            var disco = await _httpClient.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = _configuration["IdentityServerURL"],
                Policy = new DiscoveryPolicy { RequireHttps = false }
            });
            //artık disco değişkenimin içinde tüm IdentityServer endpointlerim var.
            if (disco.IsError)
            {
                throw disco.Exception;
            }

            //tokenExchangeTokenRequest oluşturalım
            TokenExchangeTokenRequest tokenExchangeTokenRequest = new TokenExchangeTokenRequest()
            {
                Address = disco.TokenEndpoint,
                ClientId = _configuration["ClientId"],
                ClientSecret = _configuration["ClientSecret"],
                GrantType = _configuration["TokenGrantType"],
                //eski tokeni ve tokenin tipinin access olduğunu belirt
                SubjectToken = requestToken,
                SubjectTokenType = "urn:ietf:params:oauth:token-type:access-token",
                //yeni token hangi microservislere istek yapacak belirt, Burada openid olmak zorunda çünkü payloadda sub kısmında kullanıcının idsini tutuyoruz
                Scope = "openid discount_fullpermission payment_fullpermission"
            };
            //isteği at
            var tokenResponse = await _httpClient.RequestTokenExchangeTokenAsync(tokenExchangeTokenRequest);
            if (tokenResponse.IsError)
            {
                throw tokenResponse.Exception;
            }
            //değişkenin içine yeni tokeni at
            _accessToken = tokenResponse.AccessToken;
            return _accessToken;
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //müdahaleyi burada yapacağım isteğin headerine yeni tokeni ekleyelim
            //önce mevcut tokeni al
            var requestToken = request.Headers.Authorization.Parameter;
            //bu eski tokendan Discount ve FakePayment izinlerine sahip yeni tokeni al
            var newToken = await GetToken(requestToken);
            //yeni tokeni isteğe ekle
            request.SetBearerToken(newToken);
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
