using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ZenithApp.Settings;
using ZenithApp.ZenithEntities;
using ZenithApp.ZenithMessage;

namespace ZenithApp.ZenithRepository
{
    public class MasterRepository 
    {
        private readonly IMongoCollection<tbl_master_certificates> _master;
        private readonly IMongoCollection<tbl_master_product_certificates> _productcertificate;
        private readonly IMongoCollection<tbl_Master_Remark> _masterremark;
        private readonly IMongoCollection<tbl_Master_Threat> _masterthreat;
        private readonly IMongoCollection<tbl_master_designation> _masterdesignation;
        private readonly IMongoCollection<tbl_master_Audit> _masterAudit;
        private readonly IMongoCollection<tbl_master_technicalArea> _technicalarea;

        private readonly IHttpContextAccessor _acc;

        public MasterRepository(IOptions<MongoDbSettings> settings , IHttpContextAccessor acc)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _master = database.GetCollection<tbl_master_certificates>("tbl_master_certificates");
            _productcertificate = database.GetCollection<tbl_master_product_certificates>("tbl_master_product_certificates");
            _masterremark = database.GetCollection<tbl_Master_Remark>("tbl_master_remarks");
            _masterthreat = database.GetCollection<tbl_Master_Threat>("tbl_master_threats");
            _masterdesignation = database.GetCollection<tbl_master_designation>("tbl_master_designation");
            _masterAudit = database.GetCollection<tbl_master_Audit>("tbl_master_audit");
            _technicalarea = database.GetCollection<tbl_master_technicalArea>("tbl_master_technicalArea");
            _acc = acc;
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
         public List<tbl_master_designation> GetAllMasterDesignation() =>
            _masterdesignation.Find(x => true).ToList();


        public void CreateMasterRemark(tbl_Master_Remark masterRemark) =>
          _masterremark.InsertOne(masterRemark);

        public void CreateMasterThreat(tbl_Master_Threat masterThreat) =>
            _masterthreat.InsertOne(masterThreat);

         public void CreateMasterDesignation(tbl_master_designation designation) =>
            _masterdesignation.InsertOne(designation);


       
        public async Task<BaseResponse> AddMasterAudit(MasterAuditRequest request)
        {
            var response = new BaseResponse();
            var UserId = _acc.HttpContext?.Session.GetString("UserId");
            if (request == null ||string.IsNullOrWhiteSpace(request.AuditName))
            {
                response.Message = "AuditName and AuditType are required.";
                response.HttpStatusCode = System.Net.HttpStatusCode.BadRequest;
                response.Success = false;
                response.ResponseCode = 1;
                return response;
            }

            var masterAudit = new tbl_master_Audit
            {
                AuditName = request.AuditName,
                CreatedBy = UserId,
                CreatedOn = DateTime.Now,
            };

            await _masterAudit.InsertOneAsync(masterAudit);

            response.Message = "Master audit added successfully.";
            response.HttpStatusCode = System.Net.HttpStatusCode.OK;
            response.Success = true;
            response.ResponseCode = 0;

            return response;
        }
        public async Task<BaseResponse> AddMasterTechnicalArea(masterTechnicalAreaRequest request)
        {
            var response = new BaseResponse();
            var UserId = _acc.HttpContext?.Session.GetString("UserId");
            if (request == null || string.IsNullOrWhiteSpace(request.TechnicalAreaName))
            {
                response.Message = "AuditName and AuditType are required.";
                response.HttpStatusCode = System.Net.HttpStatusCode.BadRequest;
                response.Success = false;
                response.ResponseCode = 1;
                return response;
            }

            var tech = new tbl_master_technicalArea
            {
                Name = request.TechnicalAreaName,
                CreatedBy = UserId,
                CreatedOn = DateTime.Now,
            };

            await _technicalarea.InsertOneAsync(tech);

            response.Message = "Master Tech Area added successfully.";
            response.HttpStatusCode = System.Net.HttpStatusCode.OK;
            response.Success = true;
            response.ResponseCode = 0;

            return response;
        }



    }
}
