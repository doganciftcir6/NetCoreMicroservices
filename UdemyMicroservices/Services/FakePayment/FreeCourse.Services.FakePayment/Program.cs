using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//artýk tokenda bir kullanýcý beklediðim için token içerisinde ne kötü bir payloadýnda sub id beklediðimden dolayý bununla ilgili bir Policy yaratmam lazým, mutlaka authentication olmuþ bir kullanýcý olmasý lazým diyorum new AuthorizationPolicyBuilder().RequireAuthenticatedUser() ile ve bunu Build() ile inþa ettiðimde geriye bir AuthorizationPolicy dönüyor.
var requireAuthorizePolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

//þimdi tek tek tüm contollerlarýmýza Authraziton attributeunu geçmemek için bir tek burada filtre ekleyecez ama bu sefer diðer microservislerden farklý olarak yukarýda kendi oluþturduðum policyi vereceðim çünkü ben artýk token içerisinde en kötü bir sub deðeri bekliyorum user olmalý yani.
builder.Services.AddControllers(opt =>
{
    opt.Filters.Add(new AuthorizeFilter(requireAuthorizePolicy));
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//MassTransit, RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQUrl"], "/", host =>
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
//AddMassTransitHostedService koduna buraya gelecek olan bu koda artýk 8. masstransit versiyonunda gerek yok.

//sub deðerini framework otomatik olarak nameidentifier’e dönüþtürüyor bunu iptal edelim
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");
//jwt bazlý kimlik doðrulama için
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt =>
{
    opt.Authority = builder.Configuration["IdentityServerURL"];
    opt.Audience = "resource_payment";
    opt.RequireHttpsMetadata = false;
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //ortam development ise UseDeveloperExceptionPage gelsinki hatalarý görelim orada.
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

//kimlik doðrulama için ekle
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();


//using Microsoft.AspNetCore.Hosting;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace FreeCourse.Services.FakePayment
//{
//    public class Program
//    {
//        public static void Main(string[] args)
//        {
//            CreateHostBuilder(args).Build().Run();
//        }

//        public static IHostBuilder CreateHostBuilder(string[] args) =>
//            Host.CreateDefaultBuilder(args)
//                .ConfigureWebHostDefaults(webBuilder =>
//                {
//                    webBuilder.UseStartup<Startup>();
//                });
//    }
//}
