using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Net.NetworkInformation;
using ZenithApp.Settings;
using ZenithApp.ZenithEntities;

namespace ZenithApp.CommonServices
{
    public class MongoDbService
    {
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<tbl_master_certificates> _masterCertificate;
        private readonly IMongoCollection<tbl_user> _userlist;
        private readonly IMongoCollection<tbl_Status> _status;

        public MongoDbService(IMongoClient mongoClient, IOptions<MongoDbSettings> settings)
        {
            _database = mongoClient.GetDatabase(settings.Value.DatabaseName);
            _masterCertificate = _database.GetCollection<tbl_master_certificates>("tbl_master_certificates");
            _userlist = _database.GetCollection<tbl_user>("tbl_user");
            _status = _database.GetCollection<tbl_Status>("tbl_Status");
        }

        // Generic method to get the application collection dynamically
        public IMongoCollection<T> GetApplicationCollection<T>(string type) 
        {
            var collectionName = $"tbl_{type}_Application";
            return _database.GetCollection<T>(collectionName);
        }
        public string Getcertificatename(string certificateId)
        {
            var name = _masterCertificate.Find(x => x.Id == certificateId)?.FirstOrDefaultAsync().Result.Certificate_Name;
            return name;
        }
        public string ReviewerName(string reviewerId)
        {
            var name = _userlist.Find(x => x.Id == reviewerId)?.FirstOrDefaultAsync().Result.UserName;
            return name;
        }
        public string StatusName(string statusid)
        {
            var name = _status.Find(x => x.Id == statusid)?.FirstOrDefaultAsync().Result.StatusName;
            return name;
        }
    }
}
