using FreeCourse.Services.Order.Application.Dtos;
using FreeCourse.Shared.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeCourse.Services.Order.Application.Commands
{
    // IRequest<> içerisine geriye response olarak ne döneceğimi belirtiyorum
    //Requestim CreateOrderCommand olacak zaten IRequest'ten kalıttığım için
    public class CreateOrderCommand : IRequest<Response<CreatedOrderDto>>
    {
        //sipariş oluşturmayla iligli tüm propları alacağım (almam gereken parametreler)
        public string BuyerId { get; set; }
        public List<OrderItemDto> OrderItems { get; set; }
        public AddressDto Address { get; set; }
    }
}
