using System.Collections.Generic;
using System.Linq;

namespace FreeCourse.Services.Basket.Dtos
{
    public class BasketDto
    {
        public string UserId { get; set; }
        public string DiscountCode { get; set; }
        public int? DiscountRate { get; set; }
        public decimal TotalPrice
        {
            get => BasketItems.Sum(x => x.Price * x.Quantity); //Total price alanı için sepetteki itemslarımın tek tek fiyatı ile miktarını çarpıp hepsini toplasın ve bu propun içine atsın
        }

        //bire çok ilişki bir basketin birden çok itemi olabilir.
        public List<BasketItemDto> BasketItems { get; set; }
    }
}
