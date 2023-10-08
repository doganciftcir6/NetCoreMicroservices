using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeCourse.Services.Order.Domain.Core
{
    public abstract class Entity
    {
        //Burada entitylerimiz içerisinde bir çok yerde ortak olarak kullanacağımız metotlarımız olabilir.
        //Bunlar özellikle karşılaştırma metotları 2 entityin birbirine eşit olup olmadığıya ilgili.
        //Bunları kodları mirosoft sitesinden hazır olarak aldık
        private int? _requestedHashCode;
        private int _Id;

        public virtual int Id
        {
            get => _Id;
            set => _Id = value;
        }
        //defaultsa demekki veritabanında bir kaşılığı yok anlamında
        public bool IsTransient()
        {
            return this.Id == default(Int32);
        }

        public override int GetHashCode()
        {
            if (!IsTransient())
            {
                if (!_requestedHashCode.HasValue)
                    _requestedHashCode = this.Id.GetHashCode() ^ 31; // XOR for random distribution (http://blogs.msdn.com/b/ericlippert/archive/2011/02/28/guidelines-and-rules-for-gethashcode.aspx)

                return _requestedHashCode.Value;
            }
            else
                return base.GetHashCode();
        }
        //2 objenin biririne eşit olup olmadığı kontrolü tipinin,referansının aynı zamanda ıd sinin
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Entity))
                return false;

            if (Object.ReferenceEquals(this, obj))
                return true;

            if (this.GetType() != obj.GetType())
                return false;

            Entity item = (Entity)obj;

            if (item.IsTransient() || this.IsTransient())
                return false;
            else
                return item.Id == this.Id;
        }
        //Eşitse bu çalışsın soldaki ile sağdakini karşılaştırsın
        public static bool operator ==(Entity left, Entity right)
        {
            if (Object.Equals(left, null))
                return (Object.Equals(right, null)) ? true : false;
            else
                return left.Equals(right);
        }
        //eşit değilse bu çalışsın
        public static bool operator !=(Entity left, Entity right)
        {
            if (Object.Equals(left, null))
                return (Object.Equals(right, null)) ? true : false;
            else
                return left.Equals(right);
        }
    }
}
