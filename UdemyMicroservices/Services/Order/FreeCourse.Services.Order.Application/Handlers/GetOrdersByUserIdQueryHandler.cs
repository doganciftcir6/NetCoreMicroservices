using FreeCourse.Services.Order.Application.Dtos;
using FreeCourse.Services.Order.Application.Mapping;
using FreeCourse.Services.Order.Application.Queries;
using FreeCourse.Services.Order.Infrastructure;
using FreeCourse.Shared.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreeCourse.Services.Order.Application.Handlers
{
    //veritabanına gidip datayı alacak olan sınıfım burası olacak.
    //IRequestHandler<> içerisine request olarak ne aldığımı yazıyorum ve dönüşünü yine burada belirtiyorum
    public class GetOrdersByUserIdQueryHandler : IRequestHandler<GetOrdersByUserIdQuery, Response<List<OrderDto>>>
    {
        //GetOrdersByUserIdQuery sınıfını MediatR'a gönderdiğimde MediatR bu sınıfı kimin handle edeceğini GetOrdersByUserIdQueryHandler biliyor ve arkasından Handle metotunu çalıştırıyor.
        //eğer repository pattern kullansaydık burada context yerine repositorylerimizi geçiyor olurduk.
        private readonly OrderDbContext _context;
        public GetOrdersByUserIdQueryHandler(OrderDbContext context)
        {
            _context = context;
        }

        public async Task<Response<List<OrderDto>>> Handle(GetOrdersByUserIdQuery request, CancellationToken cancellationToken)
        {
            var orders = await _context.Orders.Include(x => x.OrderItems).Where(x => x.BuyerId == request.UserId).ToListAsync();
            if (!orders.Any())
            {
                return Response<List<OrderDto>>.Success(new List<OrderDto>(), 200);
            }
            var ordersDto = ObjectMapper.Mapper.Map<List<OrderDto>>(orders);
            return Response<List<OrderDto>>.Success(ordersDto, 200);
        }
    }
}
