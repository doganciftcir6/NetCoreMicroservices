using FreeCourse.Web.Models;
using FreeCourse.Web.Services.Interface;
using IdentityModel.AspNetCore.AccessTokenManagement;
using IdentityModel.Client;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Threading.Tasks;

namespace FreeCourse.Web.Services
{
    public class ClientCredentialTokenService : IClientCredentialTokenService
    {
        private readonly ServiceApiSettings _serviceApiSettings;
        private readonly ClientSettings _clientSettings;
        private readonly IClientAccessTokenCache _clientAccessTokenCache;
        private readonly HttpClient _httpClient;
        public ClientCredentialTokenService(IOptions<ServiceApiSettings> serviceApiSettings, IOptions<ClientSettings> clientSettings, IClientAccessTokenCache clientAccessTokenCache, HttpClient httpClient)
        {
            _serviceApiSettings = serviceApiSettings.Value;
            _clientSettings = clientSettings.Value;
            _clientAccessTokenCache = clientAccessTokenCache;
            _httpClient = httpClient;
        }
        //burası IdentityServerdan ClientCredential type token alacağımız ve memory cache'e kaydedeceğimiz yar. Burada sadece AccessToken olur refreshtoken olmaz. Çünkü bu tokenda kullanıcıdan aldığımız bir veri yok. Sadece ClientId ve ClientSecret ile beraber istek yapıyor.
        public async Task<string> GetToken()
        {
            //önce cachede WebClientToken var mı kontrol edelim, bu metot IdentityModel.AspNetCore paketinden
            var currentToken = await _clientAccessTokenCache.GetAsync("WebClientToken");
            if(currentToken != null)
            {
                return currentToken.AccessToken;
            }
            //Cachede token yok önce discovery endpointine gitmem lazım
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
            //artık clientcredential type isteği hazırlayalım
            var clientCredentialTokenRequest = new ClientCredentialsTokenRequest
            {
                ClientId = _clientSettings.WebClient.ClientId,
                ClientSecret = _clientSettings.WebClient.ClientSecret,
                Address = disco.TokenEndpoint
            };
            //artık isteği atıp tokeni alabiliriz
            var newToken = await _httpClient.RequestClientCredentialsTokenAsync(clientCredentialTokenRequest);
            if (newToken.IsError)
            {
                throw newToken.Exception;
            }
            //elimde token var önce onu cache'e kaydet, bu metot IdentityModel.AspNetCore paketinden
            await _clientAccessTokenCache.SetAsync("WebClientToken", newToken.AccessToken, newToken.ExpiresIn);
            //cache'e kaydettikten sonra ccesstokeni geri dönebiliriz.
            return newToken.AccessToken;
        }
    }
}
