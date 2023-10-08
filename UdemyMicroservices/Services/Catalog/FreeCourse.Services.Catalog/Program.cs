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

//t�m contollerlara authrize attributeu eklemek i�in 
builder.Services.AddControllers(opt =>
{
    //art�k t�m conrollerlar�m�z� koruma alt�na ald�k hepsine Authrize attributeu gelmi� oldu.
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
            //birde rabbitmqnun ui ekran�n� farkl� porttan kald�raca��z
            //yani rabbitmq kald�r�rken 2 port olacak birisi rabbitmqnun normal portu
            //diperi ise rabbitmqnun ui portu olacak defaultu 15672'dir.
            host.Username("guest");
            host.Password("guest");
        });
    });
});

//jwt
//�emalar�n ismi birden fazla jwt bekleyebiliriz. Bayiler i�in bir token birde normal kullan�c�lar i�in bir token bekliyorsun bu �yelik sistemlerini birbirinden ay�rmak i�in �ema kavram� kullan�l�r. Bizde tek bir sistem oldu�u i�in default ismi verelim.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt =>
{
    //kritik 2 de�er belirciem 1 bu microservise tokenin kimin da��tt���n�n bilgisi (bunu appsettingsten al ��nk� localhost var localhostu kesinlikle direkt buraya kodlar�n i�ine yazmayal�m ��nk� dockerize etti�imizde bu localhost de�i�ecek.)
    opt.Authority = builder.Configuration["IdentityServerURL"];
    //bu uygulamama private key ile imzalanm�� bir token geldi�inde public key ile do�rulaams�n� yapacak ama public keyi nereden alacak i�te appsettingste yukar�da belirtti�imiz urlden gidecek oradaki endpointten alacak public keyi alacak gelen private key ile kar��la�t�racak e�er do�ruysa eyvallah diyecek. �kinci bir i�lem olarak benim audiece yani gelen tokenin i�erisinde mutlaka resource_catalog olmas� laz�m bunu IdentityServer micrsoervisimizin config k�sm�nda belirtmi�tik.
    //art�k gelen tokenin payload�nda audiece parametrelerine bakacak e�er resource_catalog varsa tamam istek yapabilirsin diyecek. Arkas�ndan e�er scope parametresi ile beraber claim bazl� bir do�rulama yapacaksan buradan da scope k�sm�na bakacak ama �uan bunu kullanm�yoruz.
    opt.Audience = "resource_catalog";
    //birde https kullanmakd�k o y�zden mutlaka bunu burda belirtmem laz�m default olarak https bekler.
    opt.RequireHttpsMetadata = false;
});

//ICategoryService ile kar��la�t���n zaman git CategoryService'ten bir nesne �rne�i getir bana DI COntainer.
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICourseService, CourseService>();

//automapper register, uygulaman�n g�ncel Assembly'sini verelim o Assembly'i uygulama aya�a kalkarken tarayacak Profile s�n�f�ndan miras al�nan mapleme s�n�flar�n� bulup memorye ekleyecek.
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());


//appsettingste tan�mlad���m�z DatabaseSettings datalar�n� olu�turdu�umuz classtaki proplara atarak doldural�m. OptionsPattern.
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("DatabaseSettings"));
//uzun uzun dependency injectionda IOptions<DatabaseSettings> _optinons  ge�mek yerine direkt IDatabaseSettings ge�elim o bize IOptions<DatabaseSetting> �rne�i versin. GetRequiredService ilgili servisi bulamazsa geriye hata f�rlat�r o y�zden bunu kullanmaya �zen g�ster.
builder.Services.AddSingleton<IDatabaseSettings>(opt =>
{
    return opt.GetRequiredService<IOptions<DatabaseSettings>>().Value;
});

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var categoryService = serviceProvider.GetRequiredService<ICategoryService>();
    ///.Result komutu asenkron olan kodu senkrona �evirir, asenkron metot geriye bir data d�n�yorsa wait yerine bunu kullan�r�z.
    if (!(await categoryService.GetAllAsync()).Data.Any())
    {
        //Wait() metotu asenkron olan metotu senkrona �evirir yani var olan threadi kitler ve geriye bir �ey d�nemdi�imizde kullan�r�z ama await asenkron olan kodu yine asenkron olarak b�rak�r yani var olan threadi kitlemez.
        await categoryService.CreateAsync(new CategoryDto { Name = "Asp.net Core Kursu" });
        await categoryService.CreateAsync(new CategoryDto { Name = "Asp.net ,API Kursu" });
    }
}

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
//            //e�er db de hi� veri yoksa otomatik olarak veri eklemesi yaps�n dbye
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
