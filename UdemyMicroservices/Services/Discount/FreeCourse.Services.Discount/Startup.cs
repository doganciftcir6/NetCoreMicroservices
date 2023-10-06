using FreeCourse.Services.Discount.Services;
using FreeCourse.Shared.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace FreeCourse.Services.Discount
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
            //artýk tokenda bir kullanýcý beklediðim için token içerisinde ne kötü bir payloadýnda sub id beklediðimden dolayý bununla ilgili bir Policy yaratmam lazým, mutlaka authentication olmuþ bir kullanýcý olmasý lazým diyorum new AuthorizationPolicyBuilder().RequireAuthenticatedUser() ile ve bunu Build() ile inþa ettiðimde geriye bir AuthorizationPolicy dönüyor.
            var requireAuthorizePolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            //sub deðerini framework otomatik olarak nameidentifier’e dönüþtürüyor bunu iptal edelim
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");
            //jwt bazlý kimlik doðrulama için
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt =>
            {
                opt.Authority = Configuration["IdentityServerURL"];
                opt.Audience = "resource_discount";
                opt.RequireHttpsMetadata = false;
            });

            //ISharedIdentityService dependency injectionda kullanabilmek için
            services.AddHttpContextAccessor();
            services.AddScoped<ISharedIdentityService, SharedIdentityService>();
            //service classýmý dependency injectionda kullanabilmek için
            services.AddScoped<IDiscountService, DiscountService>();

            //þimdi tek tek tüm contollerlarýmýza Authraziton attributeunu geçmemek için bir tek burada filtre ekleyecez ama bu sefer diðer microservislerden farklý olarak yukarýda kendi oluþturduðum policyi vereceðim çünkü ben artýk token içerisinde en kötü bir sub deðeri bekliyorum user olmalý yani.
            services.AddControllers(opt =>
            {
                opt.Filters.Add(new AuthorizeFilter(requireAuthorizePolicy));
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "FreeCourse.Services.Discount", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FreeCourse.Services.Discount v1"));
            }

            app.UseRouting();

            //kimlik doðrulama için ekle
            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
