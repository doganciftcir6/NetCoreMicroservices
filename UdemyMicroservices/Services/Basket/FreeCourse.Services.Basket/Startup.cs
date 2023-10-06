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
            //art�k tokenda bir kullan�c� bekledi�im i�in token i�erisinde ne k�t� bir payload�nda sub id bekledi�imden dolay� bununla ilgili bir Policy yaratmam laz�m, mutlaka authentication olmu� bir kullan�c� olmas� laz�m diyorum new AuthorizationPolicyBuilder().RequireAuthenticatedUser() ile ve bunu Build() ile in�a etti�imde geriye bir AuthorizationPolicy d�n�yor.
            var requireAuthorizePolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            //sub de�erini framework otomatik olarak nameidentifier�e d�n��t�r�yor bunu iptal edelim
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");
            //jwt bazl� kimlik do�rulama i�in
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt =>
            {
                opt.Authority = Configuration["IdentityServerURL"];
                opt.Audience = "resource_basket";
                opt.RequireHttpsMetadata = false;
            });

            //IHttpContextAccessor kullanabilmem i�in
            services.AddHttpContextAccessor();
            //Shareddaki SharedIdentityService'i kullanbilmem i�in DI containera ekle ve olu�turdu�umuz serviceyi
            services.AddScoped<ISharedIdentityService, SharedIdentityService>();
            services.AddScoped<IBasketService, BasketService>();

            //Options Pattern, IOptions<RedisSettings> �zerinden direkt olarak bu s�n�ftaki dolu de�erlere ula�abilirim art�k bu sayede.
            services.Configure<RedisSettings>(Configuration.GetSection("RedisSettings"));

            //Redis ba�lant�s� bunu DI containera servis olarak eklicez ama singleton olarak eklicez bir kere aya�a kalks�n ba�lant� kurulsun ve arkas�ndan o ba�lant� �zerinden devam edeyim i�lemlere.
            //ba�lant� kurulduktan sonra geriye bir RediceService d�nemek istiyorum.
            services.AddSingleton<RedisService>(sp =>
            {
                var redisSettings = sp.GetRequiredService<IOptions<RedisSettings>>().Value;
                var redis = new RedisService(redisSettings.Host, redisSettings.Port);
                //ba�lant�y� kur, gerekli ortam� OptionsPattern �zerinden appsettings de�erlerini okuyarak host ve port verdik.
                redis.Connect();
                return redis;
            });

            //�imdi tek tek t�m contollerlar�m�za Authraziton attributeunu ge�memek i�in bir tek burada filtre ekleyecez ama bu sefer di�er microservislerden farkl� olarak yukar�da kendi olu�turdu�um policyi verece�im ��nk� ben art�k token i�erisinde en k�t� bir sub de�eri bekliyorum user olmal� yani.
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
            
            //kimlik do�rulama i�in ekle
            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
