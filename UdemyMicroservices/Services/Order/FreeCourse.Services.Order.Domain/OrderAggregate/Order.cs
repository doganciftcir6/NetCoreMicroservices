using FreeCourse.Services.Order.Domain.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeCourse.Services.Order.Domain.OrderAggregate
{
    //EF Core features
    //Owned Types
    //Shadow Property
    //Backing Field
    public class Order : Entity, IAggregateRoot
    {
        public DateTime CreatedDate { get; private set; }
        //bunu burda tanımladığım bu hem Order tablosu içinde ek sütunlarda olabilir
        //ayrı bir Address tablosu olup burada ilişki anlamında duruyor da olabilir.
        //bunu belirlemek için Address sınıfıın üstünde [Owned] olarak işaretleyebiliriz
        //veya context içerisinde bunu belirleyebiliriz böyle yapacağız dikkat edersek idsi yok bunun.
        //yani addressin db tarafında karşılığı yok Owned diyerek addresi buraya order sütunu gibi ekliyoruz.
        //bunu yapma sebebimiz ise bu addressi başka yerlerde de kullanabiliriz her seferinde aynı şeyleri yazmayalım diye.
        public Address Address { get; private set; }
        public string BuyerId { get; private set; }
        //eğer get set varsa o property eğey yoksa fielddir bir field tanımlayalım
        //eğer ef core içerisinde okuma ve yazma işlemini prop değilde bir field üzerinden gerçekleştirirsek bunlara backing fields olarak adlandırıyor. Amacı encapsule etmeyi arttırmak. Order üzerinden kimse gidip orderıtem'a data eklemesin diye.
        private readonly List<OrderItem> _orderItems;
        //ekleme yapamasınlar ama okuma yapabilsin dışarıdakiler.
        public IReadOnlyCollection<OrderItem> OrderItems => _orderItems;

        //kendimiz custom bir constractor yazdığımızda default constractor'ı da yazıp belirtmeliyiz yoksa migrationda hata alırız
        //bu default constractorlar lazy loading yapmak istediğinizde önemli çünkü kendisi sonradan propları dolduracağı için kendsiine mutlaka boş bir constractor vermem gerekiyor.
        public Order()
        {

        }
        public Order(string buyerId, Address address)
        {
            _orderItems = new List<OrderItem>();
            CreatedDate = DateTime.Now;
            BuyerId = buyerId;
            Address = address;
        }

        //orderitem ekleyebilmesi için metot
        public void AddOrderItem(string productId, string productName, decimal price, string pictureUrl)
        {
            var existProduct = _orderItems.Any(x => x.ProdcutId == productId);
            if (!existProduct)
            {
                var newOrderItem = new OrderItem(productId, productName, pictureUrl, price);
                _orderItems.Add(newOrderItem);
            }
        }
        //sipariş ile ilgili toplam datayı dönen yardımcı bir prop sadece geti var.
        public decimal GetTotalPrice => _orderItems.Sum(x => x.Price);
    }
}
