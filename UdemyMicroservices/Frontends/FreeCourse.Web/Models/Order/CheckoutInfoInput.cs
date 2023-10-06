using System.ComponentModel.DataAnnotations;

namespace FreeCourse.Web.Models.Order
{
    public class CheckoutInfoInput
    {
        //adress bilgileri
        [Display(Name = "İl")]
        public string Province { get; set; }
        [Display(Name = "İlçe")]
        public string District { get; set; }
        [Display(Name = "Cadde")]
        public string Street { get; set; }
        [Display(Name = "Posta Kodu")]
        public string ZipCode { get; set; }
        [Display(Name = "Adres")]
        public string Line { get; set; }
        //kart bilgileri, payment'a gidecek
        [Display(Name = "Kart İsim Soy İsim")]
        public string CardName { get; set; }
        [Display(Name = "Kart Numarası")]
        public string CardNumber { get; set; }
        [Display(Name = "Son Kullanma Tarihi(AY/YIL)")]
        public string Expiration { get; set; }
        [Display(Name = "CVV/CVC Numarası")]
        public string CVV { get; set; }

    }
}
