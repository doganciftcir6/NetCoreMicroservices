using Dapper;
using FreeCourse.Shared.Dtos;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace FreeCourse.Services.Discount.Services
{
    public class DiscountService : IDiscountService
    {
        //postgresql'e bağlanalım
        //IConfiguration appsettings.json içindeki değerleri okumak için
        //IDbConnection dapper ile ilgili değildir herhahngi bir veritabanına
        //etkileşime geçmek istendiğinde kullanılır.
        private readonly IConfiguration _configuration;
        private readonly IDbConnection _dbConnection;
        public DiscountService(IConfiguration configuration)
        {
            _configuration = configuration;
            //GetConnectionString metotu otomatik ollarak appsettings içindeki ConnectionStrings değerini alır.
            _dbConnection = new NpgsqlConnection(_configuration.GetConnectionString("PostgreSql"));
        }

        public async Task<Response<NoContent>> DeleteAsync(int id)
        {
            var status = await _dbConnection.ExecuteAsync("delete from discount where id=@Id", new { Id = id });
            return status > 0 ? Response<NoContent>.Success(204) : Response<NoContent>.Fail("Discount not found", 404);

        }

        public async Task<Response<List<Models.Discount>>> GetAllAsync()
        {
            //QueryAsync dapper üzerinden gelir ve get işlemlerinde kullanılır.
            //Dapperin yaptığı olay dbden gelen veriyi model classımıza mapliyor bir classa mapliyor yani tüm olayı bu.
            //modelde [Dapper.Contrib.Extensions.Table("discount")] dediğimiz için tablo postgresqlde küçük harflerle var.
            var discounts = await _dbConnection.QueryAsync<Models.Discount>("Select * from discount");
            return Response<List<Models.Discount>>.Success(discounts.ToList(), 200);
        }

        public async Task<Response<Models.Discount>> GetByCodeAndUserIdAsync(string code, string userId)
        {
            //sorgum bittikten sonra sorguda kullandığım parametreleri isimsiz bir class üzerinmden dolduruyorum.
            var discount = await _dbConnection.QueryAsync<Models.Discount>("select * from discount where userid=@UserId and code=@Code", new { UserId = userId, Code = code });
            var hasDiscount = discount.FirstOrDefault();
            if (hasDiscount == null)
            {
                return Response<Models.Discount>.Fail("Discount not found", 404);
            }
            return Response<Models.Discount>.Success(hasDiscount, 200);
        }

        public async Task<Response<Models.Discount>> GetByIdAsync(int id)
        {
            var discount = (await _dbConnection.QueryAsync<Models.Discount>("Select * from discount where id=@Id", new { Id = id })).SingleOrDefault();
            if (discount == null)
            {
                return Response<Models.Discount>.Fail("Discount not found", 404);
            }
            return Response<Models.Discount>.Success(discount, 200);
        }

        public async Task<Response<NoContent>> SaveAsync(Models.Discount discount)
        {
            //istersek veir olarak direkt discountu verebiliriz dapper bu veri içinden otomati gerekli alanları alır istersek ise update'de olduğu gibi verileri tek tek biz belirtebiliriz isimsiz bir class üzerinden.
            var saveStatus = await _dbConnection.ExecuteAsync("INSERT INTO discount (userid,rate,code) VALUES(@UserId,@Rate,@Code)", discount);
            if (saveStatus > 0)
            {
                return Response<NoContent>.Success(204);
            }
            return Response<NoContent>.Fail("an error occurred while adding", 500);
        }

        public async Task<Response<NoContent>> UpdateAsync(Models.Discount discount)
        {
            var status = await _dbConnection.ExecuteAsync("update discount set userid=@UserId, code=@Code, rate=@Rate where id=@Id", new { Id = discount.Id, UserId = discount.UserId, Code = discount.Code, Rate = discount.Rate });
            if (status > 0)
            {
                return Response<NoContent>.Success(204);
            }
            return Response<NoContent>.Fail("Discount not found", 404);
        }
    }
}
