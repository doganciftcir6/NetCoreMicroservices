namespace FreeCourse.Services.Catalog.Dtos
{
    public class CourseUpdateDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string UserId { get; set; }
        public string Picture { get; set; }

        //bireçok ilişki categoryin birden çok kursu olabilir ama bir kursun sadece bir kategorisi olabilir
        public string CategoryId { get; set; }

        //birebir ilişki
        public FeatureDto Feature { get; set; }
    }
}
