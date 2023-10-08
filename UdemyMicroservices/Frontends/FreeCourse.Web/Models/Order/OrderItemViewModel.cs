using System;

namespace FreeCourse.Web.Models.Order
{
    public class OrderItemViewModel
    {
        public string ProdcutId { get; set; }
        public string ProductName { get; set; }
        public string PictureUrl { get; set; }
        public Decimal Price { get; set; }
    }
}
