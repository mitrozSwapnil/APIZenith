using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ZenithApp.Settings;
using ZenithApp.ZenithEntities;

namespace ZenithApp.ZenithRepository
{
    public class CustomerApplicationRepository : BaseRepository
    {
        private readonly IHttpContextAccessor _acc;
        private readonly IConfiguration _configuration;
        private readonly IMongoCollection<tbl_user> _user;
        private readonly IMongoCollection<tbl_customer_application> _customerapplication;

        public CustomerApplicationRepository(IOptions<MongoDbSettings> settings, IHttpContextAccessor accessor, IConfiguration configuration)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _customerapplication = database.GetCollection<tbl_customer_application>("tbl_customer_application");
            _acc = accessor;
            _configuration = configuration;
        }

        public void AddCustomer(tbl_customer_application customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            _customerapplication.InsertOne(customer);
        }
        protected override void Disposing()
        {
            throw new NotImplementedException();
        }
    }
}
