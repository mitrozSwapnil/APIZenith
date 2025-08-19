using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Net;
using System.Runtime.ConstrainedExecution;
using System.Threading;
using ZenithApp.CommonServices;
using ZenithApp.Settings;
using ZenithApp.ZenithEntities;
using ZenithApp.ZenithMessage;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ZenithApp.ZenithRepository
{
    public class AdminRepository : BaseRepository
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
        private readonly IMongoCollection<tbl_Application_Remark> _remark;
        private readonly IMongoCollection<tbl_Application_Threat> _threat;
        private readonly IMongoCollection<tbl_Master_Threat> _masterthreat;
        private readonly IMongoCollection<tbl_Master_Remark> _masterremark;
        private readonly IMongoCollection<tbl_ISO_Application> _iso;
        private readonly IMongoCollection<tbl_FSSC_Application> _fssc;
        private readonly IMongoCollection<tbl_ICMED_Application>_icmed;
        private readonly IMongoCollection<tbl_ICMED_PLUS_Application>_icmedplus;
        private readonly IMongoCollection<tbl_IMDR_Application> _imdr;
        private readonly MongoDbService _mongoDbService;
        private readonly IMongoCollection<tbl_ApplicationReview> _reviews;
        private readonly IMongoCollection<tbl_ApplicationFieldHistory> _history;
        private readonly IMongoCollection<tbl_ApplicationFieldComment> _comments;



        private readonly IHttpContextAccessor _acc;

        public AdminRepository(IOptions<MongoDbSettings> settings, IHttpContextAccessor acc, MongoDbService mongoDbService)
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
            _remark = database.GetCollection<tbl_Application_Remark>("tbl_Application_Remark");
            _threat = database.GetCollection<tbl_Application_Threat>("tbl_Application_Threat");
            _masterthreat = database.GetCollection<tbl_Master_Threat>("tbl_Master_Threat");
            _iso = database.GetCollection<tbl_ISO_Application>("tbl_ISO_Application");
            _fssc = database.GetCollection<tbl_FSSC_Application>("tbl_FSSC_Application");
            _icmed = database.GetCollection<tbl_ICMED_Application>("tbl_ICMED_Application");
            _imdr = database.GetCollection<tbl_IMDR_Application>("tbl_IMDR_Application");
            _mongoDbService = mongoDbService;
            _acc = acc;
            _reviews = database.GetCollection<tbl_ApplicationReview>("tbl_ApplicationReview");
            _history = database.GetCollection<tbl_ApplicationFieldHistory>("tbl_ApplicationFieldHistory");
            _comments = database.GetCollection<tbl_ApplicationFieldComment>("tbl_ApplicationFieldComment");



        }

        // Add methods for admin functionalities here
        // For example, methods to manage users, roles, etc.
        // Example method to get admin dashboard data

        public async Task<getDashboardResponse> GetAdminDashboard(getDashboardRequest request)
        {
            var response = new getDashboardResponse();

            var userId = _acc.HttpContext?.Session.GetString("UserId");
            var userFkRole = (await _user.Find(x => x.Id == userId).FirstOrDefaultAsync())?.Fk_RoleID;
            var usertype = (await _role.Find(x => x.Id == userFkRole).FirstOrDefaultAsync())?.roleName;

            if (usertype?.Trim().ToLower() == "admin")
            {
                try
                {
                    if (string.IsNullOrEmpty(userId))
                    {
                        response.Message = "Session expired or invalid user.";
                        response.Success = false;
                        response.HttpStatusCode = System.Net.HttpStatusCode.Unauthorized;
                        response.ResponseCode = 1;
                        return response;
                    }

                    var dashboardList = new List<CustomerDashboardData>();

                    // Step 1 — Prepare default application filter (no status filter here)
                    var applicationFilter = Builders<tbl_customer_application>.Filter.And(
                        Builders<tbl_customer_application>.Filter.Eq(x => x.IsDelete, false),
                        Builders<tbl_customer_application>.Filter.Eq(x => x.IsFinalSubmit, true)
                    );

                    // Step 2 — Fetch all applications
                    var applications = await _customer.Find(applicationFilter).ToListAsync();

                    // Step 3 — Optional: Find status record once if flag is provided
                    tbl_Status? statusRecord = null;
                    bool filterByStatus = false;

                    if (!string.IsNullOrWhiteSpace(request.Flag))
                    {
                        statusRecord = await _status
                            .Find(x => x.StatusName.ToLower() == request.Flag.Trim().ToLower())
                            .FirstOrDefaultAsync();

                        if (statusRecord == null)
                        {
                            return new getDashboardResponse
                            {
                                Success = true,
                                Message = "No records found for given status.",
                                Data = new List<CustomerDashboardData>(),
                                HttpStatusCode = System.Net.HttpStatusCode.OK
                            };
                        }

                        filterByStatus = true;
                    }

                    // Step 4 — Loop over each application and its certificates
                    foreach (var app in applications)
                    {
                        var certificates = await _customercertificates
                            .Find(x => x.Fk_Customer_Application == app.Id && x.Is_Delete == false)
                            .ToListAsync();

                        foreach (var cert in certificates)
                        {
                            // Apply status filter at certificate level only if a flag was provided
                            if (filterByStatus && cert.status != statusRecord.Id)
                            {
                                continue; // Skip this certificate
                            }

                            var masterCert = await _masterCertificate
                                .Find(x => x.Id == cert.Fk_Certificates)
                                .FirstOrDefaultAsync();

                            if (masterCert != null)
                            {
                                var assignedUser = !string.IsNullOrWhiteSpace(cert.AssignTo)
                                    ? await _user.Find(x => x.Id == cert.AssignTo && x.IsDelete == 0)
                                        .FirstOrDefaultAsync()
                                    : null;

                                var dashboardRecord = new CustomerDashboardData
                                {
                                    Id = cert.Id,
                                    ApplicationId = cert.Id,
                                    ApplicationName  =app.ApplicationId,
                                    ReceiveDate = app.SubmitDate,
                                    CompanyName = app.Orgnization_Name,
                                    Certification_Name = masterCert.Certificate_Name,
                                    Certification_Id = masterCert.Id,
                                    Status = (await _status.Find(x => x.Id == cert.status).FirstOrDefaultAsync())?.StatusName ?? "Pending",
                                    AssignedUserName = assignedUser?.Id,
                                };

                                dashboardList.Add(dashboardRecord);
                            }
                        }
                    }

                    // Step 5 — Search filter (optional)
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
                    // Sort by ReceiveDate descending before pagination
                    dashboardList = dashboardList
                        .OrderByDescending(x => x.ReceiveDate)
                        .ToList();

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

                    // Step 7 — Final response
                    response.Data = paginatedList;
                    response.Pagination = pagination;
                    response.Message = "Dashboard Data fetched successfully.";
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

        public async Task<assignUserResponse> AssignApplication(assignUserRequest request)
        {
            var response = new assignUserResponse();

            var userId = _acc.HttpContext?.Session.GetString("UserId");
            var userFkRole = (await _user.Find(x => x.Id == userId).FirstOrDefaultAsync())?.Fk_RoleID;
            var usertype = (await _role.Find(x => x.Id == userFkRole).FirstOrDefaultAsync())?.roleName;

            if (usertype?.Trim().ToLower() == "admin")
            {
                try
                { 
                    if (!string.IsNullOrWhiteSpace(request.TrineeId) && !string.IsNullOrWhiteSpace(request.ReviewerId) &&!string.IsNullOrWhiteSpace(request.ApplicationId))
                    {
                        var application = await _customercertificates.Find(x => x.Id == request.ApplicationId).FirstOrDefaultAsync();
                        var customerapp = await _customer.Find(x => x.Id == application.Fk_Customer_Application).FirstOrDefaultAsync();
                        if (application != null)
                        {
                            application.AssignTo = request.TrineeId;
                            application.UpdatedAt = DateTime.Now; // Update the timestamp
                            application.UpdatedBy = userId; // Set the user who updated
                            var status = await _status.Find(x => x.StatusName == "InProgress" && x.IsDelete == false).FirstOrDefaultAsync();
                            if (status != null)
                            {
                                application.status = "68835335b8054bb3d2914cae";
                            }
                            await _customercertificates.ReplaceOneAsync(x => x.Id == request.ApplicationId, application);
                            if (request.Certification_Id != null)
                            {
                                var masterCert = await _masterCertificate
                                .Find(x => x.Id == request.Certification_Id)
                                .FirstOrDefaultAsync();

                                // Assuming _customerSite is your MongoDB Collection of tbl_customer_site

                                var siteList = await _customersite
                                    .Find(x => x.Fk_Customer_Application == customerapp.Id && x.Is_Delete == false)
                                    .ToListAsync();

                                var customerSiteDetailsList = siteList.Select(site => new ReviewerSiteDetails
                                {
                                    Customer_SiteId = site.Id,
                                    Address = site.Address,
                                    Telephone = site.Telephone,
                                    Web = site.Web,
                                    Email = site.Email,
                                    Activity_Department = site.Activity_Department,
                                    Manpower = site.Manpower,
                                    Shift_Name = site.Shift_Name
                                }).ToList();

                                var keyPersonnelsData = await _customerKeyPersonnel
                                    .Find(x => x.Fk_Customer_Application == request.ApplicationId && x.Is_Delete == false)
                                    .ToListAsync();
                                var keyPersonnelsList = keyPersonnelsData.Select(kp => new KeyPersonnelList
                                {
                                    customerKeyPersonnelId = kp.Id,
                                    Name = kp.FullName,
                                    Designation = kp.Department,
                                    EmailId = kp.EmailId,
                                    Contact_No = string.IsNullOrEmpty(kp.Contact_No) ? kp.Contact : kp.Contact_No,  // pick available field
                                    Type = kp.TypeOfPersonnel
                                }).ToList();


                                var reviewerkeyPersonnelsList = keyPersonnelsData.Select(kp => new ReviewerKeyPersonnelList
                                {
                                    ActivityName = kp.Department,
                                    PersonnelByClient = keyPersonnelsList.Count(p => p.Designation == kp.Department).ToString(), // count matching department
                                    EffectivePersonnel = "",
                                    Comment = "",
                                    AdditionalComments = "", // pick available field
                                    Fk_site = null
                                }).ToList();


                                var reviewerAuditMandaysData = await _reviewerAuditManDays
                                    .Find(x => x.Fk_ApplicationId == request.ApplicationId)
                                    .ToListAsync();


                                var mandaysList = reviewerAuditMandaysData.Select(x => new ReviewerAuditMandaysList
                                {
                                    ActivityName = x.ActivityName,
                                    Audit_ManDays = x.Man_Days,
                                    Additional_ManDays = x.Additional_Mandays,
                                    OnSite_Stage1_ManDays = x.OnSite_manDays_Stage_1,
                                    OnSite_Stage2_ManDays = x.OnSite_manDays_Stage_2,
                                    OffSite_Stage1_ManDays = x.OfSite_manDays_Stage_1,
                                    OffSite_Stage2_ManDays = x.OfSite_manDays_Stage_2,
                                    Recertification_OnSite_ManDays = 0,        // You can map if you have this field elsewhere
                                    Recertification_OffSite_ManDays = 0,      // Same here
                                    AdditionalComments = x.Comment,
                                    
                                    Note = x.Note
                                }).ToList();


                                switch (masterCert.Certificate_Name)
                                {
                                    case "ISO":

                                        await _isoApplication.InsertOneAsync(new tbl_ISO_Application
                                        {
                                            ApplicationId = request.ApplicationId,
                                            ApplicationName=customerapp.ApplicationId,
                                            Application_Received_date = DateTime.Now,
                                            ActiveReviwer = request.ApplicationId ?? "ReviwerOne",
                                            Orgnization_Name = customerapp.Orgnization_Name,
                                            Constituation_of_Orgnization = customerapp.Constituation_of_Orgnization,
                                            Fk_Certificate = application.Fk_Certificates,
                                            AssignTo = request.ReviewerId,
                                            Audit_Type = "",  // Set based on logic or request
                                            Scop_of_Certification = "",
                                            Technical_Areas = new List<TechnicalAreasList>(),
                                            Accreditations = new List<AccreditationsList>(),
                                            Availbility_of_TechnicalAreas = false,
                                            Availbility_of_Auditor = false,
                                            Audit_Lang = "",
                                            IsInterpreter = false,
                                            IsMultisitesampling = false,
                                            Total_site = customerSiteDetailsList?.Count ?? 0,  // <-- Set site count

                                            Sample_Site = new List<LabelValue>(),   // If required, fill here
                                            Shift_Details = new List<LabelValue>(), // If required, fill here

                                            CustomerSites = customerSiteDetailsList,
                                            KeyPersonnels = keyPersonnelsList,
                                            MandaysLists = mandaysList,
                                            reviewerKeyPersonnel = reviewerkeyPersonnelsList,
                                            CreatedAt = DateTime.Now,
                                            CreatedBy = userId,
                                            Status = status.Id,
                                            Application_Status = status.Id,
                                            IsDelete = false,
                                            IsFinalSubmit = false,
                                            Fk_UserId = request.ReviewerId
                                        });
                                        if(!string.IsNullOrWhiteSpace(request.TrineeId))
                                        {
                                            await _isoApplication.InsertOneAsync(new tbl_ISO_Application
                                            {
                                                ApplicationId = request.ApplicationId,
                                                ApplicationName = customerapp.ApplicationId,
                                                CertiFicateName = masterCert.Certificate_Name,
                                                Application_Received_date = DateTime.Now,
                                                ActiveReviwer = request.ApplicationId ?? "ReviwerOne",
                                                Orgnization_Name = customerapp.Orgnization_Name,
                                                Constituation_of_Orgnization = customerapp.Constituation_of_Orgnization,
                                                Fk_Certificate = application.Fk_Certificates,
                                                AssignTo = request.TrineeId,
                                                Audit_Type = "",  // Set based on logic or request
                                                Scop_of_Certification = "",
                                                Technical_Areas = new List<TechnicalAreasList>(),
                                                Accreditations = new List<AccreditationsList>(),
                                                Availbility_of_TechnicalAreas = false,
                                                Availbility_of_Auditor = false,
                                                Audit_Lang = "",
                                                IsInterpreter = false,
                                                IsMultisitesampling = false,
                                                Total_site = customerSiteDetailsList?.Count ?? 0,  // <-- Set site count

                                                Sample_Site = new List<LabelValue>(),   // If required, fill here
                                                Shift_Details = new List<LabelValue>(), // If required, fill here

                                                CustomerSites = customerSiteDetailsList,
                                                KeyPersonnels = keyPersonnelsList,
                                                MandaysLists = mandaysList,
                                                reviewerKeyPersonnel = reviewerkeyPersonnelsList,
                                                CreatedAt = DateTime.Now,
                                                CreatedBy = userId,
                                                Status = status.Id,
                                                Application_Status = status.Id,
                                                IsDelete = false,
                                                IsFinalSubmit = false,
                                                Fk_UserId = request.TrineeId
                                            });
                                        }

                                        break;

                                    case "FSSC":
                                        await _fssc.InsertOneAsync(new tbl_FSSC_Application
                                        {
                                            ApplicationId = request.ApplicationId,
                                            Application_Received_date = DateTime.Now,
                                            ActiveReviwer = request.ApplicationId ?? "ReviwerOne",
                                            Orgnization_Name = customerapp.Orgnization_Name,
                                            Constituation_of_Orgnization = customerapp.Constituation_of_Orgnization,
                                            Fk_Certificate = application.Fk_Certificates,
                                            AssignTo = request.TrineeId,
                                            Audit_Type = "",  // Set based on logic or request
                                            Scop_of_Certification = "",
                                            Technical_Areas = new List<TechnicalAreasList>(),
                                            Accreditations = new List<AccreditationsList>(),
                                            categoryLists = new List<CategoryList>(),
                                            //subCategoryLists = new List<SubCategoryList>(),
                                            Availbility_of_TechnicalAreas = false,
                                            Availbility_of_Auditor = false,
                                            Audit_Lang = "",
                                            IsInterpreter = false,
                                            IsMultisitesampling = false,
                                            Seasonality_Factor = "", // Set based on logic or request
                                            AnyAllergens = "", // Set based on logic or request
                                            Total_site = customerSiteDetailsList?.Count ?? 0,  // <-- Set site count
                                            Sample_Site = new List<LabelValue>(),   // If required, fill here
                                            Shift_Details = new List<LabelValue>(), // If required, fill here
                                            CustomerSites = customerSiteDetailsList,
                                            KeyPersonnels = keyPersonnelsList,
                                            MandaysLists = mandaysList,
                                            reviewerKeyPersonnel= reviewerkeyPersonnelsList,
                                            CreatedAt = DateTime.Now,
                                            CreatedBy = userId,
                                            Status = status.Id,
                                            IsDelete = false,
                                            IsFinalSubmit = false,
                                            Fk_UserId = request.TrineeId

                                        });
                                    break;

                                    case "ICMED":
                                        await _icmed.InsertOneAsync(new tbl_ICMED_Application
                                        {
                                            ApplicationId = request.ApplicationId,
                                            ActiveReviwer = request.ApplicationId ?? "ReviwerOne",
                                            Application_Received_date = DateTime.Now,
                                            Orgnization_Name = customerapp.Orgnization_Name,
                                            Certification_Name = customerapp.Constituation_of_Orgnization,
                                            Fk_Certificate = application.Fk_Certificates,
                                            Audit_Type = "",
                                            Scop_of_Certification = "",
                                            AssignTo = request.TrineeId,
                                            
                                            
                                            remark = "",

                                            Availbility_of_TechnicalAreas = false,
                                            Availbility_of_Auditor = false,
                                            Audit_Lang = "",
                                            ActiveState = 1, // default as per model

                                            IsInterpreter = false,
                                            IsMultisitesampling = false,
                                            Total_site = customerSiteDetailsList?.Count ?? 0,
                                            Sample_Site = new List<LabelValue>(),
                                            Shift_Details = new List<LabelValue>(),

                                            CreatedAt = DateTime.Now,
                                            CreatedBy = userId,
                                            UpdatedAt = null,
                                            UpdatedBy = null,

                                            Status = status.Id,
                                            IsDelete = false,
                                            IsFinalSubmit = false,
                                            Fk_UserId = request.TrineeId,

                                            Technical_Areas = new List<TechnicalAreasList>(),
                                            Accreditations = new List<AccreditationsList>(),

                                            CustomerSites = customerSiteDetailsList,
                                            KeyPersonnels = keyPersonnelsList,

                                            reviewerKeyPersonnel = reviewerkeyPersonnelsList,
                                            MandaysLists = mandaysList,
                                            ReviewerThreatList = new List<ReviewerThreatList>(),
                                            ReviewerRemarkList = new List<ReviewerRemarkList>(),
                                            // If you plan to use RemarkLists in the future, uncomment the next line
                                            // RemarkLists = new List<RemarkList>()
                                        });
                                    break;

                                    case "ICMED_PLUS":
                                        await _icmed.InsertOneAsync(new tbl_ICMED_Application
                                        {
                                            ApplicationId = request.ApplicationId,
                                            ActiveReviwer = request.ApplicationId ?? "ReviwerOne",
                                            Application_Received_date = DateTime.Now,
                                            Orgnization_Name = customerapp.Orgnization_Name,
                                            Certification_Name = customerapp.Constituation_of_Orgnization,
                                            Fk_Certificate = application.Fk_Certificates,
                                            Audit_Type = "",
                                            Scop_of_Certification = "",
                                            AssignTo = request.TrineeId,
                                            remark = "",
                                            Availbility_of_TechnicalAreas = false,
                                            Availbility_of_Auditor = false,
                                            Audit_Lang = "",
                                            ActiveState = 1, // default as per model
                                            ApplicationReviewDate= null,
                                            IsInterpreter = false,
                                            IsMultisitesampling = false,
                                            Total_site = customerSiteDetailsList?.Count ?? 0,
                                            Sample_Site = new List<LabelValue>(),
                                            Shift_Details = new List<LabelValue>(),

                                            CreatedAt = DateTime.Now,
                                            CreatedBy = userId,
                                            UpdatedAt = null,
                                            UpdatedBy = null,

                                            Status = status.Id,
                                            IsDelete = false,
                                            IsFinalSubmit = false,
                                            Fk_UserId = request.TrineeId,

                                            Technical_Areas = new List<TechnicalAreasList>(),
                                            Accreditations = new List<AccreditationsList>(),

                                            CustomerSites = customerSiteDetailsList,
                                            KeyPersonnels = keyPersonnelsList,

                                            reviewerKeyPersonnel = reviewerkeyPersonnelsList,
                                            MandaysLists = mandaysList,
                                            ReviewerThreatList = new List<ReviewerThreatList>(),
                                            ReviewerRemarkList = new List<ReviewerRemarkList>(),
                                            // If you plan to use RemarkLists in the future, uncomment the next line
                                            // RemarkLists = new List<RemarkList>()
                                        });
                                    break;

                                    case "IMDR":
                                        await _imdr.InsertOneAsync(new tbl_IMDR_Application
                                        {
                                            ApplicationId = request.ApplicationId,
                                            Application_Received_date = DateTime.Now,
                                            Orgnization_Name = customerapp.Orgnization_Name,
                                            Fk_Certificate = application.Fk_Certificates,
                                            Scop_of_Certification = "",
                                            ActiveReviwer = request.ApplicationId ?? "ReviwerOne",
                                            DeviceMasterfile ="",
                                            KeyPersonnels = keyPersonnelsList,
                                            CustomerSites = customerSiteDetailsList,

                                            Technical_Areas = new List<TechnicalAreasList>(),
                                            Availbility_of_TechnicalAreas = false,
                                            Availbility_of_Auditor = false,
                                            Audit_Lang = "",
                                            IsInterpreter = false,
                                            imdrManDays = new List<ImdrManDays>(),
                                            reviewerKeyPersonnel = reviewerkeyPersonnelsList,
                                            mdrauditLists = new List<MDRAuditList>(),
                                            ReviewerThreatList = new List<ReviewerThreatList>(),
                                            ReviewerRemarkList = new List<ReviewerRemarkList>(),
                                            CreatedAt = DateTime.Now,
                                            CreatedBy = userId,
                                            Status = status.Id,
                                            IsDelete = false,
                                            IsFinalSubmit = false,
                                            Fk_UserId = request.TrineeId,
                                            AssignTo = request.TrineeId
                                        });
                                    break;

                                    default:
                                        // Handle unknown certification if needed
                                    break;

                                }


                            }


                            response.Message = "Application assigned successfully.";
                            response.HttpStatusCode = System.Net.HttpStatusCode.OK;
                            response.Success = true;
                            response.ResponseCode = 0;
                        }
                        else
                        {
                            response.Message = "Application not found.";
                            response.HttpStatusCode = System.Net.HttpStatusCode.NotFound;
                            response.Success = false;
                            response.ResponseCode = 1;
                        }
                    }
                    else if(!string.IsNullOrWhiteSpace(request.ReviewerId) && !string.IsNullOrWhiteSpace(request.ApplicationId))
                    {

                    } 
                    else
                    {
                        response.Message = "Please Provide Correct  Application data.";
                        response.HttpStatusCode = System.Net.HttpStatusCode.BadRequest;
                        response.Success = false;
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

        public async Task<userDropdownResponse> GetDropdown(userDropdownRequest request)
        {
            var response = new userDropdownResponse();

            var userId = _acc.HttpContext?.Session.GetString("UserId");
            var userFkRole = (await _user.Find(x => x.Id == userId).FirstOrDefaultAsync())?.Fk_RoleID;
            var usertype = (await _role.Find(x => x.Id == userFkRole).FirstOrDefaultAsync())?.roleName;

            if (usertype?.Trim().ToLower() == "admin")
            {
                try
                {
                    if (request.type != null)
                    {
                        //var userList = await _user
                        //.Find(x => x.IsDelete == 0 && x.Department.Trim().ToLower() == request.type.Trim().ToLower()).
                        //Project(x => new UserDropdownDto
                        //{
                        //    Id = x.Id,
                        //    Name = x.FullName
                        //}).ToListAsync();

                        //response.Data = userList;
                        response.Message = "Data fetched successfully.";
                        response.Success = true;
                        response.HttpStatusCode = System.Net.HttpStatusCode.OK;
                        response.ResponseCode = 0;
                    }
                    else
                    {
                        var userList = await _user
                        .Find(x => x.IsDelete == 0)
                        .Project(x => new UserDropdownDto
                        {
                            Id = x.Id,
                            Name = x.FullName
                        }).ToListAsync();
                        response.Data = userList;
                        response.Message = "Data fetched successfully.";
                        response.Success = true;
                        response.HttpStatusCode = System.Net.HttpStatusCode.OK;
                        response.ResponseCode = 0;
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



        public async Task<assignUserResponse> AssignReviewerTwoApplication(assignUserRequest request)
        {
            var response = new assignUserResponse();

            var userId = _acc.HttpContext?.Session.GetString("UserId");
            var userFkRole = (await _user.Find(x => x.Id == userId).FirstOrDefaultAsync())?.Fk_RoleID;
            var usertype = (await _role.Find(x => x.Id == userFkRole).FirstOrDefaultAsync())?.roleName;

            if (usertype?.Trim().ToLower() == "admin")
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(request.ReviewerId) && !string.IsNullOrWhiteSpace(request.ApplicationId))
                    {

                        var application = await _isoApplication.Find(x => x.Id == request.ApplicationId && x.IsFinalSubmit==true).SortByDescending(x => x.UpdatedAt ?? x.CreatedAt).FirstOrDefaultAsync();
                        if (application != null)
                        {
                            var masterCert = await _masterCertificate
                                .Find(x => x.Id == request.Certification_Id)
                                .FirstOrDefaultAsync();
                            var customerapplication = await _customercertificates.Find(x => x.Id == request.ApplicationId).FirstOrDefaultAsync();
                            var status = await _status.Find(x => x.StatusName == "InProgress" && x.IsDelete == false).FirstOrDefaultAsync();
                            if (status != null)
                            {
                                customerapplication.status = "6895d36cf3fbe9ce595243cb";
                            }

                            switch (masterCert.Certificate_Name)
                            {
                                case "ISO":
                                    await _isoApplication.InsertOneAsync(new tbl_ISO_Application
                                    {
                                        Application_Received_date = application.Application_Received_date,
                                        Orgnization_Name = application.Orgnization_Name,
                                        Constituation_of_Orgnization = application.Constituation_of_Orgnization,
                                        Fk_Certificate = application.Fk_Certificate,
                                        ActiveReviwer = request.ApplicationId ?? "ReviwerTwo",
                                        AssignTo = application.AssignTo,
                                        Audit_Type = application.Audit_Type,
                                        Scop_of_Certification = application.Scop_of_Certification,
                                        Availbility_of_TechnicalAreas = application.Availbility_of_TechnicalAreas,
                                        Availbility_of_Auditor = application.Availbility_of_Auditor,
                                        Audit_Lang = application.Audit_Lang,
                                        ActiveState = application.ActiveState ?? 1,
                                        IsInterpreter = application.IsInterpreter,
                                        IsMultisitesampling = application.IsMultisitesampling,
                                        Total_site = application.Total_site,
                                        Sample_Site = application.Sample_Site ?? new List<LabelValue>(),
                                        Shift_Details = application.Shift_Details ?? new List<LabelValue>(),
                                        Status = "68835335b8054bb3d2914cae",
                                        Application_Status = "68835335b8054bb3d2914cae",
                                        IsDelete = application.IsDelete ?? false,
                                        IsFinalSubmit = false,
                                        Fk_UserId = request.ReviewerId,
                                        Technical_Areas = application.Technical_Areas ?? new List<TechnicalAreasList>(),
                                        Accreditations = application.Accreditations ?? new List<AccreditationsList>(),
                                        CustomerSites = application.CustomerSites ?? new List<ReviewerSiteDetails>(),
                                        reviewerKeyPersonnel = application.reviewerKeyPersonnel ?? new List<ReviewerKeyPersonnelList>(),
                                        MandaysLists = application.MandaysLists ?? new List<ReviewerAuditMandaysList>(),
                                        ReviewerThreatList = application.ReviewerThreatList ?? new List<ReviewerThreatList>(),
                                        ReviewerRemarkList = application.ReviewerRemarkList ?? new List<ReviewerRemarkList>(),
                                        CreatedAt = DateTime.Now,
                                        CreatedBy = request.ReviewerId
                                    });

                                break;

                                case "ICMED":
                                    // Fetch the last application (ensure latest updated/created)
                                    var latestICMED = await _icmed.Find(x => x.ApplicationId == request.ApplicationId && x.IsFinalSubmit == true)
                                                                  .SortByDescending(x => x.UpdatedAt ?? x.CreatedAt)
                                                                  .FirstOrDefaultAsync();

                                    if (latestICMED != null)
                                    {
                                        await _icmed.InsertOneAsync(new tbl_ICMED_Application
                                        {
                                            ApplicationId = latestICMED.ApplicationId,
                                            Application_Received_date = latestICMED.Application_Received_date,
                                            ApplicationReviewDate = latestICMED.ApplicationReviewDate,
                                            ActiveReviwer = request.ApplicationId ?? "ReviwerTwo",
                                            Orgnization_Name = latestICMED.Orgnization_Name,
                                            Certification_Name = latestICMED.Certification_Name,
                                            Fk_Certificate = latestICMED.Fk_Certificate,

                                            AssignTo = request.ReviewerId,  // Assigning to new reviewer

                                            Audit_Type = latestICMED.Audit_Type,
                                            Scop_of_Certification = latestICMED.Scop_of_Certification,
                                            Availbility_of_TechnicalAreas = latestICMED.Availbility_of_TechnicalAreas,
                                            Availbility_of_Auditor = latestICMED.Availbility_of_Auditor,
                                            Audit_Lang = latestICMED.Audit_Lang,
                                            IsInterpreter = latestICMED.IsInterpreter,
                                            IsMultisitesampling = latestICMED.IsMultisitesampling,
                                            Total_site = latestICMED.Total_site,
                                            Sample_Site = latestICMED.Sample_Site ?? new List<LabelValue>(),
                                            Shift_Details = latestICMED.Shift_Details ?? new List<LabelValue>(),

                                            Status = "68835335b8054bb3d2914cae", // Status: Assigned
                                            IsDelete = false,
                                            IsFinalSubmit = false,
                                            Fk_UserId = request.ReviewerId,
                                            ActiveState = latestICMED.ActiveState ?? 1,

                                            Technical_Areas = latestICMED.Technical_Areas ?? new List<TechnicalAreasList>(),
                                            Accreditations = latestICMED.Accreditations ?? new List<AccreditationsList>(),
                                            CustomerSites = latestICMED.CustomerSites ?? new List<ReviewerSiteDetails>(),
                                            reviewerKeyPersonnel = latestICMED.reviewerKeyPersonnel ?? new List<ReviewerKeyPersonnelList>(),
                                            MandaysLists = latestICMED.MandaysLists ?? new List<ReviewerAuditMandaysList>(),
                                            ReviewerThreatList = latestICMED.ReviewerThreatList ?? new List<ReviewerThreatList>(),
                                            ReviewerRemarkList = latestICMED.ReviewerRemarkList ?? new List<ReviewerRemarkList>(),

                                            CreatedAt = DateTime.Now,
                                            CreatedBy = request.ReviewerId
                                        });
                                    }
                                break;
                                case "ICMED_PLUS":
                                    // Fetch the last application (ensure latest updated/created)
                                    var latestICMEDPlus = await _icmedplus.Find(x => x.ApplicationId == request.ApplicationId && x.IsFinalSubmit == true)
                                                                  .SortByDescending(x => x.UpdatedAt ?? x.CreatedAt)
                                                                  .FirstOrDefaultAsync();

                                    if (latestICMEDPlus != null)
                                    {
                                        await _icmedplus.InsertOneAsync(new tbl_ICMED_PLUS_Application
                                        {
                                            ApplicationId = latestICMEDPlus.ApplicationId,
                                            Application_Received_date = latestICMEDPlus.Application_Received_date,
                                            ApplicationReviewDate = latestICMEDPlus.ApplicationReviewDate,
                                            ActiveReviwer = request.ApplicationId ?? "ReviwerTwo",
                                            Orgnization_Name = latestICMEDPlus.Orgnization_Name,
                                            Certification_Name = latestICMEDPlus.Certification_Name,
                                            Fk_Certificate = latestICMEDPlus.Fk_Certificate,
                                            AssignTo = request.ReviewerId,  // Assigning to new reviewer

                                            Audit_Type = latestICMEDPlus.Audit_Type,
                                            Scop_of_Certification = latestICMEDPlus.Scop_of_Certification,
                                            Availbility_of_TechnicalAreas = latestICMEDPlus.Availbility_of_TechnicalAreas,
                                            Availbility_of_Auditor = latestICMEDPlus.Availbility_of_Auditor,
                                            Audit_Lang = latestICMEDPlus.Audit_Lang,
                                            IsInterpreter = latestICMEDPlus.IsInterpreter,
                                            IsMultisitesampling = latestICMEDPlus.IsMultisitesampling,
                                            Total_site = latestICMEDPlus.Total_site,
                                            Sample_Site = latestICMEDPlus.Sample_Site ?? new List<LabelValue>(),
                                            Shift_Details = latestICMEDPlus.Shift_Details ?? new List<LabelValue>(),

                                            Status = "68835335b8054bb3d2914cae", // Status: Assigned
                                            IsDelete = false,
                                            IsFinalSubmit = false,
                                            Fk_UserId = request.ReviewerId,
                                            ActiveState = latestICMEDPlus.ActiveState ?? 1,

                                            Technical_Areas = latestICMEDPlus.Technical_Areas ?? new List<TechnicalAreasList>(),
                                            Accreditations = latestICMEDPlus.Accreditations ?? new List<AccreditationsList>(),
                                            CustomerSites = latestICMEDPlus.CustomerSites ?? new List<ReviewerSiteDetails>(),
                                            reviewerKeyPersonnel = latestICMEDPlus.reviewerKeyPersonnel ?? new List<ReviewerKeyPersonnelList>(),
                                            MandaysLists = latestICMEDPlus.MandaysLists ?? new List<ReviewerAuditMandaysList>(),
                                            ReviewerThreatList = latestICMEDPlus.ReviewerThreatList ?? new List<ReviewerThreatList>(),
                                            ReviewerRemarkList = latestICMEDPlus.ReviewerRemarkList ?? new List<ReviewerRemarkList>(),

                                            CreatedAt = DateTime.Now,
                                            CreatedBy = request.ReviewerId
                                        });
                                    }
                                break;

                                case "FSSC":
                                    var latestFSSC = await _fssc.Find(x => x.ApplicationId == request.ApplicationId && x.IsFinalSubmit == true)
                                                                .SortByDescending(x => x.UpdatedAt ?? x.CreatedAt)
                                                                .FirstOrDefaultAsync();

                                    if (latestFSSC != null)
                                    {
                                        await _fssc.InsertOneAsync(new tbl_FSSC_Application
                                        {
                                            ApplicationId = latestFSSC.ApplicationId,
                                            Application_Received_date = latestFSSC.Application_Received_date,
                                            ApplicationReviewDate = latestFSSC.ApplicationReviewDate,
                                            Orgnization_Name = latestFSSC.Orgnization_Name,
                                            Constituation_of_Orgnization = latestFSSC.Constituation_of_Orgnization,
                                            Fk_Certificate = latestFSSC.Fk_Certificate,
                                            ActiveReviwer = request.ApplicationId ?? "ReviwerTwo",
                                            AssignTo = request.ReviewerId,  // Assign to new reviewer

                                            Audit_Type = latestFSSC.Audit_Type,
                                            Scop_of_Certification = latestFSSC.Scop_of_Certification,
                                            Availbility_of_TechnicalAreas = latestFSSC.Availbility_of_TechnicalAreas,
                                            Availbility_of_Auditor = latestFSSC.Availbility_of_Auditor,
                                            Audit_Lang = latestFSSC.Audit_Lang,
                                            ActiveState = latestFSSC.ActiveState ?? 1,
                                            IsInterpreter = latestFSSC.IsInterpreter,
                                            IsMultisitesampling = latestFSSC.IsMultisitesampling,
                                            Total_site = latestFSSC.Total_site,
                                            Sample_Site = latestFSSC.Sample_Site ?? new List<LabelValue>(),
                                            Shift_Details = latestFSSC.Shift_Details ?? new List<LabelValue>(),
                                            Seasonality_Factor = latestFSSC.Seasonality_Factor,
                                            AnyAllergens = latestFSSC.AnyAllergens,

                                            Status = "68835335b8054bb3d2914cae", // Assigned
                                            IsDelete = false,
                                            IsFinalSubmit = false,
                                            Fk_UserId = request.ReviewerId,
                                            CreatedAt = DateTime.Now,
                                            CreatedBy = request.ReviewerId,

                                            // Reviewer info
                                            reviewerKeyPersonnel = latestFSSC.reviewerKeyPersonnel ?? new List<ReviewerKeyPersonnelList>(),
                                            MandaysLists = latestFSSC.MandaysLists ?? new List<ReviewerAuditMandaysList>(),
                                            ReviewerThreatList = latestFSSC.ReviewerThreatList ?? new List<ReviewerThreatList>(),
                                            ReviewerRemarkList = latestFSSC.ReviewerRemarkList ?? new List<ReviewerRemarkList>(),

                                            // Technical details
                                            Technical_Areas = latestFSSC.Technical_Areas ?? new List<TechnicalAreasList>(),
                                            Accreditations = latestFSSC.Accreditations ?? new List<AccreditationsList>(),
                                            CustomerSites = latestFSSC.CustomerSites ?? new List<ReviewerSiteDetails>(),

                                            // Key Personnel
                                            KeyPersonnels = latestFSSC.KeyPersonnels ?? new List<KeyPersonnelList>(),

                                            // Audits
                                            auditLists = latestFSSC.auditLists ?? new List<stage1AndStage2Audit>(),
                                            serveillannceAuditLists = latestFSSC.serveillannceAuditLists ?? new List<ServeillannceAudit>(),
                                            reCertificationAudits = latestFSSC.reCertificationAudits ?? new List<ReCertificationAudit>(),
                                            transferAudits = latestFSSC.transferAudits ?? new List<TransferAudit>(),
                                            specialAudits = latestFSSC.specialAudits ?? new List<SpecialAudit>(),

                                            // Product category info
                                            productCategoryAndSubs = latestFSSC.productCategoryAndSubs ?? new List<ProductCategoryAndSubCategoryList>(),
                                            hACCPLists = latestFSSC.hACCPLists ?? new List<HACCPList>(),
                                            standardsLists = latestFSSC.standardsLists ?? new List<StandardsList>(),
                                            categoryLists = latestFSSC.categoryLists ?? new List<CategoryList>(),
                                            subCategoryLists = latestFSSC.subCategoryLists ?? new List<SubCategoryList>()
                                        });
                                    }
                                 break;

                                case "IMDR":
                                    var latestIMDR = await _imdr.Find(x => x.ApplicationId == request.ApplicationId && x.IsFinalSubmit == true)
                                                                .SortByDescending(x => x.UpdatedAt ?? x.CreatedAt)
                                                                .FirstOrDefaultAsync();

                                    if (latestIMDR != null)
                                    {
                                        await _imdr.InsertOneAsync(new tbl_IMDR_Application
                                        {
                                            ApplicationId = latestIMDR.ApplicationId,
                                            Application_Received_date = latestIMDR.Application_Received_date,
                                            Orgnization_Name = latestIMDR.Orgnization_Name,
                                            Fk_Certificate = latestIMDR.Fk_Certificate,
                                            ActiveReviwer = request.ApplicationId ?? "ReviwerTwo",
                                            AssignTo = request.ReviewerId, // Assign to new reviewer
                                            Scop_of_Certification = latestIMDR.Scop_of_Certification,
                                            DeviceMasterfile = latestIMDR.DeviceMasterfile,

                                            Availbility_of_TechnicalAreas = latestIMDR.Availbility_of_TechnicalAreas,
                                            Availbility_of_Auditor = latestIMDR.Availbility_of_Auditor,
                                            Audit_Lang = latestIMDR.Audit_Lang,
                                            ActiveState = latestIMDR.ActiveState ?? 1,
                                            IsInterpreter = latestIMDR.IsInterpreter,

                                            Status = "68835335b8054bb3d2914cae", // Assigned
                                            IsDelete = false,
                                            IsFinalSubmit = false,
                                            Fk_UserId = request.ReviewerId,
                                            CreatedAt = DateTime.Now,
                                            CreatedBy = request.ReviewerId,

                                            // Technical Areas
                                            Technical_Areas = latestIMDR.Technical_Areas ?? new List<TechnicalAreasList>(),
                                            CustomerSites = latestIMDR.CustomerSites ?? new List<ReviewerSiteDetails>(),

                                            // Personnel
                                            KeyPersonnels = latestIMDR.KeyPersonnels ?? new List<KeyPersonnelList>(),
                                            reviewerKeyPersonnel = latestIMDR.reviewerKeyPersonnel ?? new List<ReviewerKeyPersonnelList>(),

                                            // Reviewer Info
                                            ReviewerThreatList = latestIMDR.ReviewerThreatList ?? new List<ReviewerThreatList>(),
                                            ReviewerRemarkList = latestIMDR.ReviewerRemarkList ?? new List<ReviewerRemarkList>(),

                                            // Man-Days and Audits
                                            imdrManDays = latestIMDR.imdrManDays ?? new List<ImdrManDays>(),
                                            mdrauditLists = latestIMDR.mdrauditLists ?? new List<MDRAuditList>()
                                        });
                                    }
                                    break;





                            }


                            response.Message = "Application assigned successfully.";
                            response.HttpStatusCode = System.Net.HttpStatusCode.OK;
                            response.Success = true;
                            response.ResponseCode = 0;
                        }
                        else
                        {
                            response.Message = "Application not found.";
                            response.HttpStatusCode = System.Net.HttpStatusCode.NotFound;
                            response.Success = false;
                            response.ResponseCode = 1;
                        }
                    }
                    else
                    {
                        response.Message = "Please Provide Correct  Application data.";
                        response.HttpStatusCode = System.Net.HttpStatusCode.BadRequest;
                        response.Success = false;
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

        public async Task<getReviewerApplicationResponse> GetAdminApplication(getReviewerApplicationRequest request)
        {
            getReviewerApplicationResponse response = new getReviewerApplicationResponse();
            try
            {
                var UserId = _acc.HttpContext?.Session.GetString("UserId");
                var userFkRole = _user.Find(x => x.Id == UserId).FirstOrDefault()?.Fk_RoleID;
                var userType = _role.Find(x => x.Id == userFkRole).FirstOrDefault()?.roleName;

                if (userType?.Trim().ToLower() == "admin")
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
                        var filter = Builders<tbl_ISO_Application>.Filter.Eq(x => x.ApplicationId, request.applicationId);

                        // Get the last matching record based on descending ObjectId (which reflects insert time)
                        var data = await _iso.Find(filter)
                                             .SortByDescending(x => x.Id)
                                             .FirstOrDefaultAsync();

                        if (data != null)
                        {
                            var certificateName = _mongoDbService.Getcertificatename(data.Fk_Certificate);
                            var assigneeName = _mongoDbService.ReviewerName(data.AssignTo);
                            var statusName = _mongoDbService.StatusName(data.Status);

                            response.Data = data;
                            response.CertificateName = certificateName;
                            response.statusName = statusName;
                        }
                        else
                        {
                            response.Message = "No ISO application found for given ApplicationId.";
                        }
                    }
                    else if (request.CertificationName == "FSSC")
                    {
                        var filter = Builders<tbl_FSSC_Application>.Filter.Eq(x => x.ApplicationId, request.applicationId);

                        var data = await _fssc.Find(filter)
                                              .SortByDescending(x => x.Id)
                                              .FirstOrDefaultAsync();

                        if (data != null)
                        {
                            var certificateName = _mongoDbService.Getcertificatename(data.Fk_Certificate);
                            var assigneeName = _mongoDbService.ReviewerName(data.AssignTo);
                            var statusName = _mongoDbService.StatusName(data.Status);

                            response.Data = data;
                            response.CertificateName = certificateName;
                            response.statusName = statusName;
                        }
                    }

                    else if (request.CertificationName == "ICMED")
                    {
                        var filter = Builders<tbl_ICMED_Application>.Filter.Eq(x => x.ApplicationId, request.applicationId);

                        var data = await _icmed.Find(filter)
                                               .SortByDescending(x => x.Id)
                                               .FirstOrDefaultAsync();

                        if (data != null)
                        {
                            var certificateName = _mongoDbService.Getcertificatename(data.Fk_Certificate);
                            var assigneeName = _mongoDbService.ReviewerName(data.AssignTo);
                            var statusName = _mongoDbService.StatusName(data.Status);

                            response.Data = data;
                            response.CertificateName = certificateName;
                            response.statusName = statusName;
                        }
                    }
                    else if (request.CertificationName == "ICMED_PLUS")
                    {
                        var filter = Builders<tbl_ICMED_PLUS_Application>.Filter.Eq(x => x.ApplicationId, request.applicationId);

                        var data = await _icmedplus.Find(filter)
                                               .SortByDescending(x => x.Id)
                                               .FirstOrDefaultAsync();

                        if (data != null)
                        {
                            var certificateName = _mongoDbService.Getcertificatename(data.Fk_Certificate);
                            var assigneeName = _mongoDbService.ReviewerName(data.AssignTo);
                            var statusName = _mongoDbService.StatusName(data.Status);

                            response.Data = data;
                            response.CertificateName = certificateName;
                            response.statusName = statusName;
                        }
                    }

                    else if (request.CertificationName == "IDMR")
                    {
                        var filter = Builders<tbl_IMDR_Application>.Filter.Eq(x => x.ApplicationId, request.applicationId);

                        var data = await _imdr.Find(filter)
                                              .SortByDescending(x => x.Id)
                                              .FirstOrDefaultAsync();

                        if (data != null)
                        {
                            var certificateName = _mongoDbService.Getcertificatename(data.Fk_Certificate);
                            var assigneeName = _mongoDbService.ReviewerName(data.AssignTo);
                            var statusName = _mongoDbService.StatusName(data.Status);

                            response.Data = data;
                            response.CertificateName = certificateName;
                            response.statusName = statusName;
                        }
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

                     







                    //var Fkcertificate = _iso
                    //    .Find(x => x.Id == request.applicationId && x.IsDelete == false)
                    //    .FirstOrDefault().Fk_Certificate;

                    //var nameofcertificate = _masterCertificate
                    //    .Find(x => x.Id == Fkcertificate && x.Is_Delete == false)
                    //    .FirstOrDefault()?.Id;

                    //var app = _iso
                    //    .Find(x => x.Fk_Certificate == nameofcertificate && x.IsDelete == false)
                    //    .SortByDescending(x => x.CreatedAt)
                    //    .FirstOrDefault();

                    //var status = _status
                    //    .Find(x => x.Id == app.Status && x.IsDelete == false)
                    //    .FirstOrDefault().StatusName;

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

            if (userType?.Trim().ToLower() == "admin")
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
                            .Set(x => x.Fk_UserId, request.Fk_UserId ?? UserId)
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

        public async Task<addReviewerApplicationResponse> SaveFSSCApplication(addFsscApplicationRequest request)
        {
            var response = new addReviewerApplicationResponse();
            var UserId = _acc.HttpContext?.Session.GetString("UserId");
            var userFkRole = (await _user.Find(x => x.Id == UserId).FirstOrDefaultAsync())?.Fk_RoleID;
            var department = (await _user.Find(x => x.Id == UserId).FirstOrDefaultAsync())?.Department;
            var userType = (await _role.Find(x => x.Id == userFkRole).FirstOrDefaultAsync())?.roleName;

            if (userType?.Trim().ToLower() == "admin")
            {
                try
                {
                    var now = DateTime.Now;
                    bool? isFinalSubmit = null;
                    DateTime? submitDate = null;
                    string status = "687a2925694d00158c9bf265";

                    if (!string.IsNullOrWhiteSpace(request.IsFinalSubmit))
                    {
                        isFinalSubmit = request.IsFinalSubmit.Trim().ToLower() == "true";

                        if (isFinalSubmit == true)
                        {
                            submitDate = now;
                            status = "687a2925694d00158c9bf267"; // Final submit
                        }
                    }

                    if (!string.IsNullOrEmpty(request.Id))
                    {
                        var filter = Builders<tbl_FSSC_Application>.Filter.Eq(x => x.Id, request.Id);

                        // Clear sub-lists
                        var clearSubLists = Builders<tbl_FSSC_Application>.Update
                            .Set(x => x.Technical_Areas, new List<TechnicalAreasList>())
                            .Set(x => x.Accreditations, new List<AccreditationsList>())
                            .Set(x => x.CustomerSites, new List<ReviewerSiteDetails>())
                            .Set(x => x.reviewerKeyPersonnel, new List<ReviewerKeyPersonnelList>())
                            .Set(x => x.MandaysLists, new List<ReviewerAuditMandaysList>())
                            .Set(x => x.ReviewerThreatList, new List<ReviewerThreatList>())
                            .Set(x => x.ReviewerRemarkList, new List<ReviewerRemarkList>())
                            .Set(x => x.auditLists, new List<stage1AndStage2Audit>())
                            .Set(x => x.serveillannceAuditLists, new List<ServeillannceAudit>())
                            .Set(x => x.reCertificationAudits, new List<ReCertificationAudit>())
                            .Set(x => x.transferAudits, new List<TransferAudit>())
                            .Set(x => x.specialAudits, new List<SpecialAudit>())
                            .Set(x => x.productCategoryAndSubs, new List<ProductCategoryAndSubCategoryList>())
                            .Set(x => x.hACCPLists, new List<HACCPList>())
                            .Set(x => x.standardsLists, new List<StandardsList>())
                            .Set(x => x.categoryLists, new List<CategoryList>())
                            .Set(x => x.subCategoryLists, new List<SubCategoryList>());

                        await _fssc.UpdateOneAsync(filter, clearSubLists);

                        // Update main fields
                        var update = Builders<tbl_FSSC_Application>.Update
                            .Set(x => x.ApplicationId, request.ApplicationId)
                            .Set(x => x.Application_Received_date, request.Application_Received_date)
                            .Set(x => x.ApplicationReviewDate, request.ApplicationReviewDate)
                            .Set(x => x.Orgnization_Name, request.Orgnization_Name)
                            .Set(x => x.Constituation_of_Orgnization, request.Constituation_of_Orgnization)
                            .Set(x => x.Fk_Certificate, request.Fk_Certificate)
                            .Set(x => x.Seasonality_Factor, request.Seasonality_Factor)
                            .Set(x => x.AnyAllergens, request.AnyAllergens)
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
                            .Set(x => x.IsDelete, request.IsDelete ?? false)
                            .Set(x => x.IsFinalSubmit, isFinalSubmit ?? false)
                            .Set(x => x.Fk_UserId, request.Fk_UserId ?? UserId)
                            .Set(x => x.AssignTo, request.AssignTo)
                            .Set(x => x.UpdatedAt, now)
                            .Set(x => x.UpdatedBy, UserId)
                            .Set(x => x.Technical_Areas, request.Technical_Areas ?? new List<TechnicalAreasList>())
                            .Set(x => x.Accreditations, request.Accreditations ?? new List<AccreditationsList>())
                            .Set(x => x.CustomerSites, request.CustomerSites ?? new List<ReviewerSiteDetails>())
                            .Set(x => x.reviewerKeyPersonnel, request.KeyPersonnels ?? new List<ReviewerKeyPersonnelList>())
                            .Set(x => x.MandaysLists, request.MandaysLists ?? new List<ReviewerAuditMandaysList>())
                            .Set(x => x.ReviewerThreatList, request.ThreatLists ?? new List<ReviewerThreatList>())
                            .Set(x => x.ReviewerRemarkList, request.RemarkLists ?? new List<ReviewerRemarkList>())
                            .Set(x => x.auditLists, request.auditLists ?? new List<stage1AndStage2Audit>())
                            .Set(x => x.serveillannceAuditLists, request.serveillannceAuditLists ?? new List<ServeillannceAudit>())
                            .Set(x => x.reCertificationAudits, request.reCertificationAudits ?? new List<ReCertificationAudit>())
                            .Set(x => x.transferAudits, request.transferAudits ?? new List<TransferAudit>())
                            .Set(x => x.specialAudits, request.specialAudits ?? new List<SpecialAudit>())
                            .Set(x => x.productCategoryAndSubs, request.productCategoryAndSubs ?? new List<ProductCategoryAndSubCategoryList>())
                            .Set(x => x.hACCPLists, request.hACCPLists ?? new List<HACCPList>())
                            .Set(x => x.standardsLists, request.standardsLists ?? new List<StandardsList>())
                            .Set(x => x.categoryLists, request.categoryLists ?? new List<CategoryList>())
                            .Set(x => x.subCategoryLists, request.subCategoryLists ?? new List<SubCategoryList>());

                        await _fssc.UpdateOneAsync(filter, update);

                        response.Message = "FSSC Application saved successfully.";
                        response.HttpStatusCode = HttpStatusCode.OK;
                        response.Success = true;
                        response.ResponseCode = 0;
                    }
                }
                catch (Exception ex)
                {
                    response.Message = "SaveFSSCApplication Exception: " + ex.Message;
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

        public async Task<addReviewerApplicationResponse> SaveICMEDApplication(addICMEDApplicationRequest request)
        {
            var response = new addReviewerApplicationResponse();
            var UserId = _acc.HttpContext?.Session.GetString("UserId");
            var userFkRole = (await _user.Find(x => x.Id == UserId).FirstOrDefaultAsync())?.Fk_RoleID;
            var department = (await _user.Find(x => x.Id == UserId).FirstOrDefaultAsync())?.Department;
            var userType = (await _role.Find(x => x.Id == userFkRole).FirstOrDefaultAsync())?.roleName;

            if (userType?.Trim().ToLower() == "admin")
            {
                try
                {
                    var now = DateTime.Now;
                    bool? isFinalSubmit = null;
                    DateTime? submitDate = null;
                    string status = "687a2925694d00158c9bf265";

                    if (!string.IsNullOrWhiteSpace(request.IsFinalSubmit))
                    {
                        isFinalSubmit = request.IsFinalSubmit.Trim().ToLower() == "true";

                        if (isFinalSubmit == true)
                        {
                            submitDate = now;
                            status = "687a2925694d00158c9bf267"; // Final submit
                        }
                    }

                    if (!string.IsNullOrEmpty(request.Id))
                    {
                        var filter = Builders<tbl_ICMED_Application>.Filter.Eq(x => x.Id, request.Id);

                        // Clear sub-lists
                        var clearSubLists = Builders<tbl_ICMED_Application>.Update
                            .Set(x => x.Technical_Areas, new List<TechnicalAreasList>())
                            .Set(x => x.Accreditations, new List<AccreditationsList>())
                            .Set(x => x.CustomerSites, new List<ReviewerSiteDetails>())
                            .Set(x => x.reviewerKeyPersonnel, new List<ReviewerKeyPersonnelList>())
                            .Set(x => x.MandaysLists, new List<ReviewerAuditMandaysList>())
                            .Set(x => x.ReviewerThreatList, new List<ReviewerThreatList>())
                            .Set(x => x.ReviewerRemarkList, new List<ReviewerRemarkList>());

                        await _icmed.UpdateOneAsync(filter, clearSubLists);

                        // Update main fields
                        var update = Builders<tbl_ICMED_Application>.Update
                            .Set(x => x.ApplicationId, request.ApplicationId)
                            .Set(x => x.Application_Received_date, request.Application_Received_date)
                            .Set(x => x.ApplicationReviewDate, request.ApplicationReviewDate)
                            .Set(x => x.Orgnization_Name, request.Orgnization_Name)
                            .Set(x => x.Certification_Name, request.Certification_Name)
                            .Set(x => x.Fk_Certificate, request.Fk_Certificate)
                            .Set(x => x.AssignTo, request.AssignTo ?? UserId)
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
                            .Set(x => x.IsDelete, false)
                            .Set(x => x.IsFinalSubmit, isFinalSubmit ?? false)
                            .Set(x => x.Fk_UserId, request.Fk_UserId ?? UserId)
                            .Set(x => x.UpdatedAt, now)
                            .Set(x => x.UpdatedBy, UserId)
                            .Set(x => x.Technical_Areas, request.Technical_Areas ?? new List<TechnicalAreasList>())
                            .Set(x => x.Accreditations, request.Accreditations ?? new List<AccreditationsList>())
                            .Set(x => x.CustomerSites, request.CustomerSites ?? new List<ReviewerSiteDetails>())
                            .Set(x => x.reviewerKeyPersonnel, request.KeyPersonnels ?? new List<ReviewerKeyPersonnelList>())
                            .Set(x => x.MandaysLists, request.MandaysLists ?? new List<ReviewerAuditMandaysList>())
                            .Set(x => x.ReviewerThreatList, request.ThreatLists ?? new List<ReviewerThreatList>())
                            .Set(x => x.ReviewerRemarkList, request.RemarkLists ?? new List<ReviewerRemarkList>());

                        await _icmed.UpdateOneAsync(filter, update);

                        response.Message = "ICMED Application saved successfully.";
                        response.HttpStatusCode = HttpStatusCode.OK;
                        response.Success = true;
                        response.ResponseCode = 0;
                    }
                }
                catch (Exception ex)
                {
                    response.Message = "SaveICMEDApplication Exception: " + ex.Message;
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


        public async Task<addReviewerApplicationResponse> SaveIMDRApplication(addIMDRApplicationRequest request)
        {
            var response = new addReviewerApplicationResponse();
            var UserId = _acc.HttpContext?.Session.GetString("UserId");
            var userFkRole = (await _user.Find(x => x.Id == UserId).FirstOrDefaultAsync())?.Fk_RoleID;
            var department = (await _user.Find(x => x.Id == UserId).FirstOrDefaultAsync())?.Department;
            var userType = (await _role.Find(x => x.Id == userFkRole).FirstOrDefaultAsync())?.roleName;

            if (userType?.Trim().ToLower() == "admin")
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


                        var filter = Builders<tbl_IMDR_Application>.Filter.Eq(x => x.Id, request.Id);

                        // First, clear the sublists (hard delete)
                        var clearSubLists = Builders<tbl_IMDR_Application>.Update
                            .Set(x => x.Technical_Areas, new List<TechnicalAreasList>())
                            .Set(x => x.CustomerSites, new List<ReviewerSiteDetails>())
                            .Set(x => x.KeyPersonnels, new List<KeyPersonnelList>())
                            .Set(x => x.imdrManDays, new List<ImdrManDays>())
                            .Set(x => x.reviewerKeyPersonnel, new List<ReviewerKeyPersonnelList>())
                            .Set(x => x.ReviewerThreatList, new List<ReviewerThreatList>())
                            .Set(x => x.mdrauditLists, new List<MDRAuditList>())
                            .Set(x => x.ReviewerRemarkList, new List<ReviewerRemarkList>());

                        await _imdr.UpdateOneAsync(filter, clearSubLists);

                        // Then, update all fields including new sublist data
                        var update = Builders<tbl_IMDR_Application>.Update
                        .Set(x => x.ApplicationId, request.ApplicationId)
                        .Set(x => x.Application_Received_date, request.Application_Received_date)
                        .Set(x => x.Orgnization_Name, request.Orgnization_Name)
                        .Set(x => x.Fk_Certificate, request.Fk_Certificate)
                        .Set(x => x.AssignTo, request.AssignTo)
                        .Set(x => x.Scop_of_Certification, request.Scop_of_Certification)
                        .Set(x => x.DeviceMasterfile, request.DeviceMasterfile)
                        .Set(x => x.Availbility_of_TechnicalAreas, request.Availbility_of_TechnicalAreas)
                        .Set(x => x.Availbility_of_Auditor, request.Availbility_of_Auditor)
                        .Set(x => x.Audit_Lang, request.Audit_Lang)
                        .Set(x => x.ActiveState, request.ActiveState ?? 1)
                        .Set(x => x.IsInterpreter, request.IsInterpreter)
                        .Set(x => x.UpdatedAt, now)
                        .Set(x => x.CreatedAt, now)
                        .Set(x => x.CreatedBy, UserId)
                        .Set(x => x.UpdatedBy, UserId)
                        .Set(x => x.Status, status)
                        .Set(x => x.IsDelete, false)
                        .Set(x => x.IsFinalSubmit, isFinalSubmit ?? false)
                        .Set(x => x.Fk_UserId, request.Fk_UserId)
                        .Set(x => x.Technical_Areas, request.Technical_Areas ?? new())
                        .Set(x => x.CustomerSites, request.CustomerSites ?? new())
                        .Set(x => x.KeyPersonnels, request.KeyPersonnels ?? new())
                        .Set(x => x.imdrManDays, request.imdrManDays ?? new())
                        .Set(x => x.reviewerKeyPersonnel, request.reviewerKeyPersonnel ?? new())
                        .Set(x => x.ReviewerThreatList, request.ReviewerThreatList ?? new())
                        .Set(x => x.ReviewerRemarkList, request.ReviewerRemarkList ?? new())
                        .Set(x => x.mdrauditLists, request.mdrauditLists ?? new());

                        var result = await _imdr.UpdateOneAsync(filter, update);

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

        public async Task<addReviewerApplicationResponse> SaveICMED_Plus_Application(addICMEDApplicationRequest request)
        {
            var response = new addReviewerApplicationResponse();
            var UserId = _acc.HttpContext?.Session.GetString("UserId");
            var userFkRole = (await _user.Find(x => x.Id == UserId).FirstOrDefaultAsync())?.Fk_RoleID;
            var department = (await _user.Find(x => x.Id == UserId).FirstOrDefaultAsync())?.Department;
            var userType = (await _role.Find(x => x.Id == userFkRole).FirstOrDefaultAsync())?.roleName;

            if (userType?.Trim().ToLower() == "admin")
            {
                try
                {
                    var now = DateTime.Now;
                    bool? isFinalSubmit = null;
                    DateTime? submitDate = null;
                    string status = "687a2925694d00158c9bf265";
                    string Adminstatus = "687a2925694d00158c9bf265";

                    if (!string.IsNullOrWhiteSpace(request.IsFinalSubmit))
                    {
                        isFinalSubmit = request.IsFinalSubmit.Trim().ToLower() == "true";

                        if (isFinalSubmit == true)
                        {
                            submitDate = now;
                            status = "687a2925694d00158c9bf267"; // Final submit
                            if (request.ActiveReviwer == "ReviwerOne")
                            {
                                Adminstatus = "68930d9066a57e1b128af2e9"; // Final Submit status for ReviwerOne
                            }
                            else if (request.ActiveReviwer == "ReviwerTwo")
                            {
                                Adminstatus = "6895d649f3fbe9ce595243cc"; // Final Submit status for ReviwerTwo
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(request.Id))
                    {
                        var filter = Builders<tbl_ICMED_Application>.Filter.Eq(x => x.Id, request.Id);

                        // Clear sub-lists
                        var clearSubLists = Builders<tbl_ICMED_Application>.Update
                            .Set(x => x.Technical_Areas, new List<TechnicalAreasList>())
                            .Set(x => x.Accreditations, new List<AccreditationsList>())
                            .Set(x => x.CustomerSites, new List<ReviewerSiteDetails>())
                            .Set(x => x.reviewerKeyPersonnel, new List<ReviewerKeyPersonnelList>())
                            .Set(x => x.MandaysLists, new List<ReviewerAuditMandaysList>())
                            .Set(x => x.ReviewerThreatList, new List<ReviewerThreatList>())
                            .Set(x => x.ReviewerRemarkList, new List<ReviewerRemarkList>());

                        await _icmed.UpdateOneAsync(filter, clearSubLists);

                        // Update main fields
                        var update = Builders<tbl_ICMED_Application>.Update
                            .Set(x => x.ApplicationId, request.ApplicationId)
                            .Set(x => x.Application_Received_date, request.Application_Received_date)
                            .Set(x => x.ApplicationReviewDate, request.ApplicationReviewDate)
                            .Set(x => x.Orgnization_Name, request.Orgnization_Name)
                            .Set(x => x.Certification_Name, request.Certification_Name)
                            .Set(x => x.Fk_Certificate, request.Fk_Certificate)
                            .Set(x => x.AssignTo, request.AssignTo ?? UserId)
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
                            .Set(x => x.IsDelete, false)
                            .Set(x => x.IsFinalSubmit, isFinalSubmit ?? false)
                            .Set(x => x.Fk_UserId, request.Fk_UserId ?? UserId)
                            .Set(x => x.UpdatedAt, now)
                            .Set(x => x.UpdatedBy, UserId)
                            .Set(x => x.Technical_Areas, request.Technical_Areas ?? new List<TechnicalAreasList>())
                            .Set(x => x.Accreditations, request.Accreditations ?? new List<AccreditationsList>())
                            .Set(x => x.CustomerSites, request.CustomerSites ?? new List<ReviewerSiteDetails>())
                            .Set(x => x.reviewerKeyPersonnel, request.KeyPersonnels ?? new List<ReviewerKeyPersonnelList>())
                            .Set(x => x.MandaysLists, request.MandaysLists ?? new List<ReviewerAuditMandaysList>())
                            .Set(x => x.ReviewerThreatList, request.ThreatLists ?? new List<ReviewerThreatList>())
                            .Set(x => x.ReviewerRemarkList, request.RemarkLists ?? new List<ReviewerRemarkList>());

                        await _icmed.UpdateOneAsync(filter, update);
                        var application = await _customercertificates.Find(x => x.Id == request.ApplicationId).FirstOrDefaultAsync();
                        if (application != null)
                        {
                            var updatestatus = Builders<tbl_customer_certificates>.Update.Set(x => x.status, Adminstatus);
                            await _customercertificates.UpdateOneAsync(x => x.Id == request.ApplicationId, updatestatus);
                        }
                        response.Message = "ICMED Application saved successfully.";
                        response.HttpStatusCode = HttpStatusCode.OK;
                        response.Success = true;
                        response.ResponseCode = 0;
                    }
                }
                catch (Exception ex)
                {
                    response.Message = "SaveICMEDApplication Exception: " + ex.Message;
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


        public async Task<gethistoryResponse> GetHistory(gethistoryRequest request)
        {
            var response = new gethistoryResponse();
            try
            {
                var UserId = _acc.HttpContext?.Session.GetString("UserId");
                if (!string.IsNullOrEmpty(UserId))
                {
                    if (request.CertificationName == "ISO")
                    {
                        var filter = Builders<tbl_ISO_Application>.Filter.And(
                            Builders<tbl_ISO_Application>.Filter.Eq(x => x.ApplicationId, request.ApplicationId),
                            Builders<tbl_ISO_Application>.Filter.Eq(x => x.IsFinalSubmit, true)
                        );

                        var applications = await _iso.Find(filter).ToListAsync();

                        var result = applications
                            .GroupBy(a => a.AssignTo)
                            .Select(g =>
                            {
                                var latestApp = g.OrderByDescending(a => a.UpdatedAt ?? a.CreatedAt).FirstOrDefault();
                                return new ReviewerHistoryDto
                                {
                                    ApplicationId = latestApp.ApplicationId,
                                    AssignPersonName = _mongoDbService.ReviewerName(g.Key),
                                    LatestUpdatedDate = latestApp.UpdatedAt ?? latestApp.CreatedAt
                                };
                            })
                            .ToList();

                        // ✅ Add reviewer history array to response
                        response.ReviewerHistory = result;
                    }

                    else if (request.CertificationName == "FSSC")
                    {
                        var filter = Builders<tbl_FSSC_Application>.Filter.Eq(x => x.ApplicationId, request.ApplicationId);
                        var data = await _fssc.Find(filter)
                                               .SortByDescending(x => x.Id)
                                               .FirstOrDefaultAsync();
                        
                    }
                    else if (request.CertificationName == "ICMED")
                    {
                        var filter = Builders<tbl_ICMED_Application>.Filter.Eq(x => x.ApplicationId, request.ApplicationId);
                        var data = await _icmed.Find(filter)
                                               .SortByDescending(x => x.Id)
                                              .FirstOrDefaultAsync();
                    }
                }
                
            }
            catch (Exception ex)
            {
                response.Message = "GetHistory Exception: " + ex.Message;
                response.HttpStatusCode = System.Net.HttpStatusCode.InternalServerError;
                response.Success = false;
                response.ResponseCode = 1;
            }
            return response;
        }
        protected override void Disposing()
        {
            //throw new NotImplementedException();
        }
    }
}
