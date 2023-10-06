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
            //artýk dependency injection'da IOptions interfacesini ServiceApiSettings için kullanabilirim.
            services.Configure<ClientSettings>(Configuration.GetSection("ClientSettings"));
            services.Configure<ServiceApiSettings>(Configuration.GetSection("ServiceApiSettings"));

            services.AddHttpContextAccessor();
            services.AddScoped<ISharedIdentityService, SharedIdentityService>();
            //IdentityModel.AspNetCore paketi için
            services.AddAccessTokenManagement();
            //PhotoStock Microservisinden photolarý çekmek için helper
            services.AddSingleton<PhotoHelper>();

            //yazdýðýmýz extension metotu tanýmlayalým, extension metottaki kodlar buraya yerleþecek bu sayede.
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
            //RegisterValidatorsFromAssemblyContaining kullanarak sana bir validator class vereyim sen bu classýn assamblyini bul ve içerisindeki tüm validatorleri tara diyoruz.
            services.AddControllersWithViews().AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<CourseCreateInputValidator>());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
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

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
