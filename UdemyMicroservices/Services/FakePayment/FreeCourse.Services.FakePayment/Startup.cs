using MassTransit;
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

namespace FreeCourse.Services.FakePayment
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

            //art�k tokenda bir kullan�c� bekledi�im i�in token i�erisinde ne k�t� bir payload�nda sub id bekledi�imden dolay� bununla ilgili bir Policy yaratmam laz�m, mutlaka authentication olmu� bir kullan�c� olmas� laz�m diyorum new AuthorizationPolicyBuilder().RequireAuthenticatedUser() ile ve bunu Build() ile in�a etti�imde geriye bir AuthorizationPolicy d�n�yor.
            var requireAuthorizePolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            //sub de�erini framework otomatik olarak nameidentifier�e d�n��t�r�yor bunu iptal edelim
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");
            //jwt bazl� kimlik do�rulama i�in
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt =>
            {
                opt.Authority = Configuration["IdentityServerURL"];
                opt.Audience = "resource_payment";
                opt.RequireHttpsMetadata = false;
            });

            //�imdi tek tek t�m contollerlar�m�za Authraziton attributeunu ge�memek i�in bir tek burada filtre ekleyecez ama bu sefer di�er microservislerden farkl� olarak yukar�da kendi olu�turdu�um policyi verece�im ��nk� ben art�k token i�erisinde en k�t� bir sub de�eri bekliyorum user olmal� yani.
            services.AddControllers(opt =>
            {
                opt.Filters.Add(new AuthorizeFilter(requireAuthorizePolicy));
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "FreeCourse.Services.FakePayment", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FreeCourse.Services.FakePayment v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            //kimlik do�rulama i�in ekle
            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
