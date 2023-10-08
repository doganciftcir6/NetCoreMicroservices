using FreeCourse.Web.Models.Order;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FreeCourse.Web.Services.Interface
{
    public interface IOrderService
    {
        //senkron iletişim direkt olarak order microservisine istek yapılacak
        Task<OrderCreatedViewModel> CreateOrderAsync(CheckoutInfoInput checkoutInfoInput);
        //asenkron iletişim olan seneryo için geriye bir şey döndürmeyeceğiz
        //sipariş bilgileri rabbitMQ'ya gönderilecek.
        Task<OrderSuspendViewModel> SuspendOrder(CheckoutInfoInput checkoutInfoInput);

        Task<List<OrderViewModel>> GetOrder();
    }
}
