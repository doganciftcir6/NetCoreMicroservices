using FreeCourse.Services.Order.Application.Consumer;
using FreeCourse.Services.Order.Infrastructure;
using FreeCourse.Shared.Services;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
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

namespace FreeCourse.Services.Order.API
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
            //MassTransite CreateOrderMessageCommandConsumer'ý haberdar edelim
            services.AddMassTransit(x =>
            {
                //cunsomer ekle
                x.AddConsumer<CreateOrderMessageCommandConsumer>();
                x.AddConsumer<CourseNameChangedEventConsumer>();
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
                    //kuyruðu alýcý endpoint olarak belirle
                    //böyle bir kuyruðu okumak istiyorum diyoruz
                    //cunsomer hangi endpointi kuyruðu okuyacak
                    cfg.ReceiveEndpoint("create-order-service", e =>
                    {
                        //okuma iþlemini gerçekleþtir
                        e.ConfigureConsumer<CreateOrderMessageCommandConsumer>(context);
                    });
                    //event için
                    //catalogmicrosevisimiz mesageyi exchange'e gönderiyordu bizim exchangedeki messageyi alabilmemiz için kendimizin bir kuyruk oluþturup ilgili exchange'e maplememiz lazým.
                    cfg.ReceiveEndpoint("course-name-changed-event-order-service", e =>
                    {
                        //okuma iþlemini gerçekleþtir
                        e.ConfigureConsumer<CourseNameChangedEventConsumer>(context);
                    });
                });
            });
            services.AddMassTransitHostedService();

            //artýk tokenda bir kullanýcý beklediðim için token içerisinde ne kötü bir payloadýnda sub id beklediðimden dolayý bununla ilgili bir Policy yaratmam lazým, mutlaka authentication olmuþ bir kullanýcý olmasý lazým diyorum new AuthorizationPolicyBuilder().RequireAuthenticatedUser() ile ve bunu Build() ile inþa ettiðimde geriye bir AuthorizationPolicy dönüyor.
            var requireAuthorizePolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            //sub deðerini framework otomatik olarak nameidentifier’e dönüþtürüyor bunu iptal edelim
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");
            //jwt bazlý kimlik doðrulama için
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt =>
            {
                opt.Authority = Configuration["IdentityServerURL"];
                opt.Audience = "resource_order";
                opt.RequireHttpsMetadata = false;
            });

            //migration db
            services.AddDbContext<OrderDbContext>(opt =>
            {
                opt.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"), configure =>
                {
                    //migrationlarýmýn FreeCourse.Services.Order.Infrastructure'da oluþmasýný istiyorum
                    configure.MigrationsAssembly("FreeCourse.Services.Order.Infrastructure");
                });
            });

            //ISharedIdentityService'i kaydedelim token içindeki userýd bilgisini alabilmek için
            services.AddHttpContextAccessor();
            services.AddScoped<ISharedIdentityService, SharedIdentityService>();
            //mediatR kaydet, handlelarým nerede onu belirtmem lazým assambly olarak
            //FreeCourse.Services.Order.Application.Handlers.CreateOrderCommandHandler tipini alalým
            //sonrasýnda ise .Assembly diyerek assemblysinialabilirim yani bu tipin baðlý olduðu assemblyin ismini almýþ olduk.
            services.AddMediatR(typeof(Application.Handlers.CreateOrderCommandHandler).Assembly);

            //þimdi tek tek tüm contollerlarýmýza Authraziton attributeunu geçmemek için bir tek burada filtre ekleyecez ama bu sefer diðer microservislerden farklý olarak yukarýda kendi oluþturduðum policyi vereceðim çünkü ben artýk token içerisinde en kötü bir sub deðeri bekliyorum user olmalý yani.
            services.AddControllers(opt =>
            {
                opt.Filters.Add(new AuthorizeFilter(requireAuthorizePolicy));
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "FreeCourse.Services.Order.API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FreeCourse.Services.Order.API v1"));
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
