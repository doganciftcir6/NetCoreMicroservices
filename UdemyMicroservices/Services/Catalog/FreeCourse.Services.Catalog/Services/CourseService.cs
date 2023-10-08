using AutoMapper;
using FreeCourse.Services.Catalog.Dtos;
using FreeCourse.Services.Catalog.Models;
using FreeCourse.Services.Catalog.Settings;
using FreeCourse.Shared.Dtos;
using FreeCourse.Shared.Messages;
using MassTransit;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FreeCourse.Services.Catalog.Services
{
    public class CourseService : ICourseService
    {
        private readonly IMongoCollection<Course> _courseCollection;
        private readonly IMongoCollection<Category> _categoryCollection;
        private readonly IMapper _mapper;
        //rabbitmqya event gönderceğiz
        private readonly IPublishEndpoint _publishEndpoint;

        public CourseService(IMapper mapper, IDatabaseSettings databaseSettings, IPublishEndpoint publishEndpoint)
        {
            //mongodbye bağlan
            var client = new MongoClient(databaseSettings.ConnectionString);
            //veritabanına bağlan
            var database = client.GetDatabase(databaseSettings.DatabaseName);

            //ilgili databasedeki koleksiyonu al
            _courseCollection = database.GetCollection<Course>(databaseSettings.CourseCollectionName);
            //bunu categoryservisten de alabilirdik oluşturduğumuz ama ileride db ile ilgili başka işlemlerde yapabilirim o yüzden direkt veritabanından alalım
            _categoryCollection = database.GetCollection<Category>(databaseSettings.CategoryCollectionName);
            _mapper = mapper;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Shared.Dtos.Response<List<CourseDto>>> GetAllAsync()
        {
            //burada ilişkisel bir veritabanı kullansaydık efcore tarafında join kullanabilirdik, include metotlarını kullanabilirdik lazyloading eagerloading bir sürü yöntemle beraber kursa bağlı categoryide alabilirdik. Nosql veritabanında böyle bir şey yok benim kendim eklemem lazım. CourseDto dönerken aynı zamanda içinde bulunan CategoryDtonunda dolu olmasını istiyorum elimle eklicem dönen veriye.
            var courses = await _courseCollection.Find(course => true).ToListAsync();

            //herhangi bir course verisi datası varsa
            if (courses.Any())
            {
                foreach (var course in courses)
                {
                    //course datasınında CategoryId verisi zaten dolu geliyor onu kullanıcaz filtrelemede mutlaka kategorisi olmak durumunda o yüzden firstordefault kullanmayalım.
                    course.Category = await _categoryCollection.Find(x => x.Id == course.CategoryId).FirstAsync();
                }
            }
            else
            {
                //data yoksa boş bir tane Course listesi bizim için oluştursun
                courses = new List<Course>() { };
            }

            return Shared.Dtos.Response<List<CourseDto>>.Success(_mapper.Map<List<CourseDto>>(courses), 200);
        }

        public async Task<Shared.Dtos.Response<CourseDto>> GetByIdAsync(string id)
        {
            var course = await _courseCollection.Find<Course>(course => course.Id == id).FirstOrDefaultAsync();
            if (course == null)
            {
                return Shared.Dtos.Response<CourseDto>.Fail("Course not found", 404);
            }
            course.Category = await _categoryCollection.Find<Category>(x => x.Id == course.CategoryId).FirstAsync();
            return Shared.Dtos.Response<CourseDto>.Success(_mapper.Map<CourseDto>(course), 200);
        }

        //Userıd ye göre kullanıcının kurslarını göstermek için
        public async Task<Shared.Dtos.Response<List<CourseDto>>> GetAllByUserIdAsync(string userId)
        {
            var courses = await _courseCollection.Find<Course>(x => x.UserId == userId).ToListAsync();
            //herhangi bir course verisi datası varsa
            if (courses.Any())
            {
                foreach (var course in courses)
                {
                    //course datasınında CategoryId verisi zaten dolu geliyor onu kullanıcaz filtrelemede mutlaka kategorisi olmak durumunda o yüzden firstordefault kullanmayalım.
                    course.Category = await _categoryCollection.Find(x => x.Id == course.CategoryId).FirstAsync();
                }
            }
            else
            {
                //data yoksa boş bir tane Course listesi bizim için oluştursun
                courses = new List<Course>() { };
            }
            return Shared.Dtos.Response<List<CourseDto>>.Success(_mapper.Map<List<CourseDto>>(courses), 200);
        }

        public async Task<Shared.Dtos.Response<CourseDto>> CreateAsync(CourseCreateDto courseCreateDto)
        {
            var newCourse = _mapper.Map<Course>(courseCreateDto);
            newCourse.CreatedTime = DateTime.Now;
            await _courseCollection.InsertOneAsync(newCourse);
            return Shared.Dtos.Response<CourseDto>.Success(_mapper.Map<CourseDto>(newCourse), 200);
        }

        //UPDATE sonrasında aynı nesneyi dönemin bir anlamı yok zaten kullanıcı veriyor bana bu nesneyi o yüzden no content döndük
        public async Task<Shared.Dtos.Response<NoContent>> UpdateAsync(CourseUpdateDto courseUpdateDto)
        {
            var updateCourse = _mapper.Map<Course>(courseUpdateDto);
            var result = await _courseCollection.FindOneAndReplaceAsync(x => x.Id == courseUpdateDto.Id, updateCourse);
            if (result == null)
            {
                return Shared.Dtos.Response<NoContent>.Fail("Course not found", 404);
            }
            //rabbitmqya event gönder
            //kuyruk ismi belirlememe gerek yok çünkü bu bir event kuyruğa göndermeyeceğiz
            //bu yaptığımız exchange'e gidecek, exchange'e bir kuyruk oluşturarak subricbe olan microservislerim olacak order mesela
            //burada catalog bir event fırlattığında Orderin oluşturmuş olduğu kuyruğa düşecek.
            await _publishEndpoint.Publish<CourseNameChangedEvent>(new CourseNameChangedEvent { CourseId = updateCourse.Id, UpdatedName = courseUpdateDto.Name});
            return Shared.Dtos.Response<NoContent>.Success(204);
        }

        public async Task<Shared.Dtos.Response<NoContent>> DeleteAsync(string id)
        {
            var result = await _courseCollection.DeleteOneAsync(x => x.Id == id);
            if (result.DeletedCount > 0)
            {
                //silmiş gerçekten
                return Shared.Dtos.Response<NoContent>.Success(204);
            }
            return Shared.Dtos.Response<NoContent>.Fail("Course not found", 404);
        }
    }
}
