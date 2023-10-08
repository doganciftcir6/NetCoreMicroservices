using FreeCourse.Services.Catalog.Dtos;
using FreeCourse.Services.Catalog.Services;
using FreeCourse.Services.Catalog.Settings;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//tüm contollerlara authrize attributeu eklemek için 
builder.Services.AddControllers(opt =>
{
    //artýk tüm conrollerlarýmýzý koruma altýna aldýk hepsine Authrize attributeu gelmiþ oldu.
    opt.Filters.Add(new AuthorizeFilter());
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

//jwt
//þemalarýn ismi birden fazla jwt bekleyebiliriz. Bayiler için bir token birde normal kullanýcýlar için bir token bekliyorsun bu üyelik sistemlerini birbirinden ayýrmak için þema kavramý kullanýlýr. Bizde tek bir sistem olduðu için default ismi verelim.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt =>
{
    //kritik 2 deðer belirciem 1 bu microservise tokenin kimin daðýttýðýnýn bilgisi (bunu appsettingsten al çünkü localhost var localhostu kesinlikle direkt buraya kodlarýn içine yazmayalým çünkü dockerize ettiðimizde bu localhost deðiþecek.)
    opt.Authority = builder.Configuration["IdentityServerURL"];
    //bu uygulamama private key ile imzalanmýþ bir token geldiðinde public key ile doðrulaamsýný yapacak ama public keyi nereden alacak iþte appsettingste yukarýda belirttiðimiz urlden gidecek oradaki endpointten alacak public keyi alacak gelen private key ile karþýlaþtýracak eðer doðruysa eyvallah diyecek. Ýkinci bir iþlem olarak benim audiece yani gelen tokenin içerisinde mutlaka resource_catalog olmasý lazým bunu IdentityServer micrsoervisimizin config kýsmýnda belirtmiþtik.
    //artýk gelen tokenin payloadýnda audiece parametrelerine bakacak eðer resource_catalog varsa tamam istek yapabilirsin diyecek. Arkasýndan eðer scope parametresi ile beraber claim bazlý bir doðrulama yapacaksan buradan da scope kýsmýna bakacak ama þuan bunu kullanmýyoruz.
    opt.Audience = "resource_catalog";
    //birde https kullanmakdýk o yüzden mutlaka bunu burda belirtmem lazým default olarak https bekler.
    opt.RequireHttpsMetadata = false;
});

//ICategoryService ile karþýlaþtýðýn zaman git CategoryService'ten bir nesne örneði getir bana DI COntainer.
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICourseService, CourseService>();

//automapper register, uygulamanýn güncel Assembly'sini verelim o Assembly'i uygulama ayaða kalkarken tarayacak Profile sýnýfýndan miras alýnan mapleme sýnýflarýný bulup memorye ekleyecek.
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());


//appsettingste tanýmladýðýmýz DatabaseSettings datalarýný oluþturduðumuz classtaki proplara atarak dolduralým. OptionsPattern.
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("DatabaseSettings"));
//uzun uzun dependency injectionda IOptions<DatabaseSettings> _optinons  geçmek yerine direkt IDatabaseSettings geçelim o bize IOptions<DatabaseSetting> örneði versin. GetRequiredService ilgili servisi bulamazsa geriye hata fýrlatýr o yüzden bunu kullanmaya özen göster.
builder.Services.AddSingleton<IDatabaseSettings>(opt =>
{
    return opt.GetRequiredService<IOptions<DatabaseSettings>>().Value;
});

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var categoryService = serviceProvider.GetRequiredService<ICategoryService>();
    ///.Result komutu asenkron olan kodu senkrona çevirir, asenkron metot geriye bir data dönüyorsa wait yerine bunu kullanýrýz.
    if (!(await categoryService.GetAllAsync()).Data.Any())
    {
        //Wait() metotu asenkron olan metotu senkrona çevirir yani var olan threadi kitler ve geriye bir þey dönemdiðimizde kullanýrýz ama await asenkron olan kodu yine asenkron olarak býrakýr yani var olan threadi kitlemez.
        await categoryService.CreateAsync(new CategoryDto { Name = "Asp.net Core Kursu" });
        await categoryService.CreateAsync(new CategoryDto { Name = "Asp.net ,API Kursu" });
    }
}

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







//using FreeCourse.Services.Catalog.Dtos;
//using FreeCourse.Services.Catalog.Models;
//using FreeCourse.Services.Catalog.Services;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace FreeCourse.Services.Catalog
//{
//    public class Program
//    {
//        public static void Main(string[] args)
//        {
//            //eðer db de hiç veri yoksa otomatik olarak veri eklemesi yapsýn dbye
//            var host = CreateHostBuilder(args).Build();
//            using (var scope = host.Services.CreateScope())
//            {
//                var serviceProvider = scope.ServiceProvider;
//                var categoryService = serviceProvider.GetRequiredService<ICategoryService>();
//                if (!categoryService.GetAllAsync().Result.Data.Any())
//                {
//                    categoryService.CreateAsync(new CategoryDto { Name = "Asp.net Core Kursu" }).Wait();
//                    categoryService.CreateAsync(new CategoryDto { Name = "Asp.net ,API Kursu" }).Wait();
//                }
//            }
//            host.Run();
//        }

//        public static IHostBuilder CreateHostBuilder(string[] args) =>
//            Host.CreateDefaultBuilder(args)
//                .ConfigureWebHostDefaults(webBuilder =>
//                {
//                    webBuilder.UseStartup<Startup>();
//                });
//    }
//}
