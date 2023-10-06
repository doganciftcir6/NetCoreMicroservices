using MongoDB.Bson.Serialization.Attributes;
using System;

namespace FreeCourse.Services.Catalog.Models
{
    public class Course
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }

        public string Name { get; set; }

        [BsonRepresentation(MongoDB.Bson.BsonType.Decimal128)]
        public decimal Price { get; set; }

        public string UserId { get; set; }
        public string Picture { get; set; }
        public string Description { get; set; }

        [BsonRepresentation(MongoDB.Bson.BsonType.DateTime)]
        public DateTime CreatedTime { get; set; }

        //bireçok ilişki categoryin birden çok kursu olabilir ama bir kursun sadece bir kategorisi olabilir
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string CategoryId { get; set; }
        //kod tarafında kullanacağım propun mongodb tarafında bir karşılığı olmasın o yüzden ignorela navigation propu ignoreluyoruz yani
        [BsonIgnore]
        public Category Category { get; set; }

        //birebir ilişki
        public Feature Feature { get; set; }
    }
}
