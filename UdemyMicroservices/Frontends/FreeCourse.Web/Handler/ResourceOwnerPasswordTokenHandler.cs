using FreeCourse.Web.Exceptions;
using FreeCourse.Web.Services.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace FreeCourse.Web.Handler
{
    public class ResourceOwnerPasswordTokenHandler : DelegatingHandler
    {
        //Cookieden token okumak için
        private readonly IHttpContextAccessor _httpContextAccessor;
        //rereshtokeni elde etmek için
        private readonly IIdentityService _identityService;
        //loglama yapalım
        private readonly ILogger<ResourceOwnerPasswordTokenHandler> _logger;
        public ResourceOwnerPasswordTokenHandler(IHttpContextAccessor httpContextAccessor, IIdentityService identityService, ILogger<ResourceOwnerPasswordTokenHandler> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _identityService = identityService;
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //her bir istek başlatıldığında SendAsync() metotu araya girecek ve çalışacak.
            //önce accesstokeni alalım
            var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
            //bu accessTokeni requestin headerine ekleyelim.
            //bu singlepage bir uygulama olsaydı interceptor'lar var onlarla birlikte her bir requestte aynı bu mantıkla araya giriyoruz aynı mantığı uyguluyoruz.
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            //isteği artık gönderebilirim ama isteğin sonucunu takip edeceğim
            var response = await base.SendAsync(request, cancellationToken);
            if(response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                //refresyoken ile beraber yeni bir accesstoken alacağım.
                var tokenResponse = await _identityService.GetAccessTokenByRefreshToken();
                if(tokenResponse != null)
                {
                    //yeni accesstokeni requestin headerina ekle
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);
                    //isteği artık gönderebilirim ama isteğin sonucunu takip edeceğim
                    response = await base.SendAsync(request, cancellationToken);
                }
            }
            if(response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                //refreshtoken geçersizmiş bu durumda hata fırlatıp kullanıcı logine ekranına gönder
                throw new UnAuthorizeException();

            }
            return response;
        }
    }
}
