using Automacao.Models;
using MongoDB.Driver;

namespace Automacao.Services
{
    public class MongoService
    {
        private readonly IMongoCollection<SensorData> _collection;

        public MongoService(IConfiguration config)
        {
            var client = new MongoClient(config["MongoDb:ConnectionString"]);
            var db = client.GetDatabase(config["MongoDB:DatabaseName"]);
            _collection = db.GetCollection<SensorData>(config["MongoDB:CollectionName"]);
        }

        public async Task SaveDataAsync(SensorData data)
        {
            await _collection.InsertOneAsync(data);
        }

        public async Task<List<SensorData>> GetAllAsync() =>
            await _collection.Find(_ => true).ToListAsync();
    }
}
