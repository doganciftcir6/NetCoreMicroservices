using Microsoft.VisualBasic;
using System;

namespace FreeCourse.Services.Discount.Models
{
    //postgreqsql içerisinde tablolar küçük harflerle tutulur
    //o yüzden postgresql içindeki küçük harflerle başlayan
    //discount tablosu buradaki büyük harfli Discounta eşit olsun diye mapleme yapalım.
    [Dapper.Contrib.Extensions.Table("discount")]
    public class Discount
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int Rate { get; set; }
        public string Code { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
