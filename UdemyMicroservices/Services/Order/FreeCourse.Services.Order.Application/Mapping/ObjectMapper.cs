using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeCourse.Services.Order.Application.Mapping
{
    //Katmanda Automapperi register edecek DI container olamdığı için bu kullanıma ihtiyaç duyduk.
    public static class ObjectMapper
    {
        //sadece istenildiği anda bir şeyleri initialize etmek için Lazy 
        //yani ben automapperi ne zaman kullanırsam bu ObjectMapper'ı ne zaman kullanırsam
        //o zaman initialize edilsin normalde uygulama ayağa kalkar kalkmaz olurdu
        private static readonly Lazy<IMapper> lazy = new Lazy<IMapper>(() =>
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CustomMapping>();
            });
            return config.CreateMapper();
        });

        //ben bu prop mapper'i çağırana kadar yukarıdaki kodlar çalışmayacak. Lazy.Value sayesinde
        //hiç çağırmazsam hiç çalışmaz.
        public static IMapper Mapper => lazy.Value;
    }
}
