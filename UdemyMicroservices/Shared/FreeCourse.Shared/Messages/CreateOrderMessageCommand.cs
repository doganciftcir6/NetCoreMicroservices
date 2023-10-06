using System;
using System.Collections.Generic;
using System.Text;

namespace FreeCourse.Shared.Messages
{
    public class CreateOrderMessageCommand
    {
        //controller tarafında createOrderMessageCommand.OrderItems.Add(new OrderItem{ });
        //bu şekilde direkt olarak ekleme yapabilmem için önce bir nesne örneği olması gerekiyor
        //yoksa bu noktada hata alırız bu hatadan kurtulmak için:
        public CreateOrderMessageCommand()
        {
            OrderItems = new List<OrderItem>();
        }
        //Mesajı işleyecek olan microservis yani Order mesajı
        //işlemek için benden hangi bilgileri istiyor
        public string BuyerId { get; set; }
        public List<OrderItem> OrderItems { get; set; }
        //Adress bilgileri
        public string Province { get; set; }
        public string District { get; set; }
        public string Street { get; set; }
        public string ZipCode { get; set; }
        public string Line { get; set; }
    }

    public class OrderItem
    {
        public string ProdcutId { get; set; }
        public string ProductName { get; set; }
        public string PictureUrl { get; set; }
        public Decimal Price { get; set; }
    }
}
