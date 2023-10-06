using FreeCourse.Shared.Dtos;
using FreeCourse.Web.Helpers;
using FreeCourse.Web.Models;
using FreeCourse.Web.Models.Catalog;
using FreeCourse.Web.Services.Interface;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace FreeCourse.Web.Services
{
    public class CatalogService : ICatalogService
    {
        //catalog microservise istek yapabilmek için HttpClient nesnesi gerekli
        private readonly HttpClient _httpClient;
        private readonly IPhotoStockService _photoStockService;
        private readonly PhotoHelper _photoHelper;
        public CatalogService(HttpClient httpClient, IPhotoStockService photoStockService, PhotoHelper photoHelper)
        {
            _httpClient = httpClient;
            _photoStockService = photoStockService;
            _photoHelper = photoHelper;
        }

        public async Task<bool> CreateCourseAsync(CourseCreateInput courseCreateInput)
        {
            //kurs fotoğrafı kaydetne kısmı
            var resultPhotoService = await _photoStockService.UploadPhoto(courseCreateInput.PhotoFromFile);
            if(resultPhotoService != null)
            {
                courseCreateInput.Picture = resultPhotoService.Url;
            }
            //PostAsJsonAsync() metotu sayesinde kullanıcıdan alınan değer json değere çevrilip istek yapılır.
            //gönderdiğim datayı direkt olarak serlize yapsın jsona ve apiye json bilgi göndersin.
            //PostAsJsonAsync ile
            var response = await _httpClient.PostAsJsonAsync<CourseCreateInput>("courses", courseCreateInput);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteCourseAsync(string courseId)
        {
            var response = await _httpClient.DeleteAsync($"courses/{courseId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<CategoryViewModel>> GetAllCategoriesAsync()
        {
            var response = await _httpClient.GetAsync("categories");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            //bize response olarak gelen dtoyu viewmodelimize dönüştürelim, Deserilize işlemi.
            var responseSuccess = await response.Content.ReadFromJsonAsync<Response<List<CategoryViewModel>>>();
            return responseSuccess.Data;
        }

        public async Task<List<CourseViewModel>> GetAllCourseAsync()
        {
            //buraya sadece microservisteki controllerin ismini yazıyorum
            //baseurli zaten startup tarafında httpclient nesnesine tanımlamıştık
            //http://localhost:5000/services/catalog/courses
            var response = await _httpClient.GetAsync("courses");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            //bize response olarak gelen dtoyu viewmodelimize dönüştürelim Deserilize işlemi
            var responseSuccess = await response.Content.ReadFromJsonAsync<Response<List<CourseViewModel>>>();
            //direkt mongodbden pictureismi ve uzantı bilgisi geliyordu biz bu bilgiyi 
            //PhotoStock microservisten alacak şekilde çevirelim
            //yani resim bilgisi aslında dbden değil microservisten gelecek clienta
            responseSuccess.Data.ForEach(x =>
            {
                x.StockPictureUrl = _photoHelper.GetPhotoStockUrl(x.Picture);
            });
            return responseSuccess.Data;
        }

        public async Task<List<CourseViewModel>> GetAllCourseByUserIdAsync(string userId)
        {
            var response = await _httpClient.GetAsync($"courses/GetAllByUserId/{userId}");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            //bize response olarak gelen dtoyu viewmodelimize dönüştürelim
            var responseSuccess = await response.Content.ReadFromJsonAsync<Response<List<CourseViewModel>>>();
            responseSuccess.Data.ForEach(x =>
            {
                x.StockPictureUrl = _photoHelper.GetPhotoStockUrl(x.Picture);
            });
            return responseSuccess.Data;
        }

        public async Task<CourseViewModel> GetByCourseId(string courseId)
        {
            var response = await _httpClient.GetAsync($"courses/{courseId}");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            //bize response olarak gelen dtoyu viewmodelimize dönüştürelim
            var responseSuccess = await response.Content.ReadFromJsonAsync<Response<CourseViewModel>>();
            responseSuccess.Data.StockPictureUrl = _photoHelper.GetPhotoStockUrl(responseSuccess.Data.Picture);
            return responseSuccess.Data;
        }

        public async Task<bool> UpdateCourseAsync(CourseUpdateInput courseUpdateInput)
        {
            //kurs fotoğrafı kaydetne kısmı
            var resultPhotoService = await _photoStockService.UploadPhoto(courseUpdateInput.PhotoFromFile);
            if (resultPhotoService != null)
            {
                //yeni resmi kayıt etmeden önce eski bulunan resmi silelim
                await _photoStockService.DeletePhoto(courseUpdateInput.Picture);
                courseUpdateInput.Picture = resultPhotoService.Url;
            }
            //PutAsJsonAsync() metotu sayesinde kullanıcıdan alınan değer json değere çevrilip put istek yapılır.
            //gönderdiğim datayı direkt olarak serlize yapsın jsona ve apiye json bilgi göndersin.
            //PutAsJsonAsync ile
            var response = await _httpClient.PutAsJsonAsync<CourseUpdateInput>("courses", courseUpdateInput);
            return response.IsSuccessStatusCode;
        }
    }
}
