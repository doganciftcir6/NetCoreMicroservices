using FreeCourse.Services.Catalog.Services;
using FreeCourse.Services.Catalog.Settings;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
using System.Linq;
using System.Threading.Tasks;

namespace FreeCourse.Services.Catalog
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
            //MassTransit, RabbitMQ
            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(Configuration["RabbitMQUrl"], "/", host =>
                    {
                        //rabbitmqnun default username ve passwordu guest
                        //default portu ise 5672'dir.
                        //birde rabbitmqnun ui ekranýný farklý porttan kaldýracaðýz
                        //yani rabbitmq kaldýrýrken 2 port olacak birisi rabbitmqnun normal portu
                        //diperi ise rabbitmqnun ui portu olacak defaultu 15672'dir.
                        host.Username("guest");
                        host.Password("guest");
                    });
                });
            });
            services.AddMassTransitHostedService();

            //jwt
            //þemalarýn ismi birden fazla jwt bekleyebiliriz. Bayiler için bir token birde normal kullanýcýlar için bir token bekliyorsun bu üyelik sistemlerini birbirinden ayýrmak için þema kavramý kullanýlýr. Bizde tek bir sistem olduðu için default ismi verelim.
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt =>
            {
                //kritik 2 deðer belirciem 1 bu microservise tokenin kimin daðýttýðýnýn bilgisi (bunu appsettingsten al çünkü localhost var localhostu kesinlikle direkt buraya kodlarýn içine yazmayalým çünkü dockerize ettiðimizde bu localhost deðiþecek.)
                opt.Authority = Configuration["IdentityServerURL"];
                //bu uygulamama private key ile imzalanmýþ bir token geldiðinde public key ile doðrulaamsýný yapacak ama public keyi nereden alacak iþte appsettingste yukarýda belirttiðimiz urlden gidecek oradaki endpointten alacak public keyi alacak gelen private key ile karþýlaþtýracak eðer doðruysa eyvallah diyecek. Ýkinci bir iþlem olarak benim audiece yani gelen tokenin içerisinde mutlaka resource_catalog olmasý lazým bunu IdentityServer micrsoervisimizin config kýsmýnda belirtmiþtik.
                //artýk gelen tokenin payloadýnda audiece parametrelerine bakacak eðer resource_catalog varsa tamam istek yapabilirsin diyecek. Arkasýndan eðer scope parametresi ile beraber claim bazlý bir doðrulama yapacaksan buradan da scope kýsmýna bakacak ama þuan bunu kullanmýyoruz.
                opt.Audience = "resource_catalog";
                //birde https kullanmakdýk o yüzden mutlaka bunu burda belirtmem lazým default olarak https bekler.
                opt.RequireHttpsMetadata = false;
            });

            //ICategoryService ile karþýlaþtýðýn zaman git CategoryService'ten bir nesne örneði getir bana DI COntainer.
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ICourseService, CourseService>();

            //automapper register
            services.AddAutoMapper(typeof(Startup));
            //tüm contollerlara authrize attributeu eklemek için 
            services.AddControllers(opt =>
            {
                //artýk tüm conrollerlarýmýzý koruma altýna aldýk hepsine Authrize attributeu gelmiþ oldu.
                opt.Filters.Add(new AuthorizeFilter());
            });

            //appsettingste tanýmladýðýmýz DatabaseSettings datalarýný oluþturduðumuz classtaki proplara atarak dolduralým. OptionsPattern.
            services.Configure<DatabaseSettings>(Configuration.GetSection("DatabaseSettings"));
            //uzun uzun dependency injectionda IOptions<DatabaseSettings> _optinons  geçmek yerine direkt IDatabaseSettings geçelim o bize IOptions<DatabaseSetting> örneði versin. GetRequiredService ilgili servisi bulamazsa geriye hata fýrlatýr o yüzden bunu kullanmaya özen göster.
            services.AddSingleton<IDatabaseSettings>(opt =>
            {
                return opt.GetRequiredService<IOptions<DatabaseSettings>>().Value;
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "FreeCourse.Services.Catalog", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FreeCourse.Services.Catalog v1"));
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
