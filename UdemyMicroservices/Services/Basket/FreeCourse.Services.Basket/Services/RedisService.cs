using StackExchange.Redis;

namespace FreeCourse.Services.Basket.Services
{
    public class RedisService
    {
        //veritabanı ile haberleşicez o yüzden bir host birde porta ihtiyaç var, birde redisle bağlantı kurabilmek için ConnectionMultiplexer sınıfını kullanıyoruz.
        private readonly string _host;
        private readonly int _port;
        private ConnectionMultiplexer _connectionMultiplexer;
        public RedisService(string host, int port)
        {
            _host = host;
            _port = port;
        }

        //redisle bağlantı kuralım, bu metot bize bir bağlantı versin
        public void Connect() => _connectionMultiplexer = ConnectionMultiplexer.Connect($"{_host}:{_port}");
        //birde bize veritabanı veren bir metot yapalım, redis default olarak 10 15 tane hazır veritabanı geliyor db0 db1 db2 bunlardan bir tanesini tercih edeceğiz. Birden fazla olma sebebei bir dbyi test için kullan birini develop mode birini production için kullan vs diye. Biz 1.sini tercih edelim.
        public IDatabase GetDb(int db = 1) => _connectionMultiplexer.GetDatabase(db);

    }
}
