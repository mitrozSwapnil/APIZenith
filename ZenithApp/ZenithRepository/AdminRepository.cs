using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Runtime.ConstrainedExecution;
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



        private readonly IHttpContextAccessor _acc;

        public AdminRepository(IOptions<MongoDbSettings> settings, IHttpContextAccessor acc)
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
                                    AssignedUserName = assignedUser?.FullName,
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

                                    //case "ICMED":
                                    //    await _icmedApplication.InsertOneAsync(new tbl_ICMED_Application
                                    //    {
                                    //        ApplicationId = request.ApplicationId,
                                    //        CreatedAt = DateTime.Now,
                                    //        // other fields...
                                    //    });
                                    //    break;

                                    //case "FSSC":
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

        protected override void Disposing()
        {
            //throw new NotImplementedException();
        }
    }
}
