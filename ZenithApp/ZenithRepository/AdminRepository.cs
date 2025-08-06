using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Net;
using System.Runtime.ConstrainedExecution;
using System.Threading;
using ZenithApp.CommonServices;
using ZenithApp.Settings;
using ZenithApp.ZenithEntities;
using ZenithApp.ZenithMessage;

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
        private readonly MongoDbService _mongoDbService;



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
            _mongoDbService = mongoDbService;
            _acc = acc;
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
                                    ApplicationId = app.ApplicationId,
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
                    if (!string.IsNullOrWhiteSpace(request.UserId) && !string.IsNullOrWhiteSpace(request.ApplicationId))
                    {
                        var application = await _customercertificates.Find(x => x.Id == request.ApplicationId).FirstOrDefaultAsync();
                        var customerapp = await _customer.Find(x => x.Id == application.Fk_Customer_Application).FirstOrDefaultAsync();
                        if (application != null)
                        {
                            application.AssignTo = request.UserId;
                            application.UpdatedAt = DateTime.Now; // Update the timestamp
                            application.UpdatedBy = userId; // Set the user who updated
                            var status = await _status.Find(x => x.StatusName == "InProgress" && x.IsDelete == false).FirstOrDefaultAsync();
                            if (status != null)
                            {
                                application.status = status.Id;
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
                                            Application_Received_date = DateTime.Now,
                                            Orgnization_Name = customerapp.Orgnization_Name,
                                            Constituation_of_Orgnization = customerapp.Constituation_of_Orgnization,
                                            Fk_Certificate = application.Fk_Certificates,
                                            AssignTo = request.UserId,

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

                                            CreatedAt = DateTime.Now,
                                            CreatedBy = userId,
                                            Status = status.Id,
                                            Application_Status = status.Id,
                                            IsDelete = false,
                                            IsFinalSubmit = false,
                                            Fk_UserId = request.UserId
                                        });

                                        break;


                                    case "FSSC":
                                        await _fssc.InsertOneAsync(new tbl_FSSC_Application
                                        {
                                            ApplicationId = request.ApplicationId,
                                            Application_Received_date = DateTime.Now,
                                            Orgnization_Name = customerapp.Orgnization_Name,
                                            Constituation_of_Orgnization = customerapp.Constituation_of_Orgnization,
                                            Fk_Certificate = application.Fk_Certificates,
                                            AssignTo = request.UserId,
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

                                            CreatedAt = DateTime.Now,
                                            CreatedBy = userId,
                                            Status = status.Id,
                                            IsDelete = false,
                                            IsFinalSubmit = false,
                                            Fk_UserId = request.UserId

                                        });
                                        break;

                                    //case "ICMED":
                                    //    await _icmedApplication.InsertOneAsync(new tbl_ICMED_Application
                                    //    {
                                    //        ApplicationId = request.ApplicationId,
                                    //        CreatedAt = DateTime.Now,
                                    //        // other fields...
                                    //    });
                                    //    break;



                                    //case "Other1":
                                    //    await _other1Application.InsertOneAsync(new tbl_Other1_Application { /*...*/ });
                                    //    break;

                                    // Add more cases as per your certificates...

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
                        var userList = await _user
                        .Find(x => x.IsDelete == 0 && x.Department.Trim().ToLower() == request.type.Trim().ToLower()).
                        Project(x => new UserDropdownDto
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
                    if (!string.IsNullOrWhiteSpace(request.UserId) && !string.IsNullOrWhiteSpace(request.ApplicationId))
                    {
                        var application = await _isoApplication.Find(x => x.Id == request.ApplicationId).FirstOrDefaultAsync();
                        if (application != null)
                        {
                            var masterCert = await _masterCertificate
                                .Find(x => x.Id == request.Certification_Id)
                                .FirstOrDefaultAsync();

                            switch (masterCert.Certificate_Name)
                            {
                                case "ISO":
                                    await _isoApplication.InsertOneAsync(new tbl_ISO_Application
                                    {
                                        Application_Received_date = application.Application_Received_date,
                                        Orgnization_Name = application.Orgnization_Name,
                                        Constituation_of_Orgnization = application.Constituation_of_Orgnization,
                                        Fk_Certificate = application.Fk_Certificate,
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
                                        Fk_UserId = request.UserId,
                                        Technical_Areas = application.Technical_Areas ?? new List<TechnicalAreasList>(),
                                        Accreditations = application.Accreditations ?? new List<AccreditationsList>(),
                                        CustomerSites = application.CustomerSites ?? new List<ReviewerSiteDetails>(),
                                        reviewerKeyPersonnel = application.reviewerKeyPersonnel ?? new List<ReviewerKeyPersonnelList>(),
                                        MandaysLists = application.MandaysLists ?? new List<ReviewerAuditMandaysList>(),
                                        ReviewerThreatList = application.ReviewerThreatList ?? new List<ReviewerThreatList>(),
                                        ReviewerRemarkList = application.ReviewerRemarkList ?? new List<ReviewerRemarkList>(),
                                        CreatedAt = DateTime.Now,
                                        CreatedBy = request.UserId
                                    });

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

                        var data = await _iso.Find(filter).FirstOrDefaultAsync();
                        var cerificateName = _mongoDbService.Getcertificatename(data.Fk_Certificate);
                        var assignmane = _mongoDbService.ReviewerName(data.AssignTo);
                        var status = _mongoDbService.StatusName(data.Status);


                        response.Data = data;
                        response.CertificateName = cerificateName;
                        response.statusName = status;
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
        protected override void Disposing()
        {
            //throw new NotImplementedException();
        }
    }
}
