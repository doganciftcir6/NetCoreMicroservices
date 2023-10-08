using FreeCourse.Services.Order.Infrastructure;
using FreeCourse.Shared.Messages;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeCourse.Services.Order.Application.Consumer
{
    //burası eventi dinleyecek ve ilgili işlemi gerçekleştirecek
    public class CourseNameChangedEventConsumer : IConsumer<CourseNameChangedEvent>
    {
        //dbde güncelleme yapmak için dbye bağlanmam lazım
        private readonly OrderDbContext _orderDbContext;
        public CourseNameChangedEventConsumer(OrderDbContext orderDbContext)
        {
            _orderDbContext = orderDbContext;
        }

        public async Task Consume(ConsumeContext<CourseNameChangedEvent> context)
        {
            //buradaki context aslında bize gelen eventteki veriler oluyor.
            //productname alanı OrderItems tablosunda bulunuyor
            var orderItems = await _orderDbContext.OrderItems.Where(x => x.ProdcutId == context.Message.CourseId).ToListAsync();
            orderItems.ForEach(x =>
            {
                //sadece name alanını güncelleceyeğiz diğerleri aynı
                //bu metotu entity içinde yazmıştık daha önce yardımcı metot olarak.
                x.UpdateOrderItem(context.Message.UpdatedName, x.PictureUrl, x.Price);
            });
            //veriler memoryde güncellendi şimdi veritabanına yansıması için
            await _orderDbContext.SaveChangesAsync();
        }
    }
}
