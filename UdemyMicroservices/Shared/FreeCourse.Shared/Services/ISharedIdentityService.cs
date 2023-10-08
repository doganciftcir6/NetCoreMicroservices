using System;
using System.Collections.Generic;
using System.Text;

namespace FreeCourse.Shared.Services
{
    public interface ISharedIdentityService
    {
        //bu prop üzerinden login olmuş tokendaki userıd bilgisine erişeceğiz
        public string GetUserId { get; }
    }
}
