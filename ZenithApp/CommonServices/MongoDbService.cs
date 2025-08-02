using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ZenithApp.Settings;

namespace ZenithApp.CommonServices
{
    public class MongoDbService
    {
        private readonly IMongoDatabase _database;

        public MongoDbService(IMongoClient mongoClient, IOptions<MongoDbSettings> settings)
        {
            _database = mongoClient.GetDatabase(settings.Value.DatabaseName);
        }

        // Generic method to get the application collection dynamically
        public IMongoCollection<T> GetApplicationCollection<T>(string type) 
        {
            var collectionName = $"tbl_{type}_Application";
            return _database.GetCollection<T>(collectionName);
        }
    }
}
