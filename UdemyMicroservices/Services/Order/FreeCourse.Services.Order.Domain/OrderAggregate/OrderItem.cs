using FreeCourse.Services.Order.Domain.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeCourse.Services.Order.Domain.OrderAggregate
{
    public class OrderItem : Entity
    {
        //order olarak kurs satıyorum ya kurs benim için bir product
        //bu alanların setini dış dünyaya kapatalım sadece bu class içerisinden set edilebilsinler
        public string ProdcutId { get; private set; }
        public string ProductName { get; private set; }
        public string PictureUrl { get; private set; }
        public Decimal Price { get; private set; }
        //DDD kullanmasaydım buraya birde ilişki için OrderId navigation propunu koymam gerekirdi.
        //ama DDD kullandığımda bunu buraya koymama gerek yok bu artık benim için shadow property oluyor
        //yani ef core bu ilişkiyi benim için db tarafında bu orderId alanını OrderItem tablosunda tanımlar
        //ama bu kod tarafında entity içerisinde bir property olarak karşılığı olmuyor.

        //kendimiz custom bir constractor yazdığımızda default constractor'ı da yazıp belirtmeliyiz yoksa migrationda hata alırız
        public OrderItem()
        {

        }
        //konstaktır oluşturalım set edilmek isteniyorsa mutlaka bunun üzerinden set edilsin.
        public OrderItem(string prodcutId, string productName, string pictureUrl, decimal price)
        {
            ProdcutId = prodcutId;
            ProductName = productName;
            PictureUrl = pictureUrl;
            Price = price;
        }

        //bu alanlar private olduğu için bunları dışarıdan update edemez
        //ben kendi metotlarımı yazarak benim kontrolümde orderıtemin stateini değiştiriyorum
        //benim metotlarımı kullanarak set edecek. Çünkü ben bu metotta business kuralı uygulatabilirim.
        public void UpdateOrderItem(string productName, string pictureUrl, decimal price)
        {
            ProductName = productName;
            Price = price;
            PictureUrl= pictureUrl;
        }

    }
}
