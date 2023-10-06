using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace FreeCourse.Web.Models.Basket
{
    public class BasketViewModel
    {
        //bu class newlendiği zaman _BasketItems tarafı boş kalmasın boş bir List<BasketItemViewModel> nesne örneği atalım içine ki hata durumunu engelleyelim
        public BasketViewModel()
        {
            _BasketItems = new List<BasketItemViewModel>();
        }
        public string UserId { get; set; }
        public string DiscountCode { get; set; }
        public int? DiscountRate { get; set; }
        public decimal TotalPrice
        {
            get => _BasketItems.Sum(x => x.GetCurrentPrice * x.Quantity); //Total price alanı için sepetteki itemslarımın tek tek fiyatı ile miktarını çarpıp hepsini toplasın ve bu propun içine atsın
        }
        //indirim var mı yok mu onu kontrol edecek bir yapı kurabiliriz
        public bool HasDiscount { get => !string.IsNullOrEmpty(DiscountCode) && DiscountRate.HasValue; }


        //bire çok ilişki bir basketin birden çok itemi olabilir.
        private List<BasketItemViewModel> _BasketItems;
        //eğer indirim varsa BasketItemsları güncellemem lazım
        public List<BasketItemViewModel> BasketItems 
        {
            get 
            {
                //indirim varsa indirimi uygulayalım
                if (HasDiscount && DiscountRate.HasValue)
                {
                    //ornek kurs fiyat 100TL indirim %10
                    _BasketItems.ForEach(item =>
                    {
                        //yüzde 10luk 
                        var discountPirce = item.Price * ((decimal)DiscountRate.Value / 100);
                        item.AppliedDiscount(Math.Round(item.Price-discountPirce,2)); //90.00
                    });
                }
                return _BasketItems;
            } 
            set
            {
                _BasketItems = value;
            }
        }
        public void CancelDiscount()
        {
            DiscountCode = null;
            DiscountRate = null;
        }
        public void ApplyDiscount(string code, int rate)
        {
            DiscountCode = code;
            DiscountRate = rate;
        }
    }
}
