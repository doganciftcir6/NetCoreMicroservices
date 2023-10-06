using FluentValidation.AspNetCore;
using FreeCourse.Shared.Services;
using FreeCourse.Web.Extensions;
using FreeCourse.Web.Handler;
using FreeCourse.Web.Helpers;
using FreeCourse.Web.Models;
using FreeCourse.Web.Services;
using FreeCourse.Web.Services.Interface;
using FreeCourse.Web.Validators;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FreeCourse.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //Options Pattern
            //art�k dependency injection'da IOptions interfacesini ServiceApiSettings i�in kullanabilirim.
            services.Configure<ClientSettings>(Configuration.GetSection("ClientSettings"));
            services.Configure<ServiceApiSettings>(Configuration.GetSection("ServiceApiSettings"));

            services.AddHttpContextAccessor();
            services.AddScoped<ISharedIdentityService, SharedIdentityService>();
            //IdentityModel.AspNetCore paketi i�in
            services.AddAccessTokenManagement();
            //PhotoStock Microservisinden photolar� �ekmek i�in helper
            services.AddSingleton<PhotoHelper>();

            //yazd���m�z extension metotu tan�mlayal�m, extension metottaki kodlar buraya yerle�ecek bu sayede.
            services.AddHttpClientServices(Configuration);

            services.AddScoped<ResourceOwnerPasswordTokenHandler>();
            services.AddScoped<ClientCredentialTokenHandler>();

            //Cookie Settings
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, opt =>
            {
                opt.LoginPath = "/Auth/SignIn";
                opt.ExpireTimeSpan = TimeSpan.FromDays(60);
                opt.SlidingExpiration = true;
                opt.Cookie.Name = "udemywebcookie";
            });
            //FluentValidation Register
            //RegisterValidatorsFromAssemblyContaining kullanarak sana bir validator class vereyim sen bu class�n assamblyini bul ve i�erisindeki t�m validatorleri tara diyoruz.
            services.AddControllersWithViews().AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<CourseCreateInputValidator>());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
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

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
