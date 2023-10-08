using FreeCourse.Services.Order.Application.Dtos;
using FreeCourse.Shared.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeCourse.Services.Order.Application.Queries
{
    // IRequest<> içerisine geriye response olarak ne döneceğimi belirtiyorum
    //Requestim CreateOrderCommand olacak zaten IRequest'ten kalıttığım için
    public class GetOrdersByUserIdQuery : IRequest<Response<List<OrderDto>>>
    {
        //parametre olarak ne alacağım
        //ben controller tarafında GetOrdersByUserIdQuery'dan bir nesne örneği oluşturup UserId bilgisini doldurduğumda MediatR 'a gönderdiğimde MediatR'Da bu sınıfı Handle edecek olan sınıfı kendisi otomatik olarak MediatR Dessing Pattern ile beraber kendisi bulacak.
        public string UserId { get; set; }
    }
}
