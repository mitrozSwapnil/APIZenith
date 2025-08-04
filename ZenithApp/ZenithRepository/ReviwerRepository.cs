using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Net;
using System.Runtime.ConstrainedExecution;
using ZenithApp.CommonServices;
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

        private readonly IMongoCollection<tbl_FSSC_Application> _fssc;


        private readonly MongoDbService _mongoDbService;




        private readonly IHttpContextAccessor _acc;

        public ReviwerRepository(IOptions<MongoDbSettings> settings, IHttpContextAccessor acc, MongoDbService mongoDbService)
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
            _fssc = database.GetCollection<tbl_FSSC_Application>("tbl_FSSC_Application");


            _acc = acc;
            _mongoDbService = mongoDbService;
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
                                    Status = (await _status.Find(x => x.Id == app.Status).FirstOrDefaultAsync())?.StatusName ?? "Pending",
                                    ReceiveDate = app.Application_Received_date,
                                    AssignedUserName = _user.Find(x => x.Id == app.Fk_UserId).FirstOrDefault()?.FullName
                                };
                                dashboardList.Add(dashboardRecord);
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                        {
                            var searchTerm = request.SearchTerm.Trim().ToLower();
                            dashboardList = dashboardList
                                .Where(x =>
                                    (!string.IsNullOrEmpty(x.ApplicationId) && x.ApplicationId.ToLower().Contains(searchTerm)) ||
                                    (!string.IsNullOrEmpty(x.Certification_Name) && x.Certification_Name.ToLower().Contains(searchTerm))
                                )
                                .ToList();
                        }

                        // Step 6 — Pagination
                        var totalCount = dashboardList.Count;
                        var skip = (request.PageNumber - 1) * request.PageSize;

                        var paginatedList = dashboardList
                            .Skip(skip)
                            .Take(request.PageSize)
                            .ToList();

                        var pagination = new PageinationDto
                        {
                            PageNumber = request.PageNumber,
                            PageSize = request.PageSize,
                            TotalRecords = totalCount
                        };

                        response.Data = paginatedList;
                        response.Pagination = pagination;
                        response.Message = "Dashboard fetched successfully.";
                        response.HttpStatusCode = System.Net.HttpStatusCode.OK;
                        response.Success = true;
                    }
                    else
                    {
                        response.Message = "Login User Not a ISO Department";
                        response.Success = false;
                        response.HttpStatusCode = System.Net.HttpStatusCode.Unauthorized;
                        response.ResponseCode = 1;
                    }
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


        public async Task<getReviewerApplicationResponse> GetReviewerApplication(getReviewerApplicationRequest request)
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
                    if (request.CertificationName == "ISO")
                    {
                        var filter = Builders<tbl_ISO_Application>.Filter.Eq(x => x.Id, request.applicationId);
                        
                        var data = await _iso.Find(filter).FirstOrDefaultAsync();
                        var cerificateName = _mongoDbService.Getcertificatename(data.Fk_Certificate);
                        var assignmane = _mongoDbService.ReviewerName(data.AssignTo);
                        var result = new
                        {
                           
                             Data = data,
                             CertificateName = cerificateName,
                             AssigntToName = assignmane,
                        };

                        response.Data = result;
                    }
                    else if (request.CertificationName == "FSSC")
                    {
                        var filter = Builders<tbl_FSSC_Application>.Filter.Eq(x => x.Id, request.applicationId);
                        var result = await _fssc.Find(filter).FirstOrDefaultAsync();

                        response.Data = result;
                    }
                    else
                    {
                        response.Message = "Invalid Certification Name.";
                        response.HttpStatusCode = System.Net.HttpStatusCode.BadRequest;
                        response.Success = false;
                        response.ResponseCode = 1;
                        return response;
                    }
                    response.Message = "Data fetched successfully.";
                    response.HttpStatusCode = HttpStatusCode.OK;
                    response.Success = true;
                    response.ResponseCode = 0;






                    //var app = _iso
                    //    .Find(x => x.Id == request.applicationId && x.IsDelete == false)
                    //    .FirstOrDefault();

                    ////var status = _status
                    ////    .Find(x => x.Id == app.Status && x.IsDelete == false)
                    ////    .FirstOrDefault().StatusName;

                    //if (app == null)
                    //{
                    //    response.Message = "No data found for the given ApplicationId.";
                    //    response.HttpStatusCode = System.Net.HttpStatusCode.NotFound;
                    //    response.Success = false;
                    //    response.ResponseCode = 1;
                    //    return response;
                    //}
                    

                    //var result = new ReviewerApplicationData
                    //{
                    //    Id = app.Id,
                    //    ApplicationId = app.ApplicationId,
                    //    Orgnization_Name = app.Orgnization_Name,
                    //    Application_Received_date = app.Application_Received_date,
                    //    Constituation_of_Orgnization = app.Constituation_of_Orgnization,
                    //    Certification_Name = _masterCertificate
                    //        .Find(x => x.Id == app.Fk_Certificate && x.Is_Delete == false)
                    //        .FirstOrDefault()?.Certificate_Name,
                    //    Audit_Type = app.Audit_Type,
                    //    Scop_Of_Certification = app.Scop_of_Certification,
                    //    Technical_Areas = app.Technical_Areas,
                    //    Accreditations = app.Accreditations,
                    //    Availbility_of_TechnicalAreas = app.Availbility_of_TechnicalAreas,
                    //    Availbility_of_Auditor = app.Availbility_of_Auditor,
                    //    Audit_Lang = app.Audit_Lang,
                    //    Is_Interpreter = app.IsInterpreter,
                    //    Is_Multisite_Sampling = app.IsMultisitesampling,
                    //    Total_Site = app.Total_site,
                    //    Sample_Site = app.Sample_Site,
                    //    Shift_Details = app.Shift_Details,
                    //    status = _status.Find(x => x.Id == app.Status).FirstOrDefault()?.StatusName ?? "InProgress",
                    //    AssignUser = app.Fk_UserId,
                    //    CustomerSites = app.CustomerSites,
                    //    KeyPersonnels = app.reviewerKeyPersonnel,
                    //    MandaysLists = app.MandaysLists,
                    //    ThreatLists = app.ReviewerThreatList,

                    //    RemarkLists = app.ReviewerRemarkList
                    //};

                    //response.Data = new List<ReviewerApplicationData> { result };
                    //response.Message = "Data fetched successfully.";
                    //response.HttpStatusCode = System.Net.HttpStatusCode.OK;
                    //response.Success = true;
                    //response.ResponseCode = 0;



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



        public async Task<addReviewerApplicationResponse> SaveISOApplication(addReviewerApplicationRequest request)
        {
            var response = new addReviewerApplicationResponse();
            var UserId = _acc.HttpContext?.Session.GetString("UserId");
            var userFkRole = (await _user.Find(x => x.Id == UserId).FirstOrDefaultAsync())?.Fk_RoleID;
            var department = (await _user.Find(x => x.Id == UserId).FirstOrDefaultAsync())?.Department;
            var userType = (await _role.Find(x => x.Id == userFkRole).FirstOrDefaultAsync())?.roleName;

            if (userType?.Trim().ToLower() == "reviewer")
            {
                
                try
                {
                    var now = DateTime.Now;
                    
                        bool? isFinalSubmit = null;
                        DateTime? submitDate = null;
                        string status = "687a2925694d00158c9bf265";

                        // If user provided isFinalSubmit flag
                        if (!string.IsNullOrWhiteSpace(request.IsFinalSubmit))
                        {
                            isFinalSubmit = request.IsFinalSubmit.Trim().ToLower() == "true";

                            if (isFinalSubmit == true)
                            {
                                submitDate = DateTime.Now;
                                status = "687a2925694d00158c9bf267"; // Final Submit status

                            }
                        }
                        if (!string.IsNullOrEmpty(request.ApplicationId))
                        {

                            
                            var filter = Builders<tbl_ISO_Application>.Filter.Eq(x => x.Id, request.Id);

                            // First, clear the sublists (hard delete)
                            var clearSubLists = Builders<tbl_ISO_Application>.Update
                                .Set(x => x.CustomerSites, new List<ReviewerSiteDetails>())
                                .Set(x => x.reviewerKeyPersonnel, new List<ReviewerKeyPersonnelList>())
                                .Set(x => x.MandaysLists, new List<ReviewerAuditMandaysList>())
                                .Set(x => x.ReviewerThreatList, new List<ReviewerThreatList>())
                                .Set(x => x.ReviewerRemarkList, new List<ReviewerRemarkList>());

                            await _iso.UpdateOneAsync(filter, clearSubLists);

                            // Then, update all fields including new sublist data
                            var update = Builders<tbl_ISO_Application>.Update
                                .Set(x => x.Application_Received_date, request.Application_Received_date)
                                .Set(x => x.Orgnization_Name, request.Orgnization_Name)
                                .Set(x => x.Constituation_of_Orgnization, request.Constituation_of_Orgnization)
                                .Set(x => x.Fk_Certificate, request.Fk_Certificate)
                                .Set(x => x.AssignTo, request.AssignTo)
                                .Set(x => x.Audit_Type, request.Audit_Type)
                                .Set(x => x.Scop_of_Certification, request.Scop_of_Certification)
                                .Set(x => x.Availbility_of_TechnicalAreas, request.Availbility_of_TechnicalAreas)
                                .Set(x => x.Availbility_of_Auditor, request.Availbility_of_Auditor)
                                .Set(x => x.Audit_Lang, request.Audit_Lang)
                                .Set(x => x.ActiveState, request.ActiveState ?? 1)
                                .Set(x => x.IsInterpreter, request.IsInterpreter)
                                .Set(x => x.IsMultisitesampling, request.IsMultisitesampling)
                                .Set(x => x.Total_site, request.Total_site)
                                .Set(x => x.Sample_Site, request.Sample_Site ?? new List<LabelValue>())
                                .Set(x => x.Shift_Details, request.Shift_Details ?? new List<LabelValue>())
                                .Set(x => x.Status, status)
                                .Set(x => x.Application_Status, status)
                                .Set(x => x.IsDelete, request.IsDelete ?? false)
                                .Set(x => x.IsFinalSubmit, isFinalSubmit ?? false)
                                .Set(x => x.Fk_UserId, request.Fk_UserId)
                                .Set(x => x.Technical_Areas, request.Technical_Areas ?? new List<TechnicalAreasList>())
                                .Set(x => x.Accreditations, request.Accreditations ?? new List<AccreditationsList>())
                                .Set(x => x.CustomerSites, request.CustomerSites ?? new List<ReviewerSiteDetails>())
                                .Set(x => x.reviewerKeyPersonnel, request.KeyPersonnels ?? new List<ReviewerKeyPersonnelList>())
                                .Set(x => x.MandaysLists, request.MandaysLists ?? new List<ReviewerAuditMandaysList>())
                                .Set(x => x.ReviewerThreatList, request.ThreatLists ?? new List<ReviewerThreatList>())
                                .Set(x => x.ReviewerRemarkList, request.RemarkLists ?? new List<ReviewerRemarkList>())
                                .Set(x => x.UpdatedAt, now)
                                .Set(x => x.UpdatedBy, UserId);

                            await _iso.UpdateOneAsync(filter, update);

                            //response.Data = new List<tbl_ISO_Application> { result };
                            response.Message = "Data Saved successfully.";
                            response.HttpStatusCode = System.Net.HttpStatusCode.OK;
                            response.Success = true;
                            response.ResponseCode = 0;
                        }

                }
                catch (Exception ex)
                {
                    response.Message = "SubmitFsscApplication Exception: " + ex.Message;
                    response.HttpStatusCode = HttpStatusCode.InternalServerError;
                    response.Success = false;
                }
            }
            else
            {
                response.Message = "Invalid token.";
                response.HttpStatusCode = HttpStatusCode.Unauthorized;
                response.Success = false;
            }

            return response;
        }


        //public getFsscResponse GetFSSCApplication(getFsscRequest request)
        //{
        //    getFsscResponse response = new getFsscResponse();
        //    try
        //    {
        //        var UserId = _acc.HttpContext?.Session.GetString("UserId");
        //        var userFkRole = _user.Find(x => x.Id == UserId).FirstOrDefault()?.Fk_RoleID;
        //        var userType = _role.Find(x => x.Id == userFkRole).FirstOrDefault()?.roleName;

        //        if (userType?.Trim().ToLower() == "reviewer")
        //        {
        //            if (string.IsNullOrWhiteSpace(request.applicationId))
        //            {
        //                response.Message = "ApplicationId is required.";
        //                response.HttpStatusCode = System.Net.HttpStatusCode.BadRequest;
        //                response.Success = false;
        //                response.ResponseCode = 1;
        //                return response;
        //            }

        //            var app = _iso
        //                .Find(x => x.Id == request.applicationId && x.IsDelete == false)
        //                .FirstOrDefault();

        //            //var status = _status
        //            //    .Find(x => x.Id == app.Status && x.IsDelete == false)
        //            //    .FirstOrDefault().StatusName;

        //            if (app == null)
        //            {
        //                response.Message = "No data found for the given ApplicationId.";
        //                response.HttpStatusCode = System.Net.HttpStatusCode.NotFound;
        //                response.Success = false;
        //                response.ResponseCode = 1;
        //                return response;
        //            }


        //            var result = new ReviewerApplicationData
        //            {
        //                Id = app.Id,
        //                ApplicationId = app.ApplicationId,
        //                Orgnization_Name = app.Orgnization_Name,
        //                Application_Received_date = app.Application_Received_date,
        //                Constituation_of_Orgnization = app.Constituation_of_Orgnization,
        //                Certification_Name = _masterCertificate
        //                    .Find(x => x.Id == app.Fk_Certificate && x.Is_Delete == false)
        //                    .FirstOrDefault()?.Certificate_Name,
        //                Audit_Type = app.Audit_Type,
        //                Scop_Of_Certification = app.Scop_of_Certification,
        //                Technical_Areas = app.Technical_Areas,
        //                Accreditations = app.Accreditations,
        //                Availbility_of_TechnicalAreas = app.Availbility_of_TechnicalAreas,
        //                Availbility_of_Auditor = app.Availbility_of_Auditor,
        //                Audit_Lang = app.Audit_Lang,
        //                Is_Interpreter = app.IsInterpreter,
        //                Is_Multisite_Sampling = app.IsMultisitesampling,
        //                Total_Site = app.Total_site,
        //                Sample_Site = app.Sample_Site,
        //                Shift_Details = app.Shift_Details,
        //                status = _status.Find(x => x.Id == app.Status).FirstOrDefault()?.StatusName ?? "InProgress",
        //                AssignUser = app.Fk_UserId,
        //                CustomerSites = app.CustomerSites,
        //                KeyPersonnels = app.reviewerKeyPersonnel,
        //                MandaysLists = app.MandaysLists,
        //                ThreatLists = app.ReviewerThreatList,

        //                RemarkLists = app.ReviewerRemarkList
        //            };

        //            response.Data = new List<ReviewerApplicationData> { result };
        //            response.Message = "Data fetched successfully.";
        //            response.HttpStatusCode = System.Net.HttpStatusCode.OK;
        //            response.Success = true;
        //            response.ResponseCode = 0;



        //        }
        //        else
        //        {
        //            response.Message = "Invalid token.";
        //            response.HttpStatusCode = System.Net.HttpStatusCode.Unauthorized;
        //            response.Success = false;
        //            response.ResponseCode = 1;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        response.Message = "GetCustomerApplication Exception: " + ex.Message;
        //        response.HttpStatusCode = System.Net.HttpStatusCode.InternalServerError;
        //        response.Success = false;
        //        response.ResponseCode = 1;
        //    }

        //    return response;
        //}

        protected override void Disposing()
        {
            //throw new NotImplementedException();
        }
    }
}
