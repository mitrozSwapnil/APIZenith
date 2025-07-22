using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Net;
using ZenithApp.Settings;
using ZenithApp.ZenithEntities;
using ZenithApp.ZenithMessage;

namespace ZenithApp.ZenithRepository
{
    public class ReviwerRepository: BaseRepository
    {
        private readonly IMongoCollection<tbl_customer_application> _customer;
        private readonly IMongoCollection<tbl_User_Role> _role;
        private readonly IMongoCollection<tbl_user> _user;
        private readonly IMongoCollection<tbl_master_certificates> _masterCertificate;
        private readonly IMongoCollection<tbl_customer_certificates> _customercertificates;
        private readonly IMongoCollection<tbl_customer_key_personnels> _customerKeyPersonnel;
        private readonly IMongoCollection<tbl_customer_site> _customersite;
        private readonly IMongoCollection<tbl_customer_Entity> _customerentity;
        private readonly IMongoCollection<tbl_Status> _status;
        private readonly IMongoCollection<tbl_ISO_Application> _iso;
        private readonly IMongoCollection<tbl_Application_Remark> _remark;
        private readonly IMongoCollection<tbl_Application_Threat> _threat;
        private readonly IMongoCollection<tbl_Master_Threat> _masterthreat;
        private readonly IMongoCollection<tbl_Master_Remark> _masterremark;







        private readonly IHttpContextAccessor _acc;

        public ReviwerRepository(IOptions<MongoDbSettings> settings, IHttpContextAccessor acc)
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
            _iso = database.GetCollection<tbl_ISO_Application>("tbl_ISO_Application");
            _remark = database.GetCollection<tbl_Application_Remark>("tbl_Application_Remark");
            _threat = database.GetCollection<tbl_Application_Threat>("tbl_Application_Threat");
            _masterthreat = database.GetCollection<tbl_Master_Threat>("tbl_Master_Threat");
            _acc = acc;
        }



