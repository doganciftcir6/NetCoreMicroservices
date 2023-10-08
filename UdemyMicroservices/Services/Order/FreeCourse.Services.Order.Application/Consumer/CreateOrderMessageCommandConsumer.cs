using FreeCourse.Services.Order.Infrastructure;
using FreeCourse.Shared.Messages;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeCourse.Services.Order.Application.Consumer
{
    //IConsumer<CreateOrderMessageCommand> ile bunu tüketmek istediğimi söylüyorum
    public class CreateOrderMessageCommandConsumer : IConsumer<CreateOrderMessageCommand>
    {
        //veritabanı ile işlem yapacağız
        private readonly OrderDbContext _dbContext;
        public CreateOrderMessageCommandConsumer(OrderDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        //burada messagedeki verileri okuyup dbye kayıt yapacağız
        public async Task Consume(ConsumeContext<CreateOrderMessageCommand> context)
        {
            //buradaki context aslında kuyruktan gelen dinlediğimiz veriler oluyor.
            //önce address bilgisini oluşturalım
            var newAddress = new Domain.OrderAggregate.Address(context.Message.Province, context.Message.District, context.Message.Street, context.Message.ZipCode, context.Message.Line);
            //Order oluşturalım
            Domain.OrderAggregate.Order order = new Domain.OrderAggregate.Order(context.Message.BuyerId, newAddress);
            //ordera foreach ile dönüp itemlarınıda eklemem lazım
            //Bu metotu entity içide hazrılamıştık yardımcı metot olarak daha önce
            context.Message.OrderItems.ForEach(x =>
            {
                order.AddOrderItem(x.ProdcutId, x.ProductName, x.Price, x.PictureUrl);
            });
            //kaydetme işlemini gerçekleştir
            await _dbContext.Orders.AddAsync(order);
            await _dbContext.SaveChangesAsync();
        }
    }
}
