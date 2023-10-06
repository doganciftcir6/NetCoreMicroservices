using FreeCourse.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeCourse.Shared.ControllerBases
{
    public class CustomBaseController : ControllerBase
    {
        public IActionResult CreateActionResultInstance<T>(Response<T> response)
        {
            //ObjectResult tüm hani geriye döndüğümüz OK Created vs varya hepsi olabilir. Dönüş tipine responseumu datamı veriyorum ama bu arkadaşın statuscodu benim responsetan gelen statuscode olsun diyorum.
            //artık burası responsetan 404 geldiyse geriye 404 dönecek responseun içerisine bodyide gömecek.
            return new ObjectResult(response)
            {
                StatusCode = response.StatusCode,
            };
        }
    }
}
