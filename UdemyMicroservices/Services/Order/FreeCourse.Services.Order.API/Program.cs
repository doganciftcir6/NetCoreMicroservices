using FreeCourse.Services.Order.Application.Consumer;
using FreeCourse.Services.Order.Infrastructure;
using FreeCourse.Shared.Services;
using MassTransit;
using MassTransit.NewIdFormatters;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IdentityModel.Tokens.Jwt;
using static MassTransit.MessageHeaders;
using static System.Net.Mime.MediaTypeNames;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//artık tokenda bir kullanıcı beklediğim için token içerisinde ne kötü bir payloadında sub id beklediğimden dolayı bununla ilgili bir Policy yaratmam lazım, mutlaka authentication olmuş bir kullanıcı olması lazım diyorum new AuthorizationPolicyBuilder().RequireAuthenticatedUser() ile ve bunu Build() ile inşa ettiğimde geriye bir AuthorizationPolicy dönüyor.
var requireAuthorizePolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

//şimdi tek tek tüm contollerlarımıza Authraziton attributeunu geçmemek için bir tek burada filtre ekleyecez ama bu sefer diğer microservislerden farklı olarak yukarıda kendi oluşturduğum policyi vereceğim çünkü ben artık token içerisinde en kötü bir sub değeri bekliyorum user olmalı yani.
builder.Services.AddControllers(opt =>
{
    opt.Filters.Add(new AuthorizeFilter(requireAuthorizePolicy));
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//MassTransite CreateOrderMessageCommandConsumer'ı haberdar edelim
builder.Services.AddMassTransit(x =>
{
    //cunsomer ekle
    x.AddConsumer<CreateOrderMessageCommandConsumer>();
    x.AddConsumer<CourseNameChangedEventConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQUrl"], "/", host =>
        {
            //rabbitmqnun default username ve passwordu guest
            //default portu ise 5672'dir.
            //birde rabbitmqnun ui ekranını farklı porttan kaldıracağız
            //yani rabbitmq kaldırırken 2 port olacak birisi rabbitmqnun normal portu
            //diperi ise rabbitmqnun ui portu olacak defaultu 15672'dir.
            host.Username("guest");
            host.Password("guest");
        });
        //kuyruğu alıcı endpoint olarak belirle
        //böyle bir kuyruğu okumak istiyorum diyoruz
        //cunsomer hangi endpointi kuyruğu okuyacak
        cfg.ReceiveEndpoint("create-order-service", e =>
        {
            //okuma işlemini gerçekleştir
            e.ConfigureConsumer<CreateOrderMessageCommandConsumer>(context);
        });
        //event için
        //catalogmicrosevisimiz mesageyi exchange'e gönderiyordu bizim exchangedeki messageyi alabilmemiz için kendimizin bir kuyruk oluşturup ilgili exchange'e maplememiz lazım.
        cfg.ReceiveEndpoint("course-name-changed-event-order-service", e =>
        {
            //okuma işlemini gerçekleştir
            e.ConfigureConsumer<CourseNameChangedEventConsumer>(context);
        });
    });
});
//masstransit 8.0 ile bu koda gerek kalmadı
//services.AddMassTransitHostedService();

//sub değerini framework otomatik olarak nameidentifier’e dönüştürüyor bunu iptal edelim
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");
//jwt bazlı kimlik doğrulama için
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt =>
{
    opt.Authority = builder.Configuration["IdentityServerURL"];
    opt.Audience = "resource_order";
    opt.RequireHttpsMetadata = false;
});

//migration db
builder.Services.AddDbContext<OrderDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), configure =>
    {
        //migrationlarımın FreeCourse.Services.Order.Infrastructure'da oluşmasını istiyorum
        configure.MigrationsAssembly("FreeCourse.Services.Order.Infrastructure");
    });
});

//ISharedIdentityService'i kaydedelim token içindeki userıd bilgisini alabilmek için
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ISharedIdentityService, SharedIdentityService>();
//mediatR kaydet, handlelarım nerede onu belirtmem lazım assambly olarak
//FreeCourse.Services.Order.Application.Handlers.CreateOrderCommandHandler tipini alalım
//sonrasında ise .Assembly diyerek assemblysinialabilirim yani bu tipin bağlı olduğu assemblyin ismini almış olduk.
builder.Services.AddMediatR(typeof(FreeCourse.Services.Order.Application.Handlers.CreateOrderCommandHandler).Assembly);

var app = builder.Build();

//oto migration
using (var scope = app.Services.CreateScope())
{
    //di container'a erişmek için
    var serviceProvider = scope.ServiceProvider;
    //dbcontexte eriş
    var orderDbContext = serviceProvider.GetRequiredService<OrderDbContext>();
    //migration yap
    //önce veritabanı sonra ilgili tablolar oluşacak eğer oluşmamışsa
    orderDbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //ortam development ise UseDeveloperExceptionPage gelsinki hataları görelim orada.
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

//kimlik doğrulama için ekle
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();








//using FreeCourse.Services.Order.Infrastructure;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace FreeCourse.Services.Order.API
//{
//    public class Program
//    {
//        public static void Main(string[] args)
//        {
//            //oto migration
//           var host = CreateHostBuilder(args).Build();
//            using (var scope = host.Services.CreateScope())
//            {
//                //di container'a erişmek için
//                var serviceProvider = scope.ServiceProvider;
//                //dbcontexte eriş
//                var orderDbContext = serviceProvider.GetRequiredService<OrderDbContext>();
//                //migration yap
//                //önce veritabanı sonra ilgili tablolar oluşacak eğer oluşmamışsa
//                orderDbContext.Database.Migrate();
//            }
//           host.Run();
//        }

//        public static IHostBuilder CreateHostBuilder(string[] args) =>
//            Host.CreateDefaultBuilder(args)
//                .ConfigureWebHostDefaults(webBuilder =>
//                {
//                    webBuilder.UseStartup<Startup>();
//                });
//    }
//}
