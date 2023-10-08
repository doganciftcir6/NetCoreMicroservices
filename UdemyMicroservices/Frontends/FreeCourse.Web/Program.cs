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
//RegisterValidatorsFromAssemblyContaining kullanarak sana bir validator class vereyim sen bu classýn assamblyini bul ve içerisindeki tüm validatorleri tara diyoruz. Fluent validationun güncel versiyonunda bu kod geldi.
builder.Services.AddFluentValidationAutoValidation();

//Options Pattern
//artýk dependency injection'da IOptions interfacesini ServiceApiSettings için kullanabilirim.
builder.Services.Configure<ClientSettings>(builder.Configuration.GetSection("ClientSettings"));
builder.Services.Configure<ServiceApiSettings>(builder.Configuration.GetSection("ServiceApiSettings"));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ISharedIdentityService, SharedIdentityService>();
//IdentityModel.AspNetCore paketi için
builder.Services.AddAccessTokenManagement();
//PhotoStock Microservisinden photolarý çekmek için helper
builder.Services.AddSingleton<PhotoHelper>();

//yazdýðýmýz extension metotu tanýmlayalým, extension metottaki kodlar buraya yerleþecek bu sayede.
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
    //devel modda hata ile ilgili detaylý error sayfasýný burada görüyorum
    app.UseDeveloperExceptionPage();
}
else
{
    //uygulama development mod dýþýnda çalýþtýðýnda herhangi bir hata olduðunda bu url'E yönlendiriliyor.
    //yani uygulama canlýya alýndýðýnda bu middleware çalýþacak.
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

//kimlik doðrulama için
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
