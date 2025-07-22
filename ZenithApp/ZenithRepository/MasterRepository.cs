using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ZenithApp.Settings;
using ZenithApp.ZenithEntities;

namespace ZenithApp.ZenithRepository
{
    public class MasterRepository 
    {
        private readonly IMongoCollection<tbl_master_certificates> _master;
        private readonly IMongoCollection<tbl_master_product_certificates> _productcertificate;
        private readonly IMongoCollection<tbl_Master_Remark> _masterremark;
        private readonly IMongoCollection<tbl_Master_Threat> _masterthreat;

        public MasterRepository(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _master = database.GetCollection<tbl_master_certificates>("tbl_master_certificates");
            _productcertificate = database.GetCollection<tbl_master_product_certificates>("tbl_master_product_certificates");
            _masterremark = database.GetCollection<tbl_Master_Remark>("tbl_master_remarks");
            _masterthreat = database.GetCollection<tbl_Master_Threat>("tbl_master_threats");
        }

        public List<tbl_master_certificates> GetAll() => _master.Find(s => true).ToList();
        public List<tbl_master_product_certificates> GetAllCertificates() => _productcertificate.Find(s => true).ToList();



        public void CreateCertificate(tbl_master_certificates master_Certificates) =>
            _master.InsertOne(master_Certificates);
        public void CreateProductCertificate(tbl_master_product_certificates productCertificate) =>
         _productcertificate.InsertOne(productCertificate);

        public List<tbl_Master_Remark> GetAllMasterRemarks() =>
    _masterremark.Find(x => true).ToList();

        public List<tbl_Master_Threat> GetAllMasterThreats() =>
            _masterthreat.Find(x => true).ToList();


        public void CreateMasterRemark(tbl_Master_Remark masterRemark) =>
          _masterremark.InsertOne(masterRemark);

        public void CreateMasterThreat(tbl_Master_Threat masterThreat) =>
            _masterthreat.InsertOne(masterThreat);


    }
}
