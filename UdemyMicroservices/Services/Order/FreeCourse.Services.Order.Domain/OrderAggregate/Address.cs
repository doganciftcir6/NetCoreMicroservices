using FreeCourse.Services.Order.Domain.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeCourse.Services.Order.Domain.OrderAggregate
{
    public class Address : ValueObject
    {
        //bu propları dışarıdan set etmeye kapatalım.
        //Bu arkadaş dışarıdan propun durumunu set etmesin. 
        //set etme benim kontrolümde olsun
        public string Province { get; private set; }
        public string District { get; private set; }
        public string Street { get; private set; }
        public string ZipCode { get; private set; }
        public string Line { get; private set; }

        //stateini değiştiremezse nasıl ekleme yapacak yeni bir nesne oluşturacak
        //burada kendi constraktırımı kendim oluşturuyorum
        //yani set edebilmek için mutlaka bu constractırı kullanması gerek.
        public Address(string province, string district, string street, string zipCode, string line)
        {
            Province = province;
            District = district;
            Street = street;
            ZipCode = zipCode;
            Line = line;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            //alanları döneceğim o da equal metotunda bu değerlerin hem
            //null olup olmaması hem tipine baksın hemde içerisindeki değerlerinde
            //eşit olup olmadığına baksın, valueobjectin durumunu koruyorum.
            //kimse dışarıdan bu arkadaşın state'ini değiştiremesin.
            yield return Province;
            yield return District;
            yield return Street;
            yield return ZipCode;
            yield return Line;
        }

        //ayrıca business kodum varsa burada uygulayabilirim.
        public void SetBusinessKuralı(string zipCode)
        {
            //Business code
            //business kuralından geçemezse hata fırlat

            //eğer business kodundan geçerse setleme işemini burda yap gibi olabilir.
            ZipCode = zipCode;
        }
    }
}
