using FreeCourse.Web.Handler;
using FreeCourse.Web.Models;
using FreeCourse.Web.Services.Interface;
using FreeCourse.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.Extensions.Configuration;

namespace FreeCourse.Web.Extensions
{
    //extension olabilmesi için static olmalı
    public static class ServicesExtension
    {
        //parametreye kim için extension yazacağımı belirtiyorum,
        //this keywordu ile extension yapıyorum.
        //this IServiceCollection services ile bu interfaceyi genişlettiğimi belirtiyorum ancak bundan sonra yazılanlar mesela IConfiguration Configuration bu metotun alacağı parametrelerdir genişletme değil.
        public static void AddHttpClientServices(this IServiceCollection services, IConfiguration Configuration)
        {
            var serviceApiSettings = Configuration.GetSection("ServiceApiSettings").Get<ServiceApiSettings>();
            //HttpClient nesnesini newlememeye özen göstermeliyiz, Bu class içerisinde HttpClient nesnesi kullanmasaydım burada IdentityServiceleri AddScope yapabilirdim.
            services.AddHttpClient<IIdentityService, IdentityService>();
            //HttpCLient kullanan ClientCredentialTokenService
            services.AddHttpClient<IClientCredentialTokenService, ClientCredentialTokenService>();

            //CLİENT CREDENTİAL
            //CatalogService'in içerisinde herhangi bir HttpClient kullanıldığında yani bir yere istek atıldığında AddHttpMessageHandler<ResourceOwnerPasswordTokenHandler> ile bu handleri çalıştır diyoruz. Bu sayede yapılacak isteğin headerina clientcredentials tokeni ekliyoruz.
            services.AddHttpClient<ICatalogService, CatalogService>(opt =>
            {
                //CatalogService'te kullanmış olduğıum HttpClientın BaseUrisi bu olacak. Yani hep bu urle'e otomatik istek atacak. HttpClient nesnnesi. Bu isteği Gateway'e atarak onun üzerinden yapıcaz. Bu sayede her metotta bu pathi tekrar tekrar yazmama gerek yok.
                opt.BaseAddress = new Uri($"{serviceApiSettings.GatewayBaseUri}/{serviceApiSettings.Catalog.Path}");
            }).AddHttpMessageHandler<ClientCredentialTokenHandler>();
            services.AddHttpClient<IPhotoStockService, PhotoStockService>(opt =>
            {
                opt.BaseAddress = new Uri($"{serviceApiSettings.GatewayBaseUri}/{serviceApiSettings.PhotoStock.Path}");
            }).AddHttpMessageHandler<ClientCredentialTokenHandler>();
            //RESOURCE OWNER PASSWORD
            //UserService'in içerisinde herhangi bir HttpClient kullanıldığında yani bir yere istek atıldığında AddHttpMessageHandler<ResourceOwnerPasswordTokenHandler> ile bu handleri çalıştır diyoruz. Bu sayede yapılacak isteğin headerina resourceownerpassword tokeni ekliyoruz.
            services.AddHttpClient<IUserService, UserService>(opt =>
            {
                //UserService'te kullanmış olduğıum HttpClientın BaseUrisi bu olacak. Yani hep bu urle'e otomatik istek atacak. HttpClient nesnnesi. Bu sayede her metotta bu pathi tekrar tekrar yazmama gerek yok.
                opt.BaseAddress = new Uri(serviceApiSettings.IdentityBaseUri);
            }).AddHttpMessageHandler<ResourceOwnerPasswordTokenHandler>();
            services.AddHttpClient<IBasketService, BasketService>(opt =>
            {
                opt.BaseAddress = new Uri($"{serviceApiSettings.GatewayBaseUri}/{serviceApiSettings.Basket.Path}");
            }).AddHttpMessageHandler<ResourceOwnerPasswordTokenHandler>();
            services.AddHttpClient<IDiscountService, DiscountService>(opt =>
            {
                opt.BaseAddress = new Uri($"{serviceApiSettings.GatewayBaseUri}/{serviceApiSettings.Discount.Path}");
            }).AddHttpMessageHandler<ResourceOwnerPasswordTokenHandler>();
            services.AddHttpClient<IPaymentService, PaymentService>(opt =>
            {
                opt.BaseAddress = new Uri($"{serviceApiSettings.GatewayBaseUri}/{serviceApiSettings.Payment.Path}");
            }).AddHttpMessageHandler<ResourceOwnerPasswordTokenHandler>();
            services.AddHttpClient<IOrderService, OrderService>(opt =>
            {
                opt.BaseAddress = new Uri($"{serviceApiSettings.GatewayBaseUri}/{serviceApiSettings.Order.Path}");
            }).AddHttpMessageHandler<ResourceOwnerPasswordTokenHandler>();
        }
    }
}
