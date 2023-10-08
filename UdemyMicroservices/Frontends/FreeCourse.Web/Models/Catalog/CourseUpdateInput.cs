using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace FreeCourse.Web.Models.Catalog
{
    public class CourseUpdateInput
    {
        public string Id { get; set; }
        [Display(Name = "Kurs İsmi")]
        public string Name { get; set; }
        [Display(Name = "Kurs Açıklama")]
        public string Description { get; set; }
        [Display(Name = "Kurs Fiyat")]
        public decimal Price { get; set; }
        public string UserId { get; set; }
        public string Picture { get; set; }

        //bireçok ilişki categoryin birden çok kursu olabilir ama bir kursun sadece bir kategorisi olabilir
        [Display(Name = "Kurs Kategori")]
        public string CategoryId { get; set; }

        //birebir ilişki
        public FeatureViewModel Feature { get; set; }
        [Display(Name = "Kurs Resim")]
        public IFormFile PhotoFromFile { get; set; }
    }
}
