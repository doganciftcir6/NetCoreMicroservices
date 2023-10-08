using FreeCourse.Gateway.DelegateHandlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Values;

var builder = WebApplication.CreateBuilder(args);

//ocelot'a olu�turdu�umuz json dosyalar� haberdar edelim
//dosyan�n ba��ndaki configuration verelim sonra de�i�ken olan enviroment'a g�re de�i�ecek. sonra uzant�
builder.Configuration.AddJsonFile($"configuration.{builder.Environment.EnvironmentName.ToLower()}.json");

//TokenExchange i�in delege i�inde httpclient kulland�k
builder.Services.AddHttpClient<TokenExchangeDelegateHandler>();

//Auth sistemi kural�m jwt i�in
//bu sefer GatewayAuthenticationSchema diye bir �ema belirtiyorum 
//bu �emay� configuration dosyas�nda hangi route moduna eklersem art�k o bir token ile korunuyor olacak.
builder.Services.AddAuthentication().AddJwtBearer("GatewayAuthenticationSchema", opt =>
{
    opt.Authority = builder.Configuration["IdentityServerURL"];
    opt.Audience = "resource_gateway";
    opt.RequireHttpsMetadata = false;
});
//Ocelotu servis olarak ekle
//TokenExchange i�in .AddDelegatingHandler<TokenExchangeDelegateHandler>() ekliyoruz
//bu delege FakePayment ve Discount'a istek yap�ld���nda �al��mas� laz�m bunu configuration.development.json dosyas�nda belirtece�iz
builder.Services.AddOcelot().AddDelegatingHandler<TokenExchangeDelegateHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.


app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    //ortam development ise UseDeveloperExceptionPage gelsinki hatalar� g�relim orada.
    app.UseDeveloperExceptionPage();
}

app.MapControllers();

//ocelotu middleware olarak ekle buraya middlewarelar�m�z� ekleriz
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
//                //ocelot'a olu�turdu�umuz json dosyalar� haberdar edelim
//                //dosyan�n ba��ndaki configuration verelim sonra de�i�ken olan enviroment'a g�re de�i�ecek. sonra uzant�
//                config.AddJsonFile($"configuration.{hostingContext.HostingEnvironment.EnvironmentName.ToLower()}.json").AddEnvironmentVariables();
//            })
//                .ConfigureWebHostDefaults(webBuilder =>
//                {
//                    webBuilder.UseStartup<Startup>();
//                });
//    }
//}
