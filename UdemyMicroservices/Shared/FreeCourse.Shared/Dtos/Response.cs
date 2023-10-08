using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace FreeCourse.Shared.Dtos
{
    //bu classı 2 ye bölebilirdik successdto ve errordto olarak ama biz tek bir class içerisinde bu iki işlemide yapabilriz. Başarılı olduğunda Data propunu doldurucaz başarısız olunduğunda Errors propunu doldurucaz.
    public class Response<T>
    {
        public T Data { get; set; }
        //responseun bodysin bir daha statuscode göndermeme gerek yok çünkü zaten her isteğin sonucunda geriye dönen bir statuscodu var o yüzden JsonIgnore yapıyorum. Ama yazılım içerisinde benim buna ihtiyacım var.Ben responseun dönüş tipini belirlerken bundan faydalanıcam.
        [JsonIgnore]
        public int StatusCode { get; set; }
        //başarılı mı başarılı değil mi kısmınıda ben sadece yazılım tarafında kullanıcam bunun response bodysinde gözükmesine şuanlık gerek yok zaten bu bilgiyi anlayabiliyorum bir daha gözükmesin.
        [JsonIgnore]
        public bool IsSuccessful { get; set; }
        public List<string> Errors { get; set; }

        //bu responsedto nesnesini üretmek için statik metotlar Static Factory Method
        //başarılı ve data var
        public static Response<T> Success(T data, int statusCode)
        {
            return new Response<T> { Data = data, StatusCode = statusCode, IsSuccessful = true };
        }
        //başarılı ama data olmama durumu eğer datası yoksa bu t yerine NoContent classını verebilirim.
        public static Response<T> Success(int statusCode)
        {
            return new Response<T> { Data = default(T), StatusCode = statusCode, IsSuccessful = true };
        }
        //fail durumu birden çok hata
        public static Response<T> Fail(List<string> errors, int statusCode)
        {
            return new Response<T> { Errors = errors, StatusCode = statusCode, IsSuccessful = false };
        }
        //fail durumu tek hata
        public static Response<T> Fail(string error, int statusCode)
        {
            return new Response<T> { Errors = new List<string>() { error }, StatusCode = statusCode, IsSuccessful = false };
        }
    }
}
