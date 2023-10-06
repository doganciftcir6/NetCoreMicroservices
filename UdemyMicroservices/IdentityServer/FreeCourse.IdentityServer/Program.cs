// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using FreeCourse.IdentityServer.Data;
using FreeCourse.IdentityServer.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Linq;

namespace FreeCourse.IdentityServer
{
    public class Program
    {
        public static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
                .Enrich.FromLogContext()
                // uncomment to write to Azure diagnostics stream
                //.WriteTo.File(
                //    @"D:\home\LogFiles\Application\identityserver.txt",
                //    fileSizeLimitBytes: 1_000_000,
                //    rollOnFileSizeLimit: true,
                //    shared: true,
                //    flushToDiskInterval: TimeSpan.FromSeconds(1))
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Code)
                .CreateLogger();

            try
            {
                var host = CreateHostBuilder(args).Build();

                //Auto Migration
                //using ifadesi ile burada işlemim bittikten sonra memoryden düşsün
                using (var scope = host.Services.CreateScope())
                {
                    //serviceprovider üzerinden statrup tarafındaki services.AddDbContext'e erişicez bir nesne örneğini alıcaz.
                    var serviceProvider = scope.ServiceProvider;
                    //getrequiredsevice yani mutlaka servis olmalı diyoruz yoksa exception fırlatır
                    var applicationDbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
                    //bu Migrate eğer database yoksa oluşturacak ve olan uygulanmamış migrationlarım varsa onları uygulayacak. Uygulama ayağa kalkarken.
                    applicationDbContext.Database.Migrate();
                    //eğer veritabanında kullanıcı yoksa birde kullanıcı oluşturalım
                    //kullanıcı oluşturmakla ilgili UserManager sınıfına erişmem gerekiyor. Identity kütüphanesinden geliyor. ApplicationUser ise model klasöründen geliyor.
                    var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                    if (!userManager.Users.Any())
                    {
                        //kullanıcı yoksa 1 tane kullanıcı oluştur
                        //Bu işlem asenkron bir metot hiç burayı asekron yapmakla uğraşmıyalım Wait metotuyla buranın senkron olarak çalışmasını sağlayalım.
                        userManager.CreateAsync(new ApplicationUser { UserName = "fcakiroglu16", Email = "fcakiroglu@outlook.com", City = "Ankara" }, "Password12*").Wait();
                    }
                }

                Log.Information("Starting host...");
                host.Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly.");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}