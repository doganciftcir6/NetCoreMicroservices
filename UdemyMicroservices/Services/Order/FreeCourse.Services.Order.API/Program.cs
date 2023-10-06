using FreeCourse.Services.Order.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FreeCourse.Services.Order.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //oto migration
           var host = CreateHostBuilder(args).Build();
            using (var scope = host.Services.CreateScope())
            {
                //di container'a erişmek için
                var serviceProvider = scope.ServiceProvider;
                //dbcontexte eriş
                var orderDbContext = serviceProvider.GetRequiredService<OrderDbContext>();
                //migration yap
                //önce veritabanı sonra ilgili tablolar oluşacak eğer oluşmamışsa
                orderDbContext.Database.Migrate();
            }
           host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
