using FreeCourse.Shared.Dtos;
using FreeCourse.Web.Models.PhotoStocks;
using FreeCourse.Web.Services.Interface;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace FreeCourse.Web.Services
{
    public class PhotoStockService : IPhotoStockService
    {
        private readonly HttpClient _httpClient;
        public PhotoStockService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> DeletePhoto(string photoUrl)
        {
            //query param olarak göndermemiz lazım veriyi, microservise
            var response = await _httpClient.DeleteAsync($"photos?photoUrl={photoUrl}");
            return response.IsSuccessStatusCode;
        }

        public async Task<PhotoStockViewModel> UploadPhoto(IFormFile photo)
        {
            if (photo == null || photo.Length <= 0)
            {
                return null;
            }
            //örnek dosya ismi = 435435435435.jpg
            var randomFileName = $"{Guid.NewGuid().ToString()}{Path.GetExtension(photo.FileName)}";
            using var ms = new MemoryStream();
            await photo.CopyToAsync(ms);
            //dosyayı isteğin bodysine ekle
            var multipartContent = new MultipartFormDataContent();
            //buradaki photo ismi benim microserviste controllerdaki parametrede olan IFormFile photo ya denk geliyor. Postmanda istek atarken key kısmına photo yazmamız gibi mantığı yani.
            //bu resim dosyasını array olarak veriyoruz ve bunu bir Content olarak oluşturuyoruz.
            multipartContent.Add(new ByteArrayContent(ms.ToArray()), "photo", randomFileName);
            //artık isteği at
            var response = await _httpClient.PostAsync("photos", multipartContent);
            if (!response.IsSuccessStatusCode)
            {
                //burada loglama yapılabilir.
                return null;
            }
            //gelen cevabı PhotoStockViewModel'e deserliaze yaparak geri dönelim
            var responseSuccess = await response.Content.ReadFromJsonAsync<Response<PhotoStockViewModel>>();
            return responseSuccess.Data;
        }
    }
}