        public async Task<getDashboardResponse> GetReviewerDashboard(getDashboardRequest request)
        {
            getDashboardResponse response = new getDashboardResponse();
            var UserId = _acc.HttpContext?.Session.GetString("UserId");
            var userFkRole = _user.Find(x => x.Id == UserId).FirstOrDefault()?.Fk_RoleID;
            var department = _user.Find(x => x.Id == UserId).FirstOrDefault()?.Department;
            var usertype = _role.Find(x => x.Id == userFkRole).FirstOrDefault()?.roleName;

            if (usertype?.Trim().ToLower() == "reviewer")
            {
                try
                {
                    List<CustomerDashboardData> dashboardList = new List<CustomerDashboardData>();
                    if(department?.Trim().ToLower() == "iso")
                    {
                        // Fetch all applications assigned to the user in the certification department
                        var applications = _iso
                            .Find(x => x.IsDelete == false && x.Fk_UserId == UserId)
                            .ToList();
                        foreach (var app in applications)
                        {
                            var masterCert = _masterCertificate.Find(x => x.Id == app.Fk_Certificate).FirstOrDefault();
                            if (masterCert != null)
                            {
                                var dashboardRecord = new CustomerDashboardData
                                {
                                    Id = app.Id,
                                    ApplicationId = app.ApplicationId,
                                    CompanyName = app.Orgnization_Name,
                                    Certification_Name = masterCert.Certificate_Name,
                                    Status = app.Application_Status,
                                    ReceiveDate = app.Application_Received_date,
                                    AssignedUserName = _user.Find(x => x.Id == app.Fk_UserId).FirstOrDefault()?.FullName
                                };
                                dashboardList.Add(dashboardRecord);
                            }
                        }
                    }




                    response.Data = dashboardList;
                    response.Message = "Dashboard fetched successfully.";
                    response.HttpStatusCode = System.Net.HttpStatusCode.OK;
                    response.Success = true;
                    response.ResponseCode = 0;
                }
                catch (Exception ex)
                {
                    response.Message = "GetCustomerDashboard Exception: " + ex.Message;
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


        public getReviewerApplicationResponse GetReviewerApplication(getReviewerApplicationRequest request)
        {
            getReviewerApplicationResponse response = new getReviewerApplicationResponse();
            try
            {
                var UserId = _acc.HttpContext?.Session.GetString("UserId");
                var userFkRole = _user.Find(x => x.Id == UserId).FirstOrDefault()?.Fk_RoleID;
                var userType = _role.Find(x => x.Id == userFkRole).FirstOrDefault()?.roleName;

                if (userType?.Trim().ToLower() == "reviewer")
                {
                    if (string.IsNullOrWhiteSpace(request.applicationId))
                    {
                        response.Message = "ApplicationId is required.";
                        response.HttpStatusCode = System.Net.HttpStatusCode.BadRequest;
                        response.Success = false;
                        response.ResponseCode = 1;
                        return response;
                    }

                    var app = _iso
                        .Find(x => x.Id == request.applicationId && x.IsDelete == false)
                        .FirstOrDefault();

                    var status = _status
                        .Find(x => x.Id == app.Status && x.IsDelete == false)
                        .FirstOrDefault().StatusName;

                    if (app == null)
                    {
                        response.Message = "No data found for the given ApplicationId.";
                        response.HttpStatusCode = System.Net.HttpStatusCode.NotFound;
                        response.Success = false;
                        response.ResponseCode = 1;
                        return response;
                    }
                    if (status != null && status == "SendToApprove")
                    {

                    }

                    var result = new ReviewerApplicationData
                    {
                        Id = app.Id,
                        ApplicationId = app.ApplicationId,
                        Orgnization_Name = app.Orgnization_Name,
                        Application_Received_date = app.Application_Received_date,
                        Constituation_of_Orgnization = app.Constituation_of_Orgnization,
                        Certification_Name = _masterCertificate
                            .Find(x => x.Id == app.Fk_Certificate && x.Is_Delete == false)
                            .FirstOrDefault()?.Certificate_Name,
                        Audit_Type = app.Audit_Type,
                        Scop_Of_Certification = app.Scop_of_Certification,
                        Technical_Areas = app.Technical_Areas,
                        Accreditations = app.Accreditations,
                        Availbility_of_TechnicalAreas = app.Availbility_of_TechnicalAreas,
                        Availbility_of_Auditor = app.Availbility_of_Auditor,
                        Audit_Lang = app.Audit_Lang,
                        Is_Interpreter = app.IsInterpreter,
                        Is_Multisite_Sampling = app.IsMultisitesampling,
                        Total_Site = app.Total_site,
                        Sample_Site = app.Sample_Site,
                        Shift_Details = app.Shift_Details,
                        status = status,
                        AssignUser = app.Fk_UserId,
                        CustomerSites = app.CustomerSites,
                        KeyPersonnels = app.KeyPersonnels,
                        MandaysLists = app.MandaysLists,
                        ThreatLists = _threat
                            .Find(x => x.Fk_ApplicationId == app.Id)
                            .ToList()
                            .Select(entity => new ThreatList
                            {
                                ThreatId = entity.Id,
                                Threar_name = _masterthreat
                                    .Find(x => x.Id == entity.Fk_MasterThreat && x.Fk_Certificate== "68760495bf2bbf79b436dfaa" )
                                    .FirstOrDefault()?.Threat_Name,
                                IsApplicable = entity.IsApplicable,
                                Comment = entity.Comment
                            }).ToList(),

                        RemarkLists = _remark.Find(x => x.Fk_ApplicationId == app.Id)
                            .ToList()
                            .Select(entity => new RemarkList
                            {
                                RemarkId = entity.Id,
                                Remark_Name = _masterremark
                                    .Find(x => x.Id == entity.Fk_MasterRemark && x.Fk_Certificate == "68760495bf2bbf79b436dfaa")
                                    .FirstOrDefault()?.Remark_Name,

                                IsApplicable = entity.IsApplicable,
                                Comment = entity.Comment,
                                AdditionalRemark = entity.AdditionalRemark
                            }).ToList()
                    };



                    response.Data = new List<ReviewerApplicationData> { result };
                    response.Message = "Data fetched successfully.";
                    response.HttpStatusCode = System.Net.HttpStatusCode.OK;
                    response.Success = true;
                    response.ResponseCode = 0;



                }
                else
                {
                    response.Message = "Invalid token.";
                    response.HttpStatusCode = System.Net.HttpStatusCode.Unauthorized;
                    response.Success = false;
                    response.ResponseCode = 1;
                }
            }
            catch (Exception ex)
            {
                response.Message = "GetCustomerApplication Exception: " + ex.Message;
                response.HttpStatusCode = System.Net.HttpStatusCode.InternalServerError;
                response.Success = false;
                response.ResponseCode = 1;
            }

            return response;
        }



        //public async Task<commonResponse> SaveReviewerISOApplication(SaveReviewerISOApplicationRequest request)
        //{
        //    var response = new commonResponse();
        //    var UserId = _acc.HttpContext?.Session.GetString("UserId");
        //    var userFkRole = (await _user.Find(x => x.Id == UserId).FirstOrDefaultAsync())?.Fk_RoleID;
        //    var userType = (await _role.Find(x => x.Id == userFkRole).FirstOrDefaultAsync())?.roleName;

        //    if (userType?.Trim().ToLower() == "reviewer")
        //    {
        //        try
        //        {
        //            if (string.IsNullOrWhiteSpace(request.ApplicationId))
        //            {
        //                response.Message = "ApplicationId is required.";
        //                response.HttpStatusCode = HttpStatusCode.BadRequest;
        //                response.Success = false;
        //                return response;
        //            }

        //            var existingApp = await _iso.Find(x => x.ApplicationId == request.ApplicationId && x.IsDelete == false).FirstOrDefaultAsync();

        //            if (existingApp != null)
        //            {
        //                var updateDef = Builders<tbl_ISO_Application>.Update
        //                    .Set(x => x.Orgnization_Name, request.Orgnization_Name ?? existingApp.Orgnization_Name)
        //                    .Set(x => x.Constituation_of_Orgnization, request.Constituation_of_Orgnization ?? existingApp.Constituation_of_Orgnization)
        //                    .Set(x => x.Audit_Type, request.Audit_Type ?? existingApp.Audit_Type)
        //                    .Set(x => x.Scop_of_Certification, request.Scop_Of_Certification ?? existingApp.Scop_of_Certification)
        //                    .Set(x => x.Technical_Areas, request.Technical_Areas ?? existingApp.Technical_Areas)
        //                    .Set(x => x.Accreditations, request.Accreditations ?? existingApp.Accreditations)
        //                    .Set(x => x.Availbility_of_TechnicalAreas, request.Availbility_of_TechnicalAreas ?? existingApp.Availbility_of_TechnicalAreas)
        //                    .Set(x => x.Availbility_of_Auditor, request.Availbility_of_Auditor ?? existingApp.Availbility_of_Auditor)
        //                    .Set(x => x.Audit_Lang, request.Audit_Lang ?? existingApp.Audit_Lang)
        //                    .Set(x => x.IsInterpreter, request.Is_Interpreter ?? existingApp.IsInterpreter)
        //                    .Set(x => x.IsMultisitesampling, request.Is_Multisite_Sampling ?? existingApp.IsMultisitesampling)
        //                    .Set(x => x.Total_site, request.Total_Site ?? existingApp.Total_site)
        //                    .Set(x => x.Sample_Site, request.Sample_Site ?? existingApp.Sample_Site)
        //                    .Set(x => x.Shift_Details, request.Shift_Details ?? existingApp.Shift_Details)
        //                    .Set(x => x.UpdatedAt, DateTime.Now)
        //                    .Set(x => x.UpdatedBy, UserId);

        //                await _iso.UpdateOneAsync(x => x.Id == existingApp.Id, updateDef);

        //                // You can also call separate Update/Add logic for child collections like Threats, Remarks, Mandays here...

        //                response.Message = "ISO Application updated successfully.";
        //            }
        //            else
        //            {
        //                var newApp = new tbl_ISO_Application
        //                {
        //                    ApplicationId = request.ApplicationId,
        //                    Orgnization_Name = request.Orgnization_Name,
        //                    Constituation_of_Orgnization = request.Constituation_of_Orgnization,
        //                    Fk_Certificate = request.Fk_Certificate,
        //                    Audit_Type = request.Audit_Type,
        //                    Scop_of_Certification = request.Scop_Of_Certification,
        //                    Technical_Areas = request.Technical_Areas,
        //                    Accreditations = request.Accreditations,
        //                    Availbility_of_TechnicalAreas = request.Availbility_of_TechnicalAreas,
        //                    Availbility_of_Auditor = request.Availbility_of_Auditor,
        //                    Audit_Lang = request.Audit_Lang,
        //                    IsInterpreter = request.Is_Interpreter,
        //                    IsMultisitesampling = request.Is_Multisite_Sampling,
        //                    Total_site = request.Total_Site,
        //                    Sample_Site = request.Sample_Site,
        //                    Shift_Details = request.Shift_Details,
        //                    CreatedAt = DateTime.Now,
        //                    CreatedBy = UserId,
        //                    Application_Status = "InProgress",
        //                    IsDelete = false,
        //                    Fk_UserId = UserId
        //                };

        //                await _iso.InsertOneAsync(newApp);

        //                // You can also call Insert logic for child collections here...

        //                response.Message = "ISO Application added successfully.";
        //            }

        //            response.HttpStatusCode = HttpStatusCode.OK;
        //            response.Success = true;
        //        }
        //        catch (Exception ex)
        //        {
        //            response.Message = "SaveReviewerISOApplication Exception: " + ex.Message;
        //            response.HttpStatusCode = HttpStatusCode.InternalServerError;
        //            response.Success = false;
        //        }
        //    }
        //    else
        //    {
        //        response.Message = "Invalid token.";
        //        response.HttpStatusCode = HttpStatusCode.Unauthorized;
        //        response.Success = false;
        //    }

        //    return response;
        //}

        protected override void Disposing()
        {
            //throw new NotImplementedException();
        }
    }
}
