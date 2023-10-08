using FreeCourse.Gateway.DelegateHandlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Values;

var builder = WebApplication.CreateBuilder(args);

//ocelot'a oluþturduðumuz json dosyalarý haberdar edelim
//dosyanýn baþýndaki configuration verelim sonra deðiþken olan enviroment'a göre deðiþecek. sonra uzantý
builder.Configuration.AddJsonFile($"configuration.{builder.Environment.EnvironmentName.ToLower()}.json");

//TokenExchange için delege içinde httpclient kullandýk
builder.Services.AddHttpClient<TokenExchangeDelegateHandler>();

//Auth sistemi kuralým jwt için
//bu sefer GatewayAuthenticationSchema diye bir þema belirtiyorum 
//bu þemayý configuration dosyasýnda hangi route moduna eklersem artýk o bir token ile korunuyor olacak.
builder.Services.AddAuthentication().AddJwtBearer("GatewayAuthenticationSchema", opt =>
{
    opt.Authority = builder.Configuration["IdentityServerURL"];
    opt.Audience = "resource_gateway";
    opt.RequireHttpsMetadata = false;
});
//Ocelotu servis olarak ekle
//TokenExchange için .AddDelegatingHandler<TokenExchangeDelegateHandler>() ekliyoruz
//bu delege FakePayment ve Discount'a istek yapýldýðýnda çalýþmasý lazým bunu configuration.development.json dosyasýnda belirteceðiz
builder.Services.AddOcelot().AddDelegatingHandler<TokenExchangeDelegateHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.


app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    //ortam development ise UseDeveloperExceptionPage gelsinki hatalarý görelim orada.
    app.UseDeveloperExceptionPage();
}

app.MapControllers();

//ocelotu middleware olarak ekle buraya middlewarelarýmýzý ekleriz
await app.UseOcelot();

app.Run();





//using Microsoft.AspNetCore.Hosting;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace FreeCourse.Gateway
//{
//    public class Program
//    {
//        public static void Main(string[] args)
//        {
//            CreateHostBuilder(args).Build().Run();
//        }

//        public static IHostBuilder CreateHostBuilder(string[] args) =>
//            Host.CreateDefaultBuilder(args).ConfigureAppConfiguration((hostingContext, config) =>
//            {
//                //ocelot'a oluþturduðumuz json dosyalarý haberdar edelim
//                //dosyanýn baþýndaki configuration verelim sonra deðiþken olan enviroment'a göre deðiþecek. sonra uzantý
//                config.AddJsonFile($"configuration.{hostingContext.HostingEnvironment.EnvironmentName.ToLower()}.json").AddEnvironmentVariables();
//            })
//                .ConfigureWebHostDefaults(webBuilder =>
//                {
//                    webBuilder.UseStartup<Startup>();
//                });
//    }
//}
