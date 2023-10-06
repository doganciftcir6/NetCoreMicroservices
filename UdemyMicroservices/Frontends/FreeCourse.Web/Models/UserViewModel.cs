using System.Collections;
using System.Collections.Generic;

namespace FreeCourse.Web.Models
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string City { get; set; }

        //bu propları bana yield keywordu ile tek tek dönecek bir metot olsun
        public IEnumerable<string> GerUserProps()
        {
            yield return UserName;
            yield return Email;
            yield return City;
        }
    }
}
