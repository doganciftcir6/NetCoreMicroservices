using System;

namespace FreeCourse.Web.Models.Catalog
{
    public class CourseViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string UserId { get; set; }
        public string Picture { get; set; }
        public string StockPictureUrl { get; set; }
        public string Description { get; set; }
        //Description alanı çok uzun olabilir o yüzden böyle ek bir prop koyabiliriz
        public string ShortDescription { get => Description.Length > 100 ? Description.Substring(0, 100) + "..." : Description; }
        public DateTime CreatedTime { get; set; }

        //bireçok ilişki categoryin birden çok kursu olabilir ama bir kursun sadece bir kategorisi olabilir
        public string CategoryId { get; set; }
        public CategoryViewModel Category { get; set; }

        //birebir ilişki
        public FeatureViewModel Feature { get; set; }
    }
}
