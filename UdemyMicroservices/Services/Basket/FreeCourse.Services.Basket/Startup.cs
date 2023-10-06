using FreeCourse.Services.Basket.Services;
using FreeCourse.Services.Basket.Settings;
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
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace FreeCourse.Services.Basket
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
                opt.Audience = "resource_basket";
                opt.RequireHttpsMetadata = false;
            });

            //IHttpContextAccessor kullanabilmem için
            services.AddHttpContextAccessor();
            //Shareddaki SharedIdentityService'i kullanbilmem için DI containera ekle ve oluþturduðumuz serviceyi
            services.AddScoped<ISharedIdentityService, SharedIdentityService>();
            services.AddScoped<IBasketService, BasketService>();

            //Options Pattern, IOptions<RedisSettings> üzerinden direkt olarak bu sýnýftaki dolu deðerlere ulaþabilirim artýk bu sayede.
            services.Configure<RedisSettings>(Configuration.GetSection("RedisSettings"));

            //Redis baðlantýsý bunu DI containera servis olarak eklicez ama singleton olarak eklicez bir kere ayaða kalksýn baðlantý kurulsun ve arkasýndan o baðlantý üzerinden devam edeyim iþlemlere.
            //baðlantý kurulduktan sonra geriye bir RediceService dönemek istiyorum.
            services.AddSingleton<RedisService>(sp =>
            {
                var redisSettings = sp.GetRequiredService<IOptions<RedisSettings>>().Value;
                var redis = new RedisService(redisSettings.Host, redisSettings.Port);
                //baðlantýyý kur, gerekli ortamý OptionsPattern üzerinden appsettings deðerlerini okuyarak host ve port verdik.
                redis.Connect();
                return redis;
            });

            //þimdi tek tek tüm contollerlarýmýza Authraziton attributeunu geçmemek için bir tek burada filtre ekleyecez ama bu sefer diðer microservislerden farklý olarak yukarýda kendi oluþturduðum policyi vereceðim çünkü ben artýk token içerisinde en kötü bir sub deðeri bekliyorum user olmalý yani.
            services.AddControllers(opt =>
            {
                opt.Filters.Add(new AuthorizeFilter(requireAuthorizePolicy));
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "FreeCourse.Services.Basket", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FreeCourse.Services.Basket v1"));
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
