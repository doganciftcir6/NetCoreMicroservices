namespace FreeCourse.Web.Models.Basket
{
    public class BasketItemViewModel
    {
        //aynı kursu 2 kez satın alamayız o yüzden quantity1
        public int Quantity { get; set; } = 1;
        public string CourseId { get; set; }
        public string CourseName { get; set; }
        public decimal Price { get; set; }
        //birde eğer kupon kullanılırsa indirimli fiyatı da tutmam lazım ek olarak
        private decimal? DiscountAppliedPrice;
        //indirim uygalnıp uygulanmadığını yani güncel fiyat bilgisini alabilmek için GetCurrentPrice ekleyelim ve kontrolü gerçekleştirelim
        public decimal GetCurrentPrice {  get =>  DiscountAppliedPrice != null ? DiscountAppliedPrice.Value : Price; }
        public void AppliedDiscount(decimal discountPrice)
        {
            DiscountAppliedPrice = discountPrice;
        }
    }
}
