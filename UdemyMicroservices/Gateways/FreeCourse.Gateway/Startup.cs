using FreeCourse.Gateway.DelegateHandlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FreeCourse.Gateway
{
    public class Startup
    {
        //Configuration eklemem lazým appsettings.json içindeki bilgiyi okuyabilmek için
        private readonly IConfiguration Configuration;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //TokenExchange için delege içinde httpclient kullandýk
            services.AddHttpClient<TokenExchangeDelegateHandler>();

            //Auth sistemi kuralým jwt için
            //bu sefer GatewayAuthenticationSchema diye bir þema belirtiyorum 
            //bu þemayý configuration dosyasýnda hangi route moduna eklersem artýk o bir token ile korunuyor olacak.
            services.AddAuthentication().AddJwtBearer("GatewayAuthenticationSchema", opt =>
            {
                opt.Authority = Configuration["IdentityServerURL"];
                opt.Audience = "resource_gateway";
                opt.RequireHttpsMetadata = false;
            });
            //Ocelotu servis olarak ekle
            //TokenExchange için .AddDelegatingHandler<TokenExchangeDelegateHandler>() ekliyoruz
            //bu delege FakePayment ve Discount'a istek yapýldýðýnda çalýþmasý lazým bunu configuration.development.json dosyasýnda belirteceðiz
            services.AddOcelot().AddDelegatingHandler<TokenExchangeDelegateHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //ocelotu middleware olarak ekle buraya middlewarelarýmýzý ekleriz
            await app.UseOcelot();
        }
    }
}
