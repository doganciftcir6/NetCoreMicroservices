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
                        //birde rabbitmqnun ui ekran�n� farkl� porttan kald�raca��z
                        //yani rabbitmq kald�r�rken 2 port olacak birisi rabbitmqnun normal portu
                        //diperi ise rabbitmqnun ui portu olacak defaultu 15672'dir.
                        host.Username("guest");
                        host.Password("guest");
                    });
                });
            });
            services.AddMassTransitHostedService();

            //jwt
            //�emalar�n ismi birden fazla jwt bekleyebiliriz. Bayiler i�in bir token birde normal kullan�c�lar i�in bir token bekliyorsun bu �yelik sistemlerini birbirinden ay�rmak i�in �ema kavram� kullan�l�r. Bizde tek bir sistem oldu�u i�in default ismi verelim.
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt =>
            {
                //kritik 2 de�er belirciem 1 bu microservise tokenin kimin da��tt���n�n bilgisi (bunu appsettingsten al ��nk� localhost var localhostu kesinlikle direkt buraya kodlar�n i�ine yazmayal�m ��nk� dockerize etti�imizde bu localhost de�i�ecek.)
                opt.Authority = Configuration["IdentityServerURL"];
                //bu uygulamama private key ile imzalanm�� bir token geldi�inde public key ile do�rulaams�n� yapacak ama public keyi nereden alacak i�te appsettingste yukar�da belirtti�imiz urlden gidecek oradaki endpointten alacak public keyi alacak gelen private key ile kar��la�t�racak e�er do�ruysa eyvallah diyecek. �kinci bir i�lem olarak benim audiece yani gelen tokenin i�erisinde mutlaka resource_catalog olmas� laz�m bunu IdentityServer micrsoervisimizin config k�sm�nda belirtmi�tik.
                //art�k gelen tokenin payload�nda audiece parametrelerine bakacak e�er resource_catalog varsa tamam istek yapabilirsin diyecek. Arkas�ndan e�er scope parametresi ile beraber claim bazl� bir do�rulama yapacaksan buradan da scope k�sm�na bakacak ama �uan bunu kullanm�yoruz.
                opt.Audience = "resource_catalog";
                //birde https kullanmakd�k o y�zden mutlaka bunu burda belirtmem laz�m default olarak https bekler.
                opt.RequireHttpsMetadata = false;
            });

            //ICategoryService ile kar��la�t���n zaman git CategoryService'ten bir nesne �rne�i getir bana DI COntainer.
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ICourseService, CourseService>();

            //automapper register
            services.AddAutoMapper(typeof(Startup));
            //t�m contollerlara authrize attributeu eklemek i�in 
            services.AddControllers(opt =>
            {
                //art�k t�m conrollerlar�m�z� koruma alt�na ald�k hepsine Authrize attributeu gelmi� oldu.
                opt.Filters.Add(new AuthorizeFilter());
            });

            //appsettingste tan�mlad���m�z DatabaseSettings datalar�n� olu�turdu�umuz classtaki proplara atarak doldural�m. OptionsPattern.
            services.Configure<DatabaseSettings>(Configuration.GetSection("DatabaseSettings"));
            //uzun uzun dependency injectionda IOptions<DatabaseSettings> _optinons  ge�mek yerine direkt IDatabaseSettings ge�elim o bize IOptions<DatabaseSetting> �rne�i versin. GetRequiredService ilgili servisi bulamazsa geriye hata f�rlat�r o y�zden bunu kullanmaya �zen g�ster.
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
