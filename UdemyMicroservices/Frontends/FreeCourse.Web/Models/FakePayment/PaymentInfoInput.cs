using FreeCourse.Web.Models.Order;

namespace FreeCourse.Web.Models.FakePayment
{
    public class PaymentInfoInput
    {
        public string CardName { get; set; }
        public string CardNumber { get; set; }
        public string Expiration { get; set; }
        public string CVV { get; set; }
        public decimal TotalPrice { get; set; }
        //asenkron iletişim için ekle
        public OrderCreateInput Order { get; set; }
    }
}
