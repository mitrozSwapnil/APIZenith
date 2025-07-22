using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ZenithApp.Settings;
using ZenithApp.ZenithEntities;

namespace ZenithApp.ZenithServices
{
    public class AddService
    {
        private readonly IMongoCollection<tbl_user> _user;

        public AddService(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _user = database.GetCollection<tbl_user>("tbl_user");
        }

        public tbl_user RegisterCustomer(string emailOrMobile, string reviewerRoleId)
        {
            var user = new tbl_user
            {
                FullName = "",
                EmailId = emailOrMobile.Contains("@") ? emailOrMobile : null,
                ContactNo = !emailOrMobile.Contains("@") ? emailOrMobile : null,
                Password = "", // Default Password (never used)
                Fk_RoleID = "686fc53af41f7edee9b89cd7",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                CreatedBy = "System",
                UpdatedBy = "System",
                IsDelete = 0
            };

            _user.InsertOne(user);
            return user;
        }

       // public List<tbl_customer_application> GetAllUsers() => _user.Find(u => true).ToList();

        public void Create(tbl_user user) =>
           _user.InsertOne(user);
    }
}
