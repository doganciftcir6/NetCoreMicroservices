using FreeCourse.Web.Models;
using FreeCourse.Web.Services.Interface;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace FreeCourse.Web.Services
{
    public class UserService : IUserService
    {
        //apideki endpointe istek yapabilmek için HttpClient lazım.
        private readonly HttpClient _httpClient;
        public UserService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<UserViewModel> GetUser()
        {
            //Deserliza işlemi ile json datayı sınıfımıza UserViewModel'e atacağız.
            //BaseUriStartuptan geliyor.
            //kullanıcının tokenini httpClient içerisine eklemeliyim IdentityServera istek yaparken.
            return await _httpClient.GetFromJsonAsync<UserViewModel>("/api/user/getuser");
        }
    }
}
