namespace FreeCourse.Web.Models
{
    public class ServiceApiSettings
    {
        //IdentityServerdeki url
        //buradaki isimler appsettingsteki tanımlanan isimler ile aynı
        public string IdentityBaseUri { get; set; }
        public string GatewayBaseUri { get; set; }
        //PhotoStock microsevis hangi urlden ayağa kalkıyor
        public string PhotoStockUri { get; set; }
        public ServiceApi Catalog { get; set; }
        public ServiceApi PhotoStock { get; set; }
        public ServiceApi Basket { get; set; }
        public ServiceApi Discount { get; set; }
        public ServiceApi Payment { get; set; }
        public ServiceApi Order { get; set; }
    }

    public class ServiceApi
    {
        public string Path { get; set; }
    }

}
