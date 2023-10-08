using FluentValidation.AspNetCore;
using FreeCourse.Shared.Services;
using FreeCourse.Web.Extensions;
using FreeCourse.Web.Handler;
using FreeCourse.Web.Helpers;
using FreeCourse.Web.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//FluentValidation Register
//RegisterValidatorsFromAssemblyContaining kullanarak sana bir validator class vereyim sen bu class�n assamblyini bul ve i�erisindeki t�m validatorleri tara diyoruz. Fluent validationun g�ncel versiyonunda bu kod geldi.
builder.Services.AddFluentValidationAutoValidation();

//Options Pattern
//art�k dependency injection'da IOptions interfacesini ServiceApiSettings i�in kullanabilirim.
builder.Services.Configure<ClientSettings>(builder.Configuration.GetSection("ClientSettings"));
builder.Services.Configure<ServiceApiSettings>(builder.Configuration.GetSection("ServiceApiSettings"));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ISharedIdentityService, SharedIdentityService>();
//IdentityModel.AspNetCore paketi i�in
builder.Services.AddAccessTokenManagement();
//PhotoStock Microservisinden photolar� �ekmek i�in helper
builder.Services.AddSingleton<PhotoHelper>();

//yazd���m�z extension metotu tan�mlayal�m, extension metottaki kodlar buraya yerle�ecek bu sayede.
builder.Services.AddHttpClientServices(builder.Configuration);

builder.Services.AddScoped<ResourceOwnerPasswordTokenHandler>();
builder.Services.AddScoped<ClientCredentialTokenHandler>();

//Cookie Settings
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, opt =>
{
    opt.LoginPath = "/Auth/SignIn";
    opt.ExpireTimeSpan = TimeSpan.FromDays(60);
    opt.SlidingExpiration = true;
    opt.Cookie.Name = "udemywebcookie";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //devel modda hata ile ilgili detayl� error sayfas�n� burada g�r�yorum
    app.UseDeveloperExceptionPage();
}
else
{
    //uygulama development mod d���nda �al��t���nda herhangi bir hata oldu�unda bu url'E y�nlendiriliyor.
    //yani uygulama canl�ya al�nd���nda bu middleware �al��acak.
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

//kimlik do�rulama i�in
app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();







//using Microsoft.AspNetCore.Hosting;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace FreeCourse.Web
//{
//    public class Program
//    {
//        public static void Main(string[] args)
//        {
//            CreateHostBuilder(args).Build().Run();
//        }

//        public static IHostBuilder CreateHostBuilder(string[] args) =>
//            Host.CreateDefaultBuilder(args)
//                .ConfigureWebHostDefaults(webBuilder =>
//                {
//                    webBuilder.UseStartup<Startup>();
//                });
//    }
//}
