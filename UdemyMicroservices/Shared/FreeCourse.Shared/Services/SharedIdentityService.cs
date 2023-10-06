using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreeCourse.Shared.Services
{
    //bu sınfı kullanabilmem içinde kullancağım microservisin startupunda servis olarak kaydetmem lazım dikakt.
    public class SharedIdentityService : ISharedIdentityService
    {
        //JWT içerisindeki datayı okuyabilmek için HttpContext üzerinden bana User propertysi üzerinden Claimler geliyor. Yani Framework zaten benim için kullanıcıya ait olan dataları okuyor. Bu kullanıcıya ait olan datalarada biz Claim nesnesi olarak adlandırıyoruz. Claim demek kullanıcı hakkında tutmuş olduğum data demek. Claimler key value şeklinde tutulur. Yani tokendaki sub bir claim nesnesi ise bu zaten ana  framework gidiyor herhangi bier microservsiimde istek yapıldığında gelen tokendan kullanıcıya ait olanları direkt olarak bir claim nesnesi olarak ekliyor. Bunları ise HttpContext nesnesine ekliyor. IHttpContextAccessor nesnesi üzerinden buna erişebiliriz ama bunu startupta mutlaka bir service olarak eklemem gerekiyor bu sayede HttpContext nesnesine IHttpContextAccessor interfacesi üzerinden erişebilicez.
        private IHttpContextAccessor _httpContextAccessor;
        public SharedIdentityService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        //key value, aslında burada type dediğimiz sub 'a denk geliyor value sub: sağdaki data 
        public string GetUserId => _httpContextAccessor.HttpContext.User.FindFirst("sub").Value;
    }
}
