using FreeCourse.Services.FakePayment.Models;
using FreeCourse.Shared.ControllerBases;
using FreeCourse.Shared.Dtos;
using FreeCourse.Shared.Messages;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace FreeCourse.Services.FakePayment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FakePaymentsController : CustomBaseController
    {
        //rabbitmq'ya messageyi buradan göndereceğiz
        private readonly ISendEndpointProvider _sendEndpointProvider;
        public FakePaymentsController(ISendEndpointProvider sendEndpointProvider)
        {
            _sendEndpointProvider = sendEndpointProvider;
        }

        [HttpPost]
        public async Task<IActionResult> ReceivePaymewnt(PaymentDto paymentDto)
        {
            //Command göndereceğiz o  yüzden SendEndpoint
            //GetSendEndpoint() ile bana ISendEndpoint interfacesini implement etmiş olan bir endpoint ver.
            //ve hangi kuyruğa gönderilecek onu belirtiyoruz. Kuyruk ismi yazacağız.
            var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri("queue:create-order-service"));
            //burada automapper kullanılabilir
            var createOrderMessageCommand = new CreateOrderMessageCommand();
            createOrderMessageCommand.BuyerId = paymentDto.Order.BuyerId;
            createOrderMessageCommand.Province = paymentDto.Order.Address.Province;
            createOrderMessageCommand.District = paymentDto.Order.Address.District;
            createOrderMessageCommand.Street = paymentDto.Order.Address.Street;
            createOrderMessageCommand.Line = paymentDto.Order.Address.Line;
            createOrderMessageCommand.ZipCode = paymentDto.Order.Address.ZipCode;
            paymentDto.Order.OrderItems.ForEach(x =>
            {
                createOrderMessageCommand.OrderItems.Add(new OrderItem
                {
                    PictureUrl = x.PictureUrl,
                    Price = x.Price,
                    ProdcutId = x.ProdcutId,
                    ProductName = x.ProductName
                });
            });
            //elimde artık message var artık bu messageyi gönder
            await sendEndpoint.Send<CreateOrderMessageCommand>(createOrderMessageCommand);


            //paymentDto ile ödeme işlemi gerçekleştir.
            return CreateActionResultInstance(Shared.Dtos.Response<NoContent>.Success(200));
        }
    }
}
