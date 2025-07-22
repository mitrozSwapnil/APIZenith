using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ZenithApp.Settings;
using ZenithApp.ZenithEntities;

namespace ZenithApp.Services
{
    public class AuthService
    {
        private readonly IMongoCollection<tbl_user> _user;

        public AuthService(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _user = database.GetCollection<tbl_user>("tbl_user");
        }

        

        public List<tbl_user> GetAllUsers() => _user.Find(u => true).ToList();

        public void Create(tbl_user user) =>
           _user.InsertOne(user);
    }
}
