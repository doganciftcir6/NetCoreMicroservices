using System.Collections.Generic;

namespace FreeCourse.Web.Models.Order
{
    public class OrderCreateInput
    {
        //OrderCreateInput newlendiğinde boş bir List<OrderItemCreateInput>
        //oluşsun hata durumu olmasın
        //orderCreateInput.OrderItems = new List<OrderItems>() olarak kullanılırsa hata vermez ama
        //orderCreateInput.OrderItems.Add() kullanılırsa hata vermesin
        public OrderCreateInput()
        {
            OrderItems = new List<OrderItemCreateInput>();
        }
        public string BuyerId { get; set; }
        public List<OrderItemCreateInput> OrderItems { get; set; }
        public AddressCreateInput Address { get; set; }
    }
}
