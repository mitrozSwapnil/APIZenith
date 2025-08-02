using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using ZenithApp.CommonServices;
using ZenithApp.Settings;
using ZenithApp.ZenithEntities;
using ZenithApp.ZenithMessage;

namespace ZenithApp.ZenithRepository
{
    public class QuotationRepository : BaseRepository
    {
        private readonly IMongoCollection<tbl_customer_application> _customer;
        private readonly IMongoCollection<tbl_User_Role> _role;
        private readonly IMongoCollection<tbl_Status> _status;
        private readonly IMongoCollection<tbl_user> _user;
        private readonly IMongoCollection<tbl_master_certificates> _masterCertificate;
        private readonly IMongoCollection<tbl_customer_certificates> _customercertificates;
        private readonly IMongoCollection<tbl_customer_key_personnels> _customerKeyPersonnel;
        private readonly IMongoCollection<tbl_customer_site> _customersite;
        private readonly IMongoCollection<tbl_customer_Entity> _customerentity;
        private readonly IMongoCollection<tbl_ISO_Application> _isoApplication;
        private readonly IMongoCollection<tbl_Reviewer_Audit_ManDays> _reviewerAuditManDays;
        private readonly IMongoCollection<tbl_quoatation> _quoatation;

        private readonly MongoDbService _mongoDbService;

        

        private readonly IHttpContextAccessor _acc;

        public QuotationRepository(IOptions<MongoDbSettings> settings, IHttpContextAccessor acc, MongoDbService mongoDbService)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _customer = database.GetCollection<tbl_customer_application>("tbl_customer_application");
            _role = database.GetCollection<tbl_User_Role>("tbl_User_Role");
            _user = database.GetCollection<tbl_user>("tbl_user");
            _masterCertificate = database.GetCollection<tbl_master_certificates>("tbl_master_certificates");
            _customercertificates = database.GetCollection<tbl_customer_certificates>("tbl_customer_certificates");
            _customerKeyPersonnel = database.GetCollection<tbl_customer_key_personnels>("tbl_customer_key_personnels");
            _customersite = database.GetCollection<tbl_customer_site>("tbl_customer_site");
            _customerentity = database.GetCollection<tbl_customer_Entity>("tbl_customer_Entity");
            _status = database.GetCollection<tbl_Status>("tbl_Status");
            _isoApplication = database.GetCollection<tbl_ISO_Application>("tbl_ISO_Application");
            _reviewerAuditManDays = database.GetCollection<tbl_Reviewer_Audit_ManDays>("tbl_reviewer_audit_mandays");
            _quoatation = database.GetCollection<tbl_quoatation>("tbl_quoatation");
            _acc = acc;
            _mongoDbService = mongoDbService;
        }



        public async Task<createQuotationResponse> CreateQuotation(createQuotationRequest request)
        {
            var response = new createQuotationResponse();
            var userId = _acc.HttpContext?.Session.GetString("UserId");
            var userFkRole = (await _user.Find(x => x.Id == userId).FirstOrDefaultAsync())?.Fk_RoleID;
            var usertype = (await _role.Find(x => x.Id == userFkRole).FirstOrDefaultAsync())?.roleName;

            if (usertype?.Trim().ToLower() == "admin")
            {
                try
                {
                    var entity = new tbl_quoatation
                    {
                        ApplicationId = request.ApplicationId,
                        QuotationId = GenerateQuotationId(),
                        CertificateType = request.CertificateType,
                        CreatedOn = request.CreatedOn,
                        Currency = "INR", // or get from request if needed
                        QuotationData = BsonDocument.Parse(JsonSerializer.Serialize(request.QuotationData))
                    };

                    await _quoatation.InsertOneAsync(entity); 

                    response.Message = "Quotation saved successfully.";
                    response.Success = true;
                    response.HttpStatusCode = System.Net.HttpStatusCode.OK;
                    response.ResponseCode = 0;
                }
                catch (Exception ex)
                {

                    response.Message = "Exception: " + ex.Message + " | StackTrace: " + ex.StackTrace;
                    response.Success = false;
                    response.HttpStatusCode = System.Net.HttpStatusCode.InternalServerError;
                    response.ResponseCode = 1;
                }

            }
            else
            {
                response.Message = "Invalid Token.";
                response.Success = false;
                response.HttpStatusCode = System.Net.HttpStatusCode.Unauthorized;
                response.ResponseCode = 1;
            }
            return response;
        }




        public string GenerateQuotationId()
        {
            string currentYear = DateTime.Now.ToString("yyyy");
            string currentMonth = DateTime.Now.ToString("MM");

            // Pattern: Q/001 - MM/YYYY
            var regex = new Regex(@"Q\/(\d{3})-\d{2}/" + Regex.Escape(currentYear));

            // Get latest quotation with current year (ignore month)
            var latestQuotation = _quoatation
                .Find(q => q.QuotationId.EndsWith(currentYear))
                .SortByDescending(q => q.QuotationId)
                .FirstOrDefault();

            int nextSerial = 1;

            if (latestQuotation != null)
            {
                var match = regex.Match(latestQuotation.QuotationId);
                if (match.Success)
                {
                    int lastSerial = int.Parse(match.Groups[1].Value);
                    nextSerial = lastSerial + 1;
                }
            }

            string serial = nextSerial.ToString("D3"); // pad to 3 digits
            return $"Q/{serial}-{currentMonth}/{currentYear}";
        }



        public async Task<getCretificationsbyAppIdResponse> GetMandaysbyapplicationId(getmandaysbyapplicationIdRequest request)
        {
            var response = new getCretificationsbyAppIdResponse();
            var userId = _acc.HttpContext?.Session.GetString("UserId");
            var userFkRole = (await _user.Find(x => x.Id == userId).FirstOrDefaultAsync())?.Fk_RoleID;
            var usertype = (await _role.Find(x => x.Id == userFkRole).FirstOrDefaultAsync())?.roleName;

            if (usertype?.Trim().ToLower() == "admin")
            {
                try
                {
                    var certificate = _masterCertificate
                        .Find(x => x.Id == request.CertificateTypeId)
                        .FirstOrDefault();

                    if (certificate == null)
                        throw new Exception("Certificate Type not found.");

                    var tblCollection = _mongoDbService.GetApplicationCollection<BsonDocument>(certificate.Certificate_Name);
                    if (!ObjectId.TryParse(request.ApplicationId, out var objectId))
                        throw new Exception("Invalid Application ID format.");
                    var filter = Builders<BsonDocument>.Filter.Eq("_id", objectId);


                    var result = tblCollection.Find(filter).FirstOrDefault();

                    response.Message = "Quotation saved successfully.";
                    response.Success = true;
                    response.HttpStatusCode = System.Net.HttpStatusCode.OK;
                    response.ResponseCode = 0;
                }
                catch (Exception ex)
                {
                    response.Message = "Exception: " + ex.Message + " | StackTrace: " + ex.StackTrace;
                    response.Success = false;
                    response.HttpStatusCode = System.Net.HttpStatusCode.InternalServerError;
                    response.ResponseCode = 1;
                }
            }
            else
            {
                response.Message = "Invalid Token.";
                response.Success = false;
                response.HttpStatusCode = System.Net.HttpStatusCode.Unauthorized;
                response.ResponseCode = 1;
            }
            return response;
        }


        protected override void Disposing()
        {
           // throw new NotImplementedException();
        }

        
    }
}
