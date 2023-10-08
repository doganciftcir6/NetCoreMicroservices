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

//art�k tokenda bir kullan�c� bekledi�im i�in token i�erisinde ne k�t� bir payload�nda sub id bekledi�imden dolay� bununla ilgili bir Policy yaratmam laz�m, mutlaka authentication olmu� bir kullan�c� olmas� laz�m diyorum new AuthorizationPolicyBuilder().RequireAuthenticatedUser() ile ve bunu Build() ile in�a etti�imde geriye bir AuthorizationPolicy d�n�yor.
var requireAuthorizePolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

//�imdi tek tek t�m contollerlar�m�za Authraziton attributeunu ge�memek i�in bir tek burada filtre ekleyecez ama bu sefer di�er microservislerden farkl� olarak yukar�da kendi olu�turdu�um policyi verece�im ��nk� ben art�k token i�erisinde en k�t� bir sub de�eri bekliyorum user olmal� yani.
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
            //birde rabbitmqnun ui ekran�n� farkl� porttan kald�raca��z
            //yani rabbitmq kald�r�rken 2 port olacak birisi rabbitmqnun normal portu
            //diperi ise rabbitmqnun ui portu olacak defaultu 15672'dir.
            host.Username("guest");
            host.Password("guest");
        });
    });
});
//AddMassTransitHostedService koduna buraya gelecek olan bu koda art�k 8. masstransit versiyonunda gerek yok.

//sub de�erini framework otomatik olarak nameidentifier�e d�n��t�r�yor bunu iptal edelim
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");
//jwt bazl� kimlik do�rulama i�in
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
    //ortam development ise UseDeveloperExceptionPage gelsinki hatalar� g�relim orada.
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

//kimlik do�rulama i�in ekle
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
