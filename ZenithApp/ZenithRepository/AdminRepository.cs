using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.Net;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Threading;
using ZenithApp.CommonServices;
using ZenithApp.Settings;
using ZenithApp.ZenithEntities;
using ZenithApp.ZenithMessage;
using static System.Net.Mime.MediaTypeNames;
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
        private readonly IMongoCollection<tbl_quoatation> _quoatation;

        private readonly IMongoCollection<tbl_Audit> _audit;
        private readonly IMongoCollection<tbl_dynamic_audit_template> _dynamic;


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
            _quoatation = database.GetCollection<tbl_quoatation>("tbl_quoatation");
            _audit = database.GetCollection<tbl_Audit>("tbl_Audit");
            _dynamic = database.GetCollection<tbl_dynamic_audit_template>("tbl_dynamic_audit_template");
            _icmedplus = database.GetCollection<tbl_ICMED_PLUS_Application>("tbl_ICMED_PLUS_Application");

        }
        //new code
        public async Task<getDashboardResponse> GetAdminDashboard(getDashboardRequest request)
        {
            var response = new getDashboardResponse();

            var userId = _acc.HttpContext?.Session.GetString("UserId");
            var userFkRole = (await _user.Find(x => x.Id == userId).FirstOrDefaultAsync())?.Fk_RoleID;
            var usertype = (await _role.Find(x => x.Id == userFkRole).FirstOrDefaultAsync())?.roleName;

            if (usertype?.Trim().ToLower() != "admin")
            {
                response.Message = "Invalid token or user role.";
                response.Success = false;
                response.HttpStatusCode = System.Net.HttpStatusCode.Forbidden;
                response.ResponseCode = 1;
                return response;
            }

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

                // 🔹 Step 1: Filter for submitted applications
                var applicationFilter = Builders<tbl_customer_application>.Filter.And(
                    Builders<tbl_customer_application>.Filter.Eq(x => x.IsDelete, false),
                    Builders<tbl_customer_application>.Filter.Eq(x => x.IsFinalSubmit, true)
                );

                // 🔹 Step 2: Get applications
                var applications = await _customer.Find(applicationFilter).ToListAsync();

                // 🔹 Step 3: Handle status filter (optional)
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

                // 🔹 Step 4: Loop through applications and flatten certificates
                foreach (var app in applications)
                {
                    if (app.Fk_ApplicationCertificates == null || !app.Fk_ApplicationCertificates.Any())
                        continue;

                    foreach (var cert in app.Fk_ApplicationCertificates)
                    {
                        // Apply optional status filter
                        if (filterByStatus && cert.CertificationType != statusRecord?.StatusName)
                            continue;


                        // 🔹 Resolve Assigned Users (Null Safe)
                        string reviewerName = null;
                        string traineeName = null;

                        if (cert?.AssignTo != null && cert.AssignTo.Any())
                        {
                            var reviewer = cert.AssignTo
                                .FirstOrDefault(a => a?.Role != null && a.Role.Equals("Reviewer", StringComparison.OrdinalIgnoreCase));

                            var trainee = cert.AssignTo
                                .FirstOrDefault(a => a?.Role != null && a.Role.Equals("Trainee", StringComparison.OrdinalIgnoreCase));

                            if (reviewer?.UserId != null)
                            {
                                var reviewerUser = await _user.Find(x => x.Id == reviewer.UserId).FirstOrDefaultAsync();
                                reviewerName = reviewerUser?.FullName; // use FullName only
                            }

                            if (trainee?.UserId != null)
                            {
                                var traineeUser = await _user.Find(x => x.Id == trainee.UserId).FirstOrDefaultAsync();
                                traineeName = traineeUser?.FullName; // use FullName only
                            }
                        }
                        // Safely fetch status name
                        string statusName = "Pending";
                        if (!string.IsNullOrEmpty(cert.Status))
                        {
                            var statusDoc = await _status.Find(x => x.Id == cert.Status).FirstOrDefaultAsync();
                            if (statusDoc != null && !string.IsNullOrEmpty(statusDoc.StatusName))
                                statusName = statusDoc.StatusName;
                        }


                        var dashboardRecord = new CustomerDashboardData
                        {
                            ApplicationId = app.Id,
                            sub_applicationId = cert.Id,
                            ApplicationName = app.ApplicationName,
                            ReceiveDate = app.SubmitDate,
                            IsFinal = app.IsFinalSubmit,
                            CompanyName = app.Orgnization_Name,
                            Certification_Name = cert.Certificate_Name,
                            Certification_Id = cert.Id,
                            Status = statusName,
                            TargetDate = cert.TargetDate, // can map if you add one in future
                            TraineeName = traineeName, // add if you assign users later
                            ReviewerName = reviewerName,
                            totalComments = 0 // will fill later
                        };

                        dashboardList.Add(dashboardRecord);
                    }
                }

                // 🔹 Step 5: Optional search filter
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

                // 🔹 Step 6: Sort & Pagination
                dashboardList = dashboardList.OrderByDescending(x => x.ReceiveDate).ToList();

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

                // 🔹 Step 7: Comment count logic
                foreach (var app in paginatedList)
                {
                    var filter = Builders<tbl_ApplicationFieldComment>.Filter.And(
                        Builders<tbl_ApplicationFieldComment>.Filter.Eq(c => c.ApplicationId, app.Id),
                        Builders<tbl_ApplicationFieldComment>.Filter.Eq(c => c.CertificationName, app.ApplicationName),
                        Builders<tbl_ApplicationFieldComment>.Filter.Eq(c => c.isResolved, false)
                    );

                    var commentList = await _comments
                        .Find(filter)
                        .SortByDescending(c => c.CreatedOn)
                        .ToListAsync();

                    var latestUniqueComments = commentList
                        .GroupBy(c => c.FieldName)
                        .Select(g => g.First())
                        .ToList();

                    app.totalComments = latestUniqueComments.Count;
                }

                // 🔹 Step 8: Panel counts (Applications / Quotations / Audits)
                var totalApplications = applications.Count;
                var totalQuotations = await _quoatation.CountDocumentsAsync(FilterDefinition<tbl_quoatation>.Empty);
                var totalAudit = await _audit.CountDocumentsAsync(FilterDefinition<tbl_Audit>.Empty);

                var panelData = new PannelDto
                {
                    totalApplication = totalApplications,
                    totalQuotation = (int)totalQuotations,
                    totalAuditFile = (int)totalAudit,
                    other = 0
                };

                // 🔹 Step 9: Response
                response.Data = paginatedList;
                response.Pagination = pagination;
                response.Pannale = panelData;
                response.Message = "Dashboard Data fetched successfully.";
                response.HttpStatusCode = System.Net.HttpStatusCode.OK;
                response.Success = true;
                response.ResponseCode = 0;
            }
            catch (Exception ex)
            {
                response.Message = "GetAdminDashboard Exception: " + ex.Message;
                response.Success = false;
                response.HttpStatusCode = System.Net.HttpStatusCode.InternalServerError;
                response.ResponseCode = 1;
            }

            return response;
        }

        //end new code



        //public async Task<getDashboardResponse> GetAdminDashboard(getDashboardRequest request)
        //{
        //    var response = new getDashboardResponse();

        //    var userId = _acc.HttpContext?.Session.GetString("UserId");
        //    var userFkRole = (await _user.Find(x => x.Id == userId).FirstOrDefaultAsync())?.Fk_RoleID;
        //    var usertype = (await _role.Find(x => x.Id == userFkRole).FirstOrDefaultAsync())?.roleName;

        //    if (usertype?.Trim().ToLower() == "admin")
        //    {
        //        try
        //        {
        //            if (string.IsNullOrEmpty(userId))
        //            {
        //                response.Message = "Session expired or invalid user.";
        //                response.Success = false;
        //                response.HttpStatusCode = System.Net.HttpStatusCode.Unauthorized;
        //                response.ResponseCode = 1;
        //                return response;
        //            }

        //            var dashboardList = new List<CustomerDashboardData>();

        //            // Step 1 — Prepare default application filter (no status filter here)
        //            var applicationFilter = Builders<tbl_customer_application>.Filter.And(
        //                Builders<tbl_customer_application>.Filter.Eq(x => x.IsDelete, false),
        //                Builders<tbl_customer_application>.Filter.Eq(x => x.IsFinalSubmit, true)
        //            );

        //            // Step 2 — Fetch all applications
        //            var applications = await _customer.Find(applicationFilter).ToListAsync();

        //            // Step 3 — Optional: Find status record once if flag is provided
        //            tbl_Status? statusRecord = null;
        //            bool filterByStatus = false;

        //            if (!string.IsNullOrWhiteSpace(request.Flag))
        //            {
        //                statusRecord = await _status
        //                    .Find(x => x.StatusName.ToLower() == request.Flag.Trim().ToLower())
        //                    .FirstOrDefaultAsync();

        //                if (statusRecord == null)
        //                {
        //                    return new getDashboardResponse
        //                    {
        //                        Success = true,
        //                        Message = "No records found for given status.",
        //                        Data = new List<CustomerDashboardData>(),
        //                        HttpStatusCode = System.Net.HttpStatusCode.OK
        //                    };
        //                }

        //                filterByStatus = true;
        //            }

        //            // Step 4 — Loop over each application and its certificates
        //            foreach (var app in applications)
        //            {
        //                var certificates = await _customercertificates
        //                    .Find(x => x.Fk_Customer_Application == app.Id && x.Is_Delete == false)
        //                    .ToListAsync();

        //                foreach (var cert in certificates)
        //                {
        //                    // Apply status filter at certificate level only if a flag was provided
        //                    if (filterByStatus && cert.status != statusRecord.Id)
        //                    {
        //                        continue; // Skip this certificate
        //                    }

        //                    var masterCert = await _masterCertificate
        //                        .Find(x => x.Id == cert.Fk_Certificates)
        //                        .FirstOrDefaultAsync();


        //                    var statusDoc = await _status
        //                       .Find(x => x.Id == cert.status)
        //                       .FirstOrDefaultAsync();

        //                    string nameStatus = statusDoc?.StatusName; // null if not found




        //                    if (masterCert != null)
        //                    {
        //                        //var assignedUser = !string.IsNullOrWhiteSpace(cert.AssignTo)
        //                        //    ? await _user.Find(x => x.Id == cert.AssignTo && x.IsDelete == 0)
        //                        //        .FirstOrDefaultAsync()
        //                        //    : null;
        //                        var subApplicationId = "";
        //                        var certificatetype = masterCert.Certificate_Name.Trim().ToUpper();

        //                        if (certificatetype == "ISO")
        //                        {
        //                            subApplicationId = (await _iso
        //                                .Find(x => x.Fk_Certificate == masterCert.Id && x.ApplicationId == cert.Id)
        //                                .FirstOrDefaultAsync())?.Id;
        //                        }
        //                        else if (certificatetype == "FSSC")
        //                        {
        //                            subApplicationId = (await _fssc
        //                                .Find(x => x.Fk_Certificate == masterCert.Id && x.ApplicationId == cert.Id)
        //                                .FirstOrDefaultAsync())?.Id;
        //                        }
        //                        else if (certificatetype == "ICMED")
        //                        {
        //                            subApplicationId = (await _icmed
        //                                .Find(x => x.Fk_Certificate == masterCert.Id && x.ApplicationId == cert.Id)
        //                                .FirstOrDefaultAsync())?.Id;
        //                        }
        //                        else if (certificatetype == "IMDR")
        //                        {
        //                            subApplicationId = (await _imdr
        //                                .Find(x => x.Fk_Certificate == masterCert.Id && x.ApplicationId == cert.Id)
        //                                .FirstOrDefaultAsync())?.Id;
        //                        }


        //                        string traineeName = null;
        //                        string reviewerName = null;

        //                        if (cert.AssignTo != null && cert.AssignTo.Any())
        //                        {
        //                            // Get all user IDs from AssignTo
        //                            var userIds = cert.AssignTo.Select(a => a.UserId).ToList();

        //                            // Fetch users from DB
        //                            var assignedUsers = await _user
        //                                .Find(x => userIds.Contains(x.Id) && x.IsDelete == 0)
        //                                .ToListAsync();

        //                            // Match names based on role
        //                            foreach (var assign in cert.AssignTo)
        //                            {
        //                                var user = assignedUsers.FirstOrDefault(u => u.Id == assign.UserId);
        //                                if (user != null)
        //                                {
        //                                    if (assign.Role.Equals("Trainee", StringComparison.OrdinalIgnoreCase))
        //                                        traineeName = user.FullName;

        //                                    else if (assign.Role.Equals("Reviewer", StringComparison.OrdinalIgnoreCase))
        //                                        reviewerName = user.FullName;
        //                                }
        //                            }
        //                        }



        //                        var isoFilter = Builders<tbl_ISO_Application>.Filter.And(
        //                                        Builders<tbl_ISO_Application>.Filter.Eq(x => x.ApplicationId, cert.Id),
        //                                        Builders<tbl_ISO_Application>.Filter.Eq(x => x.Fk_Certificate, masterCert.Id)
        //                                    );

        //                        var isoApplication = await _iso.Find(isoFilter).FirstOrDefaultAsync();


        //                        var dashboardRecord = new CustomerDashboardData
        //                        {
        //                            Id = cert.Id,
        //                            ApplicationId = cert.Id,
        //                            sub_applicationId = subApplicationId,
        //                            ApplicationName  =app.ApplicationId,
        //                            ReceiveDate = app.SubmitDate,
        //                            IsFinal=app.IsFinalSubmit,
        //                            CompanyName = app.Orgnization_Name,
        //                            Certification_Name = masterCert.Certificate_Name,
        //                            Certification_Id = masterCert.Id,
        //                            TargetDate = cert.TargetDate,
        //                            //Status = isoApplication != null
        //                            //        ? (await _status.Find(x => x.Id == isoApplication.Status).FirstOrDefaultAsync())?.StatusName ?? "Pending"
        //                            //        : (await _status.Find(x => x.Id == cert.status).FirstOrDefaultAsync())?.StatusName ?? "Pending",
        //                            Status = nameStatus,
        //                            //AssignedUserName = string.Join(", ", assignedUserNames),
        //                            TraineeName = traineeName,
        //                            ReviewerName = reviewerName
        //                        };

        //                        dashboardList.Add(dashboardRecord);
        //                    }
        //                }
        //            }

        //            // Step 5 — Search filter (optional)
        //            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        //            {
        //                var searchTerm = request.SearchTerm.Trim().ToLower();
        //                dashboardList = dashboardList
        //                    .Where(x =>
        //                        (!string.IsNullOrEmpty(x.ApplicationId) && x.ApplicationId.ToLower().Contains(searchTerm)) ||
        //                        (!string.IsNullOrEmpty(x.Certification_Name) && x.Certification_Name.ToLower().Contains(searchTerm))
        //                    )
        //                    .ToList();
        //            }

        //            // Step 6 — Pagination
        //            // Sort by ReceiveDate descending before pagination
        //            dashboardList = dashboardList
        //                .OrderByDescending(x => x.ReceiveDate)
        //                .ToList();
        //            foreach (var app in dashboardList)
        //            {
        //                // ---- Step 1: Build filter for current application ----
        //                var certId = app.Certification_Id; // adjust this property name as per your model

        //                var filter = Builders<tbl_ApplicationFieldComment>.Filter.And(
        //                    Builders<tbl_ApplicationFieldComment>.Filter.Eq(c => c.ApplicationId, app.ApplicationId),
        //                    Builders<tbl_ApplicationFieldComment>.Filter.Eq(c => c.CertificationName, certId),
        //                    Builders<tbl_ApplicationFieldComment>.Filter.Eq(c => c.isResolved, false)
        //                );

        //                // ---- Step 2: Fetch comments for this application ----
        //                var commentList = await _comments
        //                    .Find(filter)
        //                    .SortByDescending(c => c.CreatedOn) // latest first
        //                    .ToListAsync();

        //                // ---- Step 3: Group by FieldName and take latest only ----
        //                var latestUniqueComments = commentList
        //                    .GroupBy(c => c.FieldName)
        //                    .Select(g => g.First()) // latest due to sorting
        //                    .ToList();

        //                // ---- Step 4: Count only the unique unresolved ones ----
        //                var commentCount = latestUniqueComments.Count;

        //                // Add it to your dashboard object
        //                app.totalComments = (int)commentCount;
        //            }
        //            var totalCount = dashboardList.Count;
        //            var skip = (request.PageNumber - 1) * request.PageSize;

        //            var paginatedList = dashboardList
        //                .Skip(skip)
        //                .Take(request.PageSize)
        //                .ToList();


        //            var pagination = new PageinationDto
        //            {
        //                PageNumber = request.PageNumber,
        //                PageSize = request.PageSize,
        //                TotalRecords = totalCount
        //            };


        //            var applications1 = await _customer.Find(applicationFilter).ToListAsync();

        //            // Step 2: get all application Ids
        //            var appIds = applications.Select(a => a.Id).ToList();

        //            // Step 3: count certificates linked to those applications
        //            var certFilter = Builders<tbl_customer_certificates>.Filter.And(
        //                Builders<tbl_customer_certificates>.Filter.In(x => x.Fk_Customer_Application, appIds),
        //                Builders<tbl_customer_certificates>.Filter.Eq(x => x.Is_Delete, false)
        //            );

        //            var totalApplications = await _customercertificates.CountDocumentsAsync(certFilter);

        //            var totalQuotations = await _quoatation.CountDocumentsAsync(FilterDefinition<tbl_quoatation>.Empty);
        //            var totalAudit = await _audit.CountDocumentsAsync(FilterDefinition<tbl_Audit>.Empty);


        //            var pannelData = new PannelDto
        //            {
        //                totalApplication = (int)totalApplications,
        //                totalQuotation = (int)totalQuotations,
        //                totalAuditFile = (int)totalAudit,
        //                other = 0
        //            };


        //            // Step 7 — Final response
        //            response.Data = paginatedList;
        //            response.Pagination = pagination;
        //            response.Pannale = pannelData;
        //            response.Message = "Dashboard Data fetched successfully.";
        //            response.HttpStatusCode = System.Net.HttpStatusCode.OK;
        //            response.Success = true;
        //            response.ResponseCode = 0;

        //        }
        //        catch (Exception ex)
        //        {
        //            response.Message = "GetCustomerDashboard Exception: " + ex.Message;
        //            response.Success = false;
        //            response.HttpStatusCode = System.Net.HttpStatusCode.InternalServerError;
        //            response.ResponseCode = 1;
        //        }
        //    }
        //    else
        //    {
        //        response.Message = "Invalid Token.";
        //        response.Success = false;
        //        response.HttpStatusCode = System.Net.HttpStatusCode.Unauthorized;
        //        response.ResponseCode = 1;
        //    }

        //    return response;
        //}
        public class AssignedUser
        {
            [BsonRepresentation(BsonType.ObjectId)]
            public string UserId { get; set; }
            public string Role { get; set; } // e.g. "Trainee", "Reviewer"
        }

        public async Task<assignUserResponse> AssignApplication(assignUserRequest request)
        {
            var response = new assignUserResponse();
            var userId = _acc.HttpContext?.Session.GetString("UserId");

            try
            {
                // 🔹 Validate Admin Role
                var userFkRole = (await _user.Find(x => x.Id == userId).FirstOrDefaultAsync())?.Fk_RoleID;
                var userType = (await _role.Find(x => x.Id == userFkRole).FirstOrDefaultAsync())?.roleName;

                if (userType?.Trim().ToLower() != "admin")
                {
                    response.Message = "Invalid Token.";
                    response.Success = false;
                    response.HttpStatusCode = System.Net.HttpStatusCode.Unauthorized;
                    response.ResponseCode = 1;
                    return response;
                }

                // 🔹 Validate Inputs
                if (string.IsNullOrWhiteSpace(request.ApplicationId) ||
                    string.IsNullOrWhiteSpace(request.ReviewerId) ||
                    string.IsNullOrWhiteSpace(request.Certification_Id))
                {
                    response.Message = "Please provide ApplicationId, Certification_Id and ReviewerId.";
                    response.Success = false;
                    response.HttpStatusCode = System.Net.HttpStatusCode.BadRequest;
                    response.ResponseCode = 1;
                    return response;
                }

                // 🔹 Find parent customer application
                var customerApp = await _customer.Find(x => x.Id == request.ApplicationId && !x.IsDelete).FirstOrDefaultAsync();
                if (customerApp == null)
                {
                    response.Message = "Customer application not found.";
                    response.Success = false;
                    response.HttpStatusCode = System.Net.HttpStatusCode.NotFound;
                    response.ResponseCode = 1;
                    return response;
                }

                // 🔹 Find target certificate within Fk_ApplicationCertificates
                var targetCert = customerApp.Fk_ApplicationCertificates?
                    .FirstOrDefault(c => c.Id == request.Certification_Id);

                if (targetCert == null)
                {
                    response.Message = "Certificate not found inside customer application.";
                    response.Success = false;
                    response.HttpStatusCode = System.Net.HttpStatusCode.NotFound;
                    response.ResponseCode = 1;
                    return response;
                }

                // 🔹 Update sub-document (certificate) assignment metadata
                var status = await _status.Find(x => x.StatusName == "InProgress").FirstOrDefaultAsync();
                var statusId = status?.Id ?? "68835335b8054bb3d2914cae";

                targetCert.AssignTo = new List<AssignedUser>
                {
                    new AssignedUser { UserId = request.ReviewerId, Role = "Reviewer" }
                };
                if (!string.IsNullOrWhiteSpace(request.TrineeId))
                    targetCert.AssignTo.Add(new AssignedUser { UserId = request.TrineeId, Role = "Trainee" });

                targetCert.TargetDate = request.targetDate;
                targetCert.Status = statusId;

                // 🔹 Replace certificate in list
                var certList = customerApp.Fk_ApplicationCertificates?.Select(c =>
                    c.Id == request.Certification_Id ? targetCert : c).ToList();

                // 🔹 Update database
                var update = Builders<tbl_customer_application>.Update
                    .Set(x => x.Fk_ApplicationCertificates, certList)
                    .Set(x => x.UpdatedAt, DateTime.Now)
                    .Set(x => x.UpdatedBy, userId);

                await _customer.UpdateOneAsync(x => x.Id == customerApp.Id, update);

                // 🔹 Prepare data for new certificate-specific table
                var siteList = customerApp.Fk_Customer_Sites ?? new List<CustomerSite>();
                var keyPersonnelList = customerApp.Fk_Key_Personnels ?? new List<KeyPersonnel>();
                var mandayList = await _reviewerAuditManDays
                    .Find(x => x.Fk_ApplicationId == request.ApplicationId)
                    .ToListAsync();

                var masterCert = await _masterCertificate
                    .Find(x => x.Id == request.Certification_Id)
                    .FirstOrDefaultAsync();

                if (masterCert == null)
                {
                    response.Message = "Master certificate not found.";
                    response.Success = false;
                    response.HttpStatusCode = System.Net.HttpStatusCode.NotFound;
                    response.ResponseCode = 1;
                    return response;
                }

                // 🔹 Map shared lists
                var reviewerSites = siteList.Select(s => new ReviewerSiteDetails
                {
                    Customer_SiteId = s.Id,
                    Address = s.Address,
                    Telephone = s.Telephone,
                    Web = s.Web,
                    Email = s.Email,
                    Activity_Department = s.Activity_Department,
                    Manpower = s.Manpower,
                    Shift_Name = s.Shift_Name
                }).ToList();

                var reviewerKeyPersonnels = keyPersonnelList.Select(kp => new ReviewerKeyPersonnelList
                {
                    ActivityName = kp.Designation,
                    PersonnelByClient = keyPersonnelList.Count(p => p.Designation == kp.Designation).ToString(),
                    EffectivePersonnel = "",
                    Comment = "",
                    AdditionalComments = "",
                    Fk_site = null
                }).ToList();
                var keyPersonnelsList = keyPersonnelList.Select(kp => new KeyPersonnelList
                {
                    customerKeyPersonnelId = kp.Id,
                    Name = kp.Name,
                    Designation = kp.Designation,
                    EmailId = kp.EmailId,
                    Contact_No = string.IsNullOrEmpty(kp.Contact_No) ? kp.Contact_No : kp.Contact_No,  // pick available field
                    Type = kp.Type
                }).ToList();

                var reviewerMandays = mandayList.Select(x => new ReviewerAuditMandaysList
                {
                    ActivityName = x.ActivityName,
                    Audit_ManDays = x.Man_Days,
                    Additional_ManDays = x.Additional_Mandays,
                    OnSite_Stage1_ManDays = x.OnSite_manDays_Stage_1,
                    OnSite_Stage2_ManDays = x.OnSite_manDays_Stage_2,
                    OffSite_Stage1_ManDays = x.OfSite_manDays_Stage_1,
                    OffSite_Stage2_ManDays = x.OfSite_manDays_Stage_2,
                    AdditionalComments = x.Comment,
                    Note = x.Note
                }).ToList();

                switch (masterCert.Certificate_Name)
                {
                    case "ISO":

                        await _isoApplication.InsertOneAsync(new tbl_ISO_Application
                        {
                            ApplicationId = request.ApplicationId,
                            ApplicationName =customerApp.ApplicationId,
                            Application_Received_date = DateTime.Now,
                            CertiFicateName = masterCert.Certificate_Name,
                            ActiveReviwer = request.ApplicationId ?? "ReviwerTwo",
                            Orgnization_Name = customerApp.Orgnization_Name,
                            Constituation_of_Orgnization = customerApp.Constituation_of_Orgnization,
                            Fk_Certificate = request.Certification_Id,
                            AssignTo = request.ReviewerId,
                            TargetDate = request.targetDate,
                            Audit_Type = "",  // Set based on logic or request
                            Scop_of_Certification = "",
                            Technical_Areas = new List<TechnicalAreasList>(),
                            Accreditations = new List<AccreditationsList>(),
                            Availbility_of_TechnicalAreas = false,
                            Availbility_of_Auditor = false,
                            Audit_Lang = "",
                            IsInterpreter = false,
                            IsMultisitesampling = false,
                            Total_site = reviewerSites?.Count ?? 0,  // <-- Set site count

                            Sample_Site = new List<LabelValue>(),   // If required, fill here
                            Shift_Details = new List<LabelValue>(), // If required, fill here

                            CustomerSites = reviewerSites,
                            KeyPersonnels = keyPersonnelsList,
                            MandaysLists = reviewerMandays,
                            reviewerKeyPersonnel = reviewerKeyPersonnels,
                            CreatedAt = DateTime.Now,
                            CreatedBy = userId,
                            Status = status.Id,
                            Application_Status = status.Id,
                            IsDelete = false,
                            IsFinalSubmit = false,
                            Fk_UserId = request.ReviewerId
                        });
                        if (!string.IsNullOrWhiteSpace(request.TrineeId))
                        {
                            await _isoApplication.InsertOneAsync(new tbl_ISO_Application
                            {
                                ApplicationId = request.ApplicationId,
                                ApplicationName = customerApp.ApplicationId,
                                CertiFicateName = masterCert.Certificate_Name,
                                Application_Received_date = DateTime.Now,
                                ActiveReviwer = request.ApplicationId ?? "ReviwerOne",
                                Orgnization_Name = customerApp.Orgnization_Name,
                                Constituation_of_Orgnization = customerApp.Constituation_of_Orgnization,
                                Fk_Certificate = request.Certification_Id,
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
                                Total_site = reviewerSites?.Count ?? 0,  // <-- Set site count
                                TargetDate = request.targetDate,
                                Sample_Site = new List<LabelValue>(),   // If required, fill here
                                Shift_Details = new List<LabelValue>(), // If required, fill here

                                CustomerSites = reviewerSites,
                                KeyPersonnels = keyPersonnelsList,
                                MandaysLists = reviewerMandays,
                                reviewerKeyPersonnel = reviewerKeyPersonnels,
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
                            ActiveReviwer = request.ApplicationId ?? "ReviewerTwo",
                            ApplicationName = customerApp.ApplicationId,
                            Orgnization_Name = customerApp.Orgnization_Name,
                            Constituation_of_Orgnization = customerApp.Constituation_of_Orgnization,

                            Fk_Certificate = request.Certification_Id,
                            AssignTo = request.ReviewerId,

                            Audit_Type = "",   // Set based on logic
                            Scop_of_Certification = "",

                            Availbility_of_TechnicalAreas = false,
                            Availbility_of_Auditor = false,
                            Audit_Lang = "",

                            IsInterpreter = false,
                            IsMultisitesampling = false,
                            Total_site = reviewerSites?.Count ?? 0,

                            Sample_Site = new List<LabelValue>(),
                            Shift_Details = new List<LabelValue>(),

                            Seasonality_Factor = null,
                            AnyAllergens = null,

                            CustomerSites = reviewerSites,
                            KeyPersonnels = keyPersonnelsList,
                            MandaysLists = reviewerMandays,
                            reviewerKeyPersonnel = reviewerKeyPersonnels,

                            Technical_Areas = new List<TechnicalAreasList>(),
                            Accreditations = new List<AccreditationsList>(),
                            productCategoryAndSubs = new List<ProductCategoryAndSubCategoryList>(),
                            hACCPLists = new List<HACCPList>(),
                            standardsLists = new List<StandardsList>(),
                            categoryLists = new List<CategoryList>(),
                            subCategoryLists = new List<SubCategoryList>(),

                            ReviewerThreatList = new List<ReviewerThreatList>(),
                            ReviewerRemarkList = new List<ReviewerRemarkList>(),
                            TargetDate = request.targetDate,
                            CreatedAt = DateTime.Now,
                            CreatedBy = userId,
                            Status = status.Id,
                            IsDelete = false,
                            IsFinalSubmit = false,
                            Fk_UserId = request.ReviewerId
                        });
                        if (!string.IsNullOrWhiteSpace(request.TrineeId))
                        {
                            await _fssc.InsertOneAsync(new tbl_FSSC_Application
                            {
                                ApplicationId = request.ApplicationId,
                                Application_Received_date = DateTime.Now,
                                ActiveReviwer = request.ApplicationId ?? "ReviewerOne",
                                ApplicationName = customerApp.ApplicationId,
                                Orgnization_Name = customerApp.Orgnization_Name,
                                Constituation_of_Orgnization = customerApp.Constituation_of_Orgnization,

                                Fk_Certificate = request.Certification_Id,
                                AssignTo = request.TrineeId,
                                TargetDate = request.targetDate,
                                Audit_Type = "",   // Set based on logic
                                Scop_of_Certification = "",

                                Availbility_of_TechnicalAreas = false,
                                Availbility_of_Auditor = false,
                                Audit_Lang = "",

                                IsInterpreter = false,
                                IsMultisitesampling = false,
                                Total_site = reviewerSites?.Count ?? 0,

                                Sample_Site = new List<LabelValue>(),
                                Shift_Details = new List<LabelValue>(),

                                Seasonality_Factor = null,
                                AnyAllergens = null,

                                CustomerSites = reviewerSites,
                                KeyPersonnels = keyPersonnelsList,
                                MandaysLists = reviewerMandays,
                                reviewerKeyPersonnel = reviewerKeyPersonnels,

                                Technical_Areas = new List<TechnicalAreasList>(),
                                Accreditations = new List<AccreditationsList>(),
                                productCategoryAndSubs = new List<ProductCategoryAndSubCategoryList>(),
                                hACCPLists = new List<HACCPList>(),
                                standardsLists = new List<StandardsList>(),
                                categoryLists = new List<CategoryList>(),
                                subCategoryLists = new List<SubCategoryList>(),

                                ReviewerThreatList = new List<ReviewerThreatList>(),
                                ReviewerRemarkList = new List<ReviewerRemarkList>(),

                                CreatedAt = DateTime.Now,
                                CreatedBy = userId,

                                Status = status.Id,
                                IsDelete = false,
                                IsFinalSubmit = false,
                                Fk_UserId = request.TrineeId
                            });
                        }

                        break;


                    case "ICMED":
                        var icmedApplication = new tbl_ICMED_Application
                        {
                            ApplicationId = request.ApplicationId,
                            Application_Received_date = DateTime.Now,
                            ApplicationName = customerApp.ApplicationId,
                            Orgnization_Name = customerApp.Orgnization_Name,
                            Fk_Certificate = request.Certification_Id,
                            Certification_Name = masterCert.Certificate_Name, // if you have it
                            Scop_of_Certification = "",  // set as per your logic or request
                            Audit_Type = "",             // set as per your logic
                            ActiveReviwer = request.ApplicationId ?? "ReviewerTwo",
                            remark = "",
                            Availbility_of_TechnicalAreas = false,
                            Availbility_of_Auditor = false,
                            Audit_Lang = "",
                            ActiveState = 1,
                            IsInterpreter = false,
                            IsMultisitesampling = false,
                            Total_site = reviewerSites?.Count ?? 0,
                            TargetDate = request.targetDate,
                            Sample_Site = new List<LabelValue>(),
                            Shift_Details = new List<LabelValue>(),

                            ApplicationReviewDate = null,
                            CreatedAt = DateTime.Now,
                            CreatedBy = userId,
                            Status = status.Id,
                            IsDelete = false,
                            IsFinalSubmit = false,
                            Fk_UserId = request.ReviewerId,
                            AssignTo = request.ReviewerId,
                            Technical_Areas = new List<TechnicalAreasList>(),
                            Accreditations = new List<AccreditationsList>(),
                            CustomerSites = reviewerSites,
                            KeyPersonnels = keyPersonnelsList,
                            MandaysLists = reviewerMandays,
                            reviewerKeyPersonnel = reviewerKeyPersonnels,
                            ThreatLists = new List<ThreatList>(),
                            ReviewerThreatList = new List<ReviewerThreatList>(),
                            ReviewerRemarkList = new List<ReviewerRemarkList>()
                        };

                        await _icmed.InsertOneAsync(icmedApplication);

                        // If trainee is also assigned, create one more entry
                        if (!string.IsNullOrWhiteSpace(request.TrineeId))
                        {
                            var traineeIcmdApplication = new tbl_ICMED_Application
                            {
                                ApplicationId = request.ApplicationId,
                                Application_Received_date = DateTime.Now,
                                ApplicationName = customerApp.ApplicationId,
                                Orgnization_Name = customerApp.Orgnization_Name,
                                Fk_Certificate = request.Certification_Id,
                                Certification_Name = masterCert.Certificate_Name,
                                Scop_of_Certification = "",
                                Audit_Type = "",
                                ActiveReviwer = request.ApplicationId ?? "ReviewerOne",
                                remark = "",
                                Availbility_of_TechnicalAreas = false,
                                Availbility_of_Auditor = false,
                                Audit_Lang = "",
                                ActiveState = 1,
                                IsInterpreter = false,
                                IsMultisitesampling = false,
                                Total_site = reviewerSites?.Count ?? 0,
                                TargetDate = request.targetDate,
                                Sample_Site = new List<LabelValue>(),
                                Shift_Details = new List<LabelValue>(),

                                ApplicationReviewDate = null,
                                CreatedAt = DateTime.Now,
                                CreatedBy = userId,
                                Status = status.Id,
                                IsDelete = false,
                                IsFinalSubmit = false,
                                Fk_UserId = request.TrineeId,
                                AssignTo = request.TrineeId,
                                Technical_Areas = new List<TechnicalAreasList>(),
                                Accreditations = new List<AccreditationsList>(),
                                CustomerSites = reviewerSites,
                                KeyPersonnels = keyPersonnelsList,
                                MandaysLists = reviewerMandays,
                                reviewerKeyPersonnel = reviewerKeyPersonnels,
                                ThreatLists = new List<ThreatList>(),
                                ReviewerThreatList = new List<ReviewerThreatList>(),
                                ReviewerRemarkList = new List<ReviewerRemarkList>()
                            };

                            await _icmed.InsertOneAsync(traineeIcmdApplication);
                        }

                        break;


                    case "ICMED_PLUS":

                        var icmedplusApplication = new tbl_ICMED_PLUS_Application
                        {
                            ApplicationId = request.ApplicationId,
                            Application_Received_date = DateTime.Now,
                            ApplicationName = customerApp.ApplicationId,
                            Orgnization_Name = customerApp.Orgnization_Name,
                            Fk_Certificate = request.Certification_Id,
                            Certification_Name = masterCert.Certificate_Name, // if you have it
                            Scop_of_Certification = "",  // set as per your logic or request
                            Audit_Type = "",             // set as per your logic
                            ActiveReviwer = request.ApplicationId ?? "ReviewerTwo",
                            remark = "",
                            Availbility_of_TechnicalAreas = false,
                            Availbility_of_Auditor = false,
                            Audit_Lang = "",
                            ActiveState = 1,
                            IsInterpreter = false,
                            IsMultisitesampling = false,
                            Total_site = reviewerSites?.Count ?? 0,
                            TargetDate = request.targetDate,
                            Sample_Site = new List<LabelValue>(),
                            Shift_Details = new List<LabelValue>(),

                            ApplicationReviewDate = null,
                            CreatedAt = DateTime.Now,
                            CreatedBy = userId,
                            Status = status.Id,
                            IsDelete = false,
                            IsFinalSubmit = false,
                            Fk_UserId = request.ReviewerId,
                            AssignTo = request.ReviewerId,
                            Technical_Areas = new List<TechnicalAreasList>(),
                            Accreditations = new List<AccreditationsList>(),
                            CustomerSites = reviewerSites,
                            KeyPersonnels = keyPersonnelsList,
                            MandaysLists = reviewerMandays,
                            reviewerKeyPersonnel = reviewerKeyPersonnels,
                            ThreatLists = new List<ThreatList>(),
                            ReviewerThreatList = new List<ReviewerThreatList>(),
                            ReviewerRemarkList = new List<ReviewerRemarkList>()
                        };

                        await _icmedplus.InsertOneAsync(icmedplusApplication);

                        // If trainee is also assigned, create one more entry
                        if (!string.IsNullOrWhiteSpace(request.TrineeId))
                        {
                            var traineeIcmdplusApplication = new tbl_ICMED_PLUS_Application
                            {
                                ApplicationId = request.ApplicationId,
                                Application_Received_date = DateTime.Now,
                                ApplicationName = customerApp.ApplicationId,
                                Orgnization_Name = customerApp.Orgnization_Name,
                                Fk_Certificate = request.Certification_Id,
                                Certification_Name = masterCert.Certificate_Name,
                                Scop_of_Certification = "",
                                Audit_Type = "",
                                ActiveReviwer = request.ApplicationId ?? "ReviewerOne",
                                remark = "",
                                Availbility_of_TechnicalAreas = false,
                                Availbility_of_Auditor = false,
                                Audit_Lang = "",
                                ActiveState = 1,
                                IsInterpreter = false,
                                IsMultisitesampling = false,
                                Total_site = reviewerSites?.Count ?? 0,
                                TargetDate = request.targetDate,
                                Sample_Site = new List<LabelValue>(),
                                Shift_Details = new List<LabelValue>(),

                                ApplicationReviewDate = null,
                                CreatedAt = DateTime.Now,
                                CreatedBy = userId,
                                Status = status.Id,
                                IsDelete = false,
                                IsFinalSubmit = false,
                                Fk_UserId = request.TrineeId,
                                AssignTo = request.TrineeId,
                                Technical_Areas = new List<TechnicalAreasList>(),
                                Accreditations = new List<AccreditationsList>(),
                                CustomerSites = reviewerSites,
                                KeyPersonnels = keyPersonnelsList,
                                MandaysLists = reviewerMandays,
                                reviewerKeyPersonnel = reviewerKeyPersonnels,
                                ThreatLists = new List<ThreatList>(),
                                ReviewerThreatList = new List<ReviewerThreatList>(),
                                ReviewerRemarkList = new List<ReviewerRemarkList>()
                            };

                            await _icmedplus.InsertOneAsync(traineeIcmdplusApplication);
                        }

                        break;


                    case "IMDR":

                        // Insert Reviewer (main)
                        await _imdr.InsertOneAsync(new tbl_IMDR_Application
                        {
                            ApplicationId = request.ApplicationId,
                            ApplicationName = customerApp.ApplicationId,
                            Application_Received_date = DateTime.Now,
                            ActiveReviwer = request.ApplicationId ?? "ReviwerTwo",  // Default reviewer role
                            Orgnization_Name = customerApp.Orgnization_Name,
                            Fk_Certificate = request.Certification_Id,
                            AssignTo = request.ReviewerId,
                            TargetDate = request.targetDate,

                            // IMDR-specific fields
                            Scop_of_Certification = "",
                            DeviceMasterfile = "",
                            Technical_Areas = new List<TechnicalAreasList>(),
                            Availbility_of_TechnicalAreas = false,
                            Availbility_of_Auditor = false,
                            Audit_Lang = "",
                            IsInterpreter = false,

                            // Collections
                            CustomerSites = reviewerSites ?? new List<ReviewerSiteDetails>(),
                            //KeyPersonnels = keyPersonnelsList ?? new List<KeyPersonnelList>(),
                            //imdrManDays = imdrMandaysList ?? new List<ImdrManDays>(),
                            //reviewerKeyPersonnel = reviewerkeyPersonnelsList ?? new List<ReviewerKeyPersonnelList>(),
                            //ReviewerThreatList = reviewerThreatLists ?? new List<ReviewerThreatList>(),
                            //ReviewerRemarkList = reviewerRemarkLists ?? new List<ReviewerRemarkList>(),
                            //mdrauditLists = mdrauditLists ?? new List<MDRAuditList>(),

                            // Meta
                            CreatedAt = DateTime.Now,
                            CreatedBy = userId,
                            Status = status.Id,
                            IsDelete = false,
                            IsFinalSubmit = false,
                            Fk_UserId = request.ReviewerId
                        });

                        // Insert Trainee (if present)
                        if (!string.IsNullOrWhiteSpace(request.TrineeId))
                        {
                            await _imdr.InsertOneAsync(new tbl_IMDR_Application
                            {
                                ApplicationId = request.ApplicationId,
                                ApplicationName = customerApp.ApplicationId,
                                Application_Received_date = DateTime.Now,
                                ActiveReviwer = request.ApplicationId ?? "ReviwerOne",  // Default trainee role
                                Orgnization_Name = customerApp.Orgnization_Name,
                                Fk_Certificate = request.Certification_Id,
                                AssignTo = request.TrineeId,
                                TargetDate = request.targetDate,

                                // IMDR-specific fields
                                Scop_of_Certification = "",
                                DeviceMasterfile = "",
                                Technical_Areas = new List<TechnicalAreasList>(),
                                Availbility_of_TechnicalAreas = false,
                                Availbility_of_Auditor = false,
                                Audit_Lang = "",
                                IsInterpreter = false,

                                // Collections
                                CustomerSites = reviewerSites ?? new List<ReviewerSiteDetails>(),
                                KeyPersonnels = keyPersonnelsList ?? new List<KeyPersonnelList>(),
                                //imdrManDays = imdrMandaysList ?? new List<ImdrManDays>(),
                                //reviewerKeyPersonnel = reviewerkeyPersonnelsList ?? new List<ReviewerKeyPersonnelList>(),
                                //ReviewerThreatList = reviewerThreatLists ?? new List<ReviewerThreatList>(),
                                //ReviewerRemarkList = reviewerRemarkLists ?? new List<ReviewerRemarkList>(),
                                //mdrauditLists = mdrauditLists ?? new List<MDRAuditList>(),

                                // Meta
                                CreatedAt = DateTime.Now,
                                CreatedBy = userId,
                                Status = status.Id,
                                IsDelete = false,
                                IsFinalSubmit = false,
                                Fk_UserId = request.TrineeId
                            });
                        }

                        break;


                    default:
                        // Handle unknown certification if needed
                        break;

                }

                // ✅ Final Success
                response.Message = "Application assigned successfully.";
                response.Success = true;
                response.HttpStatusCode = System.Net.HttpStatusCode.OK;
                response.ResponseCode = 0;
            }
            catch (Exception ex)
            {
                response.Message = "AssignApplication Exception: " + ex.Message;
                response.Success = false;
                response.HttpStatusCode = System.Net.HttpStatusCode.InternalServerError;
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
                        
                        
                        // With the following initialization:
                        tbl_ISO_Application? adminData = null;
                        tbl_ISO_Application? reviwerData = null;
                        tbl_ISO_Application? traineData = null;
                        

                        var filter = Builders<tbl_ISO_Application>.Filter.Eq(x => x.ApplicationId, request.applicationId);
                        var data = await _iso.Find(filter).FirstOrDefaultAsync();

                        var filterwithAdmin = Builders<tbl_ISO_Application>.Filter.Eq(x => x.ApplicationId, request.applicationId) &
                                              Builders<tbl_ISO_Application>.Filter.Eq(x => x.AssignTo, UserId);
                        if (filterwithAdmin != null)
                        {
                            adminData = await _iso.Find(filterwithAdmin).FirstOrDefaultAsync();
                        }
                        else
                        {
                            var findreviewer = Builders<tbl_ISO_Application>.Filter.And(
                               Builders<tbl_ISO_Application>.Filter.Eq(x => x.ApplicationId, data.ApplicationId),
                               Builders<tbl_ISO_Application>.Filter.Eq(x => x.Fk_Certificate, data.Fk_Certificate),
                               Builders<tbl_ISO_Application>.Filter.Ne(x => x.AssignTo, "686fc25880b29ec6e7867768")
                            );
                            var reviewerAndTraineeList = await _iso.Find(findreviewer).ToListAsync();
                            foreach (var item in reviewerAndTraineeList)
                            {
                                var type = _user.Find(x => x.Id == item.AssignTo).FirstOrDefault()?.Type;
                                if (type == "Reviewer")
                                {
                                    var reviwerDatas = Builders<tbl_ISO_Application>.Filter.And(
                                       Builders<tbl_ISO_Application>.Filter.Eq(x => x.ApplicationId, data.ApplicationId),
                                       Builders<tbl_ISO_Application>.Filter.Eq(x => x.Fk_Certificate, data.Fk_Certificate),
                                       Builders<tbl_ISO_Application>.Filter.Eq(x => x.AssignTo, item.AssignTo)
                                    );
                                    reviwerData = await _iso.Find(reviwerDatas).FirstOrDefaultAsync();
                                }
                                else if (type == "Trainee")
                                {
                                    var traineDatas = Builders<tbl_ISO_Application>.Filter.And(
                                        Builders<tbl_ISO_Application>.Filter.Eq(x => x.ApplicationId, data.ApplicationId),
                                        Builders<tbl_ISO_Application>.Filter.Eq(x => x.Fk_Certificate, data.Fk_Certificate),
                                        Builders<tbl_ISO_Application>.Filter.Eq(x => x.AssignTo, item.AssignTo)
                                     );
                                    traineData = await _iso.Find(traineDatas).FirstOrDefaultAsync();
                                }
                            }

                        }
                        MergeDataByLatest(adminData, reviwerData, traineData); // merges directly into data
                       


                        //var cerificateName = _mongoDbService.Getcertificatename(data.Fk_Certificate);
                        var assignmane = _mongoDbService.ReviewerName(data.AssignTo);
                        var status = _mongoDbService.StatusName(data.Status);
                        var comments = await GetFieldCommentsAsync(data.ApplicationId, data.Fk_Certificate, UserId, _comments);

                        response.Data = data;
                        response.Comments = comments;
                        //response.CertificateName = cerificateName;
                        response.statusName = status;
                    }
                    else if (request.CertificationName == "FSSC")
                    {
                        // Initialize holders
                        tbl_FSSC_Application? adminData = null;
                        tbl_FSSC_Application? reviewerData = null;
                        tbl_FSSC_Application? traineeData = null;

                        // Fetch base application
                        var filter = Builders<tbl_FSSC_Application>.Filter.Eq(x => x.ApplicationId, request.applicationId);
                        var data = await _fssc.Find(filter).FirstOrDefaultAsync();

                        // Check if current user is Admin on this application
                        var filterWithAdmin = Builders<tbl_FSSC_Application>.Filter.Eq(x => x.ApplicationId, request.applicationId) &
                                              Builders<tbl_FSSC_Application>.Filter.Eq(x => x.AssignTo, UserId);

                        if (filterWithAdmin != null)
                        {
                            adminData = await _fssc.Find(filterWithAdmin).FirstOrDefaultAsync();
                        }
                        else
                        {
                            // If not admin, check Reviewer/Trainee assignments
                            var findReviewer = Builders<tbl_FSSC_Application>.Filter.And(
                                Builders<tbl_FSSC_Application>.Filter.Eq(x => x.ApplicationId, data.ApplicationId),
                                Builders<tbl_FSSC_Application>.Filter.Eq(x => x.Fk_Certificate, data.Fk_Certificate),
                                Builders<tbl_FSSC_Application>.Filter.Ne(x => x.AssignTo, "686fc25880b29ec6e7867768") // exclude trainee constant
                            );

                            var reviewerAndTraineeList = await _fssc.Find(findReviewer).ToListAsync();
                            foreach (var item in reviewerAndTraineeList)
                            {
                                var type = _user.Find(x => x.Id == item.AssignTo).FirstOrDefault()?.Type;
                                if (type == "Reviewer")
                                {
                                    var reviewerFilter = Builders<tbl_FSSC_Application>.Filter.And(
                                        Builders<tbl_FSSC_Application>.Filter.Eq(x => x.ApplicationId, data.ApplicationId),
                                        Builders<tbl_FSSC_Application>.Filter.Eq(x => x.Fk_Certificate, data.Fk_Certificate),
                                        Builders<tbl_FSSC_Application>.Filter.Eq(x => x.AssignTo, item.AssignTo)
                                    );
                                    reviewerData = await _fssc.Find(reviewerFilter).FirstOrDefaultAsync();
                                }
                                else if (type == "Trainee")
                                {
                                    var traineeFilter = Builders<tbl_FSSC_Application>.Filter.And(
                                        Builders<tbl_FSSC_Application>.Filter.Eq(x => x.ApplicationId, data.ApplicationId),
                                        Builders<tbl_FSSC_Application>.Filter.Eq(x => x.Fk_Certificate, data.Fk_Certificate),
                                        Builders<tbl_FSSC_Application>.Filter.Eq(x => x.AssignTo, item.AssignTo)
                                    );
                                    traineeData = await _fssc.Find(traineeFilter).FirstOrDefaultAsync();
                                }
                            }
                        }

                        // Merge Admin > Reviewer > Trainee
                        MergeDataByLatest(adminData, reviewerData, traineeData);

                        // Enrich response
                        var assignName = _mongoDbService.ReviewerName(data.AssignTo);
                        var statusName = _mongoDbService.StatusName(data.Status);
                        var comments = await GetFieldCommentsAsync(data.ApplicationId, data.Fk_Certificate, UserId, _comments);

                        response.Data = data;
                        response.Comments = comments;
                        response.statusName = statusName;
                    }
                    else if (request.CertificationName == "ICMED")
                    {
                        // With the following initialization:
                        tbl_ICMED_Application? adminData = null;
                        tbl_ICMED_Application? reviwerData = null;
                        tbl_ICMED_Application? traineData = null;

                        var filter = Builders<tbl_ICMED_Application>.Filter
                                                                    .Eq(x => x.ApplicationId, request.applicationId);

                        var data = await _icmed.Find(filter).FirstOrDefaultAsync();

                        var filterwithAdmin = Builders<tbl_ICMED_Application>.Filter.Eq(x => x.ApplicationId, request.applicationId) &
                                              Builders<tbl_ICMED_Application>.Filter.Eq(x => x.AssignTo, UserId);

                        if (filterwithAdmin != null)
                        {
                            adminData = await _icmed.Find(filterwithAdmin).FirstOrDefaultAsync();
                        }
                        else
                        {
                            var findreviewer = Builders<tbl_ICMED_Application>.Filter.And(
                                Builders<tbl_ICMED_Application>.Filter.Eq(x => x.ApplicationId, data.ApplicationId),
                                Builders<tbl_ICMED_Application>.Filter.Eq(x => x.Fk_Certificate, data.Fk_Certificate),
                                Builders<tbl_ICMED_Application>.Filter.Ne(x => x.AssignTo, "686fc25880b29ec6e7867768") // default/system user
                            );

                            var reviewerAndTraineeList = await _icmed.Find(findreviewer).ToListAsync();
                            foreach (var item in reviewerAndTraineeList)
                            {
                                var type = _user.Find(x => x.Id == item.AssignTo).FirstOrDefault()?.Type;
                                if (type == "Reviewer")
                                {
                                    var reviwerDatas = Builders<tbl_ICMED_Application>.Filter.And(
                                        Builders<tbl_ICMED_Application>.Filter.Eq(x => x.ApplicationId, data.ApplicationId),
                                        Builders<tbl_ICMED_Application>.Filter.Eq(x => x.Fk_Certificate, data.Fk_Certificate),
                                        Builders<tbl_ICMED_Application>.Filter.Eq(x => x.AssignTo, item.AssignTo)
                                    );
                                    reviwerData = await _icmed.Find(reviwerDatas).FirstOrDefaultAsync();
                                }
                                else if (type == "Trainee")
                                {
                                    var traineDatas = Builders<tbl_ICMED_Application>.Filter.And(
                                        Builders<tbl_ICMED_Application>.Filter.Eq(x => x.ApplicationId, data.ApplicationId),
                                        Builders<tbl_ICMED_Application>.Filter.Eq(x => x.Fk_Certificate, data.Fk_Certificate),
                                        Builders<tbl_ICMED_Application>.Filter.Eq(x => x.AssignTo, item.AssignTo)
                                    );
                                    traineData = await _icmed.Find(traineDatas).FirstOrDefaultAsync();
                                }
                            }
                        }

                        // Merge all into the latest data
                        MergeDataByLatest(adminData, reviwerData, traineData);

                        var assignmane = _mongoDbService.ReviewerName(data.AssignTo);
                        var status = _mongoDbService.StatusName(data.Status);
                        var comments = await GetFieldCommentsAsync(data.ApplicationId, data.Fk_Certificate, UserId, _comments);

                        response.Data = data;
                        response.Comments = comments;
                        //response.CertificateName = _mongoDbService.Getcertificatename(data.Fk_Certificate);
                        response.statusName = status;
                    }
                    else if (request.CertificationName == "ICMED_PLUS")
                    {
                        // With the following initialization:
                        tbl_ICMED_PLUS_Application? adminData = null;
                        tbl_ICMED_PLUS_Application? reviwerData = null;
                        tbl_ICMED_PLUS_Application? traineData = null;

                        var filter = Builders<tbl_ICMED_PLUS_Application>.Filter
                                                                         .Eq(x => x.ApplicationId, request.applicationId);

                        var data = await _icmedplus.Find(filter).FirstOrDefaultAsync();

                        var filterwithAdmin = Builders<tbl_ICMED_PLUS_Application>.Filter.Eq(x => x.ApplicationId, request.applicationId) &
                                              Builders<tbl_ICMED_PLUS_Application>.Filter.Eq(x => x.AssignTo, UserId);

                        if (filterwithAdmin != null)
                        {
                            adminData = await _icmedplus.Find(filterwithAdmin).FirstOrDefaultAsync();
                        }
                        else
                        {
                            var findreviewer = Builders<tbl_ICMED_PLUS_Application>.Filter.And(
                                Builders<tbl_ICMED_PLUS_Application>.Filter.Eq(x => x.ApplicationId, data.ApplicationId),
                                Builders<tbl_ICMED_PLUS_Application>.Filter.Eq(x => x.Fk_Certificate, data.Fk_Certificate),
                                Builders<tbl_ICMED_PLUS_Application>.Filter.Ne(x => x.AssignTo, "686fc25880b29ec6e7867768") // system/default user
                            );

                            var reviewerAndTraineeList = await _icmedplus.Find(findreviewer).ToListAsync();
                            foreach (var item in reviewerAndTraineeList)
                            {
                                var type = _user.Find(x => x.Id == item.AssignTo).FirstOrDefault()?.Type;
                                if (type == "Reviewer")
                                {
                                    var reviwerDatas = Builders<tbl_ICMED_PLUS_Application>.Filter.And(
                                        Builders<tbl_ICMED_PLUS_Application>.Filter.Eq(x => x.ApplicationId, data.ApplicationId),
                                        Builders<tbl_ICMED_PLUS_Application>.Filter.Eq(x => x.Fk_Certificate, data.Fk_Certificate),
                                        Builders<tbl_ICMED_PLUS_Application>.Filter.Eq(x => x.AssignTo, item.AssignTo)
                                    );
                                    reviwerData = await _icmedplus.Find(reviwerDatas).FirstOrDefaultAsync();
                                }
                                else if (type == "Trainee")
                                {
                                    var traineDatas = Builders<tbl_ICMED_PLUS_Application>.Filter.And(
                                        Builders<tbl_ICMED_PLUS_Application>.Filter.Eq(x => x.ApplicationId, data.ApplicationId),
                                        Builders<tbl_ICMED_PLUS_Application>.Filter.Eq(x => x.Fk_Certificate, data.Fk_Certificate),
                                        Builders<tbl_ICMED_PLUS_Application>.Filter.Eq(x => x.AssignTo, item.AssignTo)
                                    );
                                    traineData = await _icmedplus.Find(traineDatas).FirstOrDefaultAsync();
                                }
                            }
                        }

                        // Merge all into the latest data
                        MergeDataByLatest(adminData, reviwerData, traineData);

                        var assignmane = _mongoDbService.ReviewerName(data.AssignTo);
                        var status = _mongoDbService.StatusName(data.Status);
                        var comments = await GetFieldCommentsAsync(data.ApplicationId, data.Fk_Certificate, UserId, _comments);

                        response.Data = data;
                        response.Comments = comments;
                        //response.CertificateName = _mongoDbService.Getcertificatename(data.Fk_Certificate);
                        response.statusName = status;
                    }
                    else if (request.CertificationName == "IMDR")
                    {
                        // With the following initialization:
                        tbl_IMDR_Application? adminData = null;
                        tbl_IMDR_Application? reviwerData = null;
                        tbl_IMDR_Application? traineData = null;

                        var filter = Builders<tbl_IMDR_Application>.Filter
                                                                   .Eq(x => x.ApplicationId, request.applicationId);

                        var data = await _imdr.Find(filter).FirstOrDefaultAsync();

                        var filterwithAdmin = Builders<tbl_IMDR_Application>.Filter.Eq(x => x.ApplicationId, request.applicationId) &
                                              Builders<tbl_IMDR_Application>.Filter.Eq(x => x.AssignTo, UserId);

                        if (filterwithAdmin != null)
                        {
                            adminData = await _imdr.Find(filterwithAdmin).FirstOrDefaultAsync();
                        }
                        else
                        {
                            var findreviewer = Builders<tbl_IMDR_Application>.Filter.And(
                                Builders<tbl_IMDR_Application>.Filter.Eq(x => x.ApplicationId, data.ApplicationId),
                                Builders<tbl_IMDR_Application>.Filter.Eq(x => x.Fk_Certificate, data.Fk_Certificate),
                                Builders<tbl_IMDR_Application>.Filter.Ne(x => x.AssignTo, "686fc25880b29ec6e7867768") // system/default user
                            );

                            var reviewerAndTraineeList = await _imdr.Find(findreviewer).ToListAsync();
                            foreach (var item in reviewerAndTraineeList)
                            {
                                var type = _user.Find(x => x.Id == item.AssignTo).FirstOrDefault()?.Type;
                                if (type == "Reviewer")
                                {
                                    var reviwerDatas = Builders<tbl_IMDR_Application>.Filter.And(
                                        Builders<tbl_IMDR_Application>.Filter.Eq(x => x.ApplicationId, data.ApplicationId),
                                        Builders<tbl_IMDR_Application>.Filter.Eq(x => x.Fk_Certificate, data.Fk_Certificate),
                                        Builders<tbl_IMDR_Application>.Filter.Eq(x => x.AssignTo, item.AssignTo)
                                    );
                                    reviwerData = await _imdr.Find(reviwerDatas).FirstOrDefaultAsync();
                                }
                                else if (type == "Trainee")
                                {
                                    var traineDatas = Builders<tbl_IMDR_Application>.Filter.And(
                                        Builders<tbl_IMDR_Application>.Filter.Eq(x => x.ApplicationId, data.ApplicationId),
                                        Builders<tbl_IMDR_Application>.Filter.Eq(x => x.Fk_Certificate, data.Fk_Certificate),
                                        Builders<tbl_IMDR_Application>.Filter.Eq(x => x.AssignTo, item.AssignTo)
                                    );
                                    traineData = await _imdr.Find(traineDatas).FirstOrDefaultAsync();
                                }
                            }
                        }

                        // Merge all into the latest data
                        MergeDataByLatest(adminData, reviwerData, traineData);

                        var assignmane = _mongoDbService.ReviewerName(data.AssignTo);
                        var status = _mongoDbService.StatusName(data.Status);
                        var comments = await GetFieldCommentsAsync(data.ApplicationId, data.Fk_Certificate, UserId, _comments);

                        response.Data = data;
                        response.Comments = comments;
                        //response.CertificateName = _mongoDbService.Getcertificatename(data.Fk_Certificate);
                        response.statusName = status;
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
                            Builders<tbl_ISO_Application>.Filter.Eq(x => x.ApplicationId, request.ApplicationId)
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
                                    AssignPersonId=g.Key,
                                    AssignPersonName = _mongoDbService.ReviewerName(g.Key),
                                    AssignPersonRole = _mongoDbService.UserRoleType(g.Key),
                                    LatestUpdatedDate = latestApp.UpdatedAt ?? latestApp.CreatedAt
                                };
                            })
                            .ToList();

                        // ✅ Add reviewer history array to response
                        response.ReviewerHistory = result;
                        response.HttpStatusCode = System.Net.HttpStatusCode.OK;
                        response.Success = true;
                        response.ResponseCode = 200;
                    }

                    else if (request.CertificationName == "FSSC")
                    {
                        var filter = Builders<tbl_FSSC_Application>.Filter.And(
                            Builders<tbl_FSSC_Application>.Filter.Eq(x => x.ApplicationId, request.ApplicationId)
                        );

                        var applications = await _fssc.Find(filter).ToListAsync();

                        var result = applications
                            .GroupBy(a => a.AssignTo)
                            .Select(g =>
                            {
                                var latestApp = g.OrderByDescending(a => a.UpdatedAt ?? a.CreatedAt).FirstOrDefault();
                                return new ReviewerHistoryDto
                                {
                                    ApplicationId = latestApp.ApplicationId,
                                    AssignPersonId = g.Key,
                                    AssignPersonName = _mongoDbService.ReviewerName(g.Key),
                                    AssignPersonRole = _mongoDbService.UserRoleType(g.Key),
                                    LatestUpdatedDate = latestApp.UpdatedAt ?? latestApp.CreatedAt
                                };
                            })
                            .ToList();

                        // ✅ Add reviewer history array to response
                        response.ReviewerHistory = result;
                        response.HttpStatusCode = System.Net.HttpStatusCode.OK;
                        response.Success = true;
                        response.ResponseCode = 200;
                    }

                    else if (request.CertificationName == "ICMED")
                    {
                        var filter = Builders<tbl_ICMED_Application>.Filter.And(
                            Builders<tbl_ICMED_Application>.Filter.Eq(x => x.ApplicationId, request.ApplicationId)
                        );

                        var applications = await _icmed.Find(filter).ToListAsync();

                        var result = applications
                            .GroupBy(a => a.AssignTo)
                            .Select(g =>
                            {
                                var latestApp = g.OrderByDescending(a => a.UpdatedAt ?? a.CreatedAt).FirstOrDefault();
                                return new ReviewerHistoryDto
                                {
                                    ApplicationId = latestApp.ApplicationId,
                                    AssignPersonId = g.Key,
                                    AssignPersonName = _mongoDbService.ReviewerName(g.Key),
                                    AssignPersonRole = _mongoDbService.UserRoleType(g.Key),
                                    LatestUpdatedDate = latestApp.UpdatedAt ?? latestApp.CreatedAt
                                };
                            })
                            .ToList();

                        // ✅ Add reviewer history array to response
                        response.ReviewerHistory = result;
                        response.HttpStatusCode = System.Net.HttpStatusCode.OK;
                        response.Success = true;
                        response.ResponseCode = 200;
                    }
                    else if (request.CertificationName == "IMDR")
                    {
                        var filter = Builders<tbl_IMDR_Application>.Filter.And(
                            Builders<tbl_IMDR_Application>.Filter.Eq(x => x.ApplicationId, request.ApplicationId)
                        );

                        var applications = await _imdr.Find(filter).ToListAsync();

                        var result = applications
                            .GroupBy(a => a.AssignTo)
                            .Select(g =>
                            {
                                var latestApp = g.OrderByDescending(a => a.UpdatedAt ?? a.CreatedAt).FirstOrDefault();
                                return new ReviewerHistoryDto
                                {
                                    ApplicationId = latestApp.ApplicationId,
                                    AssignPersonId = g.Key,
                                    AssignPersonName = _mongoDbService.ReviewerName(g.Key),
                                    AssignPersonRole = _mongoDbService.UserRoleType(g.Key),
                                    LatestUpdatedDate = latestApp.UpdatedAt ?? latestApp.CreatedAt
                                };
                            })
                            .ToList();

                        // ✅ Add reviewer history array to response
                        response.ReviewerHistory = result;
                        response.HttpStatusCode = System.Net.HttpStatusCode.OK;
                        response.Success = true;
                        response.ResponseCode = 200;
                    }
                    else if (request.CertificationName == "ICMED_PLUS")
                    {
                        var filter = Builders<tbl_ICMED_PLUS_Application>.Filter.And(
                            Builders<tbl_ICMED_PLUS_Application>.Filter.Eq(x => x.ApplicationId, request.ApplicationId)
                        );

                        var applications = await _icmedplus.Find(filter).ToListAsync();

                        var result = applications
                            .GroupBy(a => a.AssignTo)
                            .Select(g =>
                            {
                                var latestApp = g.OrderByDescending(a => a.UpdatedAt ?? a.CreatedAt).FirstOrDefault();
                                return new ReviewerHistoryDto
                                {
                                    ApplicationId = latestApp.ApplicationId,
                                    AssignPersonId = g.Key,
                                    AssignPersonName = _mongoDbService.ReviewerName(g.Key),
                                    AssignPersonRole = _mongoDbService.UserRoleType(g.Key),
                                    LatestUpdatedDate = latestApp.UpdatedAt ?? latestApp.CreatedAt
                                };
                            })
                            .ToList();

                        // ✅ Add reviewer history array to response
                        response.ReviewerHistory = result;
                        response.HttpStatusCode = System.Net.HttpStatusCode.OK;
                        response.Success = true;
                        response.ResponseCode = 200;
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

        private async Task<List<tbl_ApplicationFieldComment>> GetFieldCommentsAsync(string applicationId, string fkCertificate, string UserId, IMongoCollection<tbl_ApplicationFieldComment> commentCollection)
        {
            var filter = Builders<tbl_ApplicationFieldComment>.Filter.And(
                Builders<tbl_ApplicationFieldComment>.Filter.Eq(c => c.ApplicationId, applicationId),
                Builders<tbl_ApplicationFieldComment>.Filter.Eq(c => c.CertificationName, fkCertificate),
                Builders<tbl_ApplicationFieldComment>.Filter.Eq(c => c.Fk_User, UserId)
            );

            //return await commentCollection
            //    .Find(filter)
            //    .SortByDescending(c => c.CreatedOn)
            //    .ToListAsync();

            var pipeline = commentCollection.Aggregate()
            .Match(filter)
            .SortByDescending(c => c.CreatedOn) // or use c.Id
            .Group(
                key => key.FieldName,
                g => g.First()
            );

                return await pipeline.ToListAsync();
            }

        
        private T MergeDataByLatest<T>(T admin, T reviewer, T trainee) where T : class, new()
        {
            var target = new T();
            var excludedFields = new[] { "AssignTo", "Id", "UserFk", "ActiveState", "ActiveReviwer" };

            // Collect available candidates
            var candidates = new List<T> { admin, reviewer, trainee }
                             .Where(x => x != null)
                             .ToList();

            if (!candidates.Any())
                return target;

            // Sort candidates by UpdatedAt (or CreatedAt fallback) - latest first
            var orderedCandidates = candidates
                .Select(c => new
                {
                    Obj = c,
                    Updated = GetUpdatedDate(c)
                })
                .OrderByDescending(x => x.Updated)
                .ToList();

            foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (excludedFields.Contains(prop.Name))
                    continue;

                object finalValue = null;

                // Loop through candidates by latest updated
                foreach (var candidate in orderedCandidates)
                {
                    var value = prop.GetValue(candidate.Obj);
                    if (HasValue(value))
                    {
                        finalValue = value;
                        break; // take the first non-null, non-empty from latest record
                    }
                }

                if (finalValue != null)
                    prop.SetValue(target, finalValue);
            }

            return target;
        }

        
        private DateTime GetUpdatedDate<T>(T obj)
        {
            if (obj == null) return DateTime.MinValue;

            var type = typeof(T);

            var updatedAt = type.GetProperty("UpdatedAt")?.GetValue(obj) as DateTime?;
            if (updatedAt.HasValue && updatedAt.Value != default)
                return updatedAt.Value;

            var createdAt = type.GetProperty("CreatedAt")?.GetValue(obj) as DateTime?;
            if (createdAt.HasValue && createdAt.Value != default)
                return createdAt.Value;

            return DateTime.MinValue;
        }

        private bool HasValue(object value)
        {
            if (value == null) return false;
            if (value is string str) return !string.IsNullOrWhiteSpace(str);

            var type = value.GetType();
            if (type.IsValueType)
                return !value.Equals(Activator.CreateInstance(type));

            if (value is System.Collections.IEnumerable enumerable && !(value is string))
                return enumerable.GetEnumerator().MoveNext();

            return true;
        }


        public async Task<BaseResponse> SaveApplicationStatus(statusRequest request)
        {
            var response = new addReviewerApplicationResponse();

            try
            {
                if (string.IsNullOrEmpty(request.applicationId) || string.IsNullOrEmpty(request.type))
                {
                    response.Message = "ApplicationId or Type is missing.";
                    response.HttpStatusCode = HttpStatusCode.BadRequest;
                    response.Success = false;
                    return response;
                }

                // map status to ObjectId string
                string statusId = request.status switch
                {
                    "Approved" => "68a80adcf43ed36702310521",
                    "Rejected" => "68ac658b45a82f9f829724db",
                    _ => null
                };

                if (statusId == null)
                {
                    response.Message = $"Invalid status: {request.status}";
                    response.HttpStatusCode = HttpStatusCode.BadRequest;
                    response.Success = false;
                    return response;
                }

                // update depending on type
                switch (request.type.ToLower())
                {
                    case "iso":
                        await UpdateStatusAsync(_iso, request.applicationId, statusId);
                        break;

                    case "fssc":
                        await UpdateStatusAsync(_fssc, request.applicationId, statusId);
                        break;

                    case "icmed":
                        await UpdateStatusAsync(_icmed, request.applicationId, statusId);
                        break;

                    case "imdr":
                        await UpdateStatusAsync(_imdr, request.applicationId, statusId);
                        break;

                    case "icmdplus":
                        await UpdateStatusAsync(_icmedplus, request.applicationId, statusId);
                        break;

                    default:
                        response.Message = $"Invalid application type: {request.type}";
                        response.HttpStatusCode = HttpStatusCode.BadRequest;
                        response.Success = false;
                        return response;
                }

                // also update customer certificates
                var certUpdate = Builders<tbl_customer_certificates>.Update
                    .Set(x => x.status, statusId)
                    .Set(x => x.UpdatedAt, DateTime.UtcNow);

                await _customercertificates.UpdateOneAsync(
                    x => x.Id == request.applicationId,
                    certUpdate
                );

                response.Message = $"{request.type} status updated successfully.";
                response.HttpStatusCode = HttpStatusCode.OK;
                response.Success = true;
                response.ResponseCode = 0;
            }
            catch (Exception ex)
            {
                response.Message = "SaveApplicationStatus Exception: " + ex.Message;
                response.HttpStatusCode = HttpStatusCode.InternalServerError;
                response.Success = false;
            }

            return response;
        }

        private async Task UpdateStatusAsync<T>(IMongoCollection<T> collection, string applicationId, string statusId)
        {
            var filter = Builders<T>.Filter.Eq("ApplicationId", applicationId);
            var update = Builders<T>.Update
                .Set("Status", statusId)
                .Set("UpdatedAt", DateTime.UtcNow);

            await collection.UpdateOneAsync(filter, update);
        }

        [HttpPost("AddOrUpdateMasterUser")]
        public async Task<BaseResponse> AddOrUpdateMasterUser([FromBody] AddMasterUsersRequest request)
        {
            var response = new BaseResponse();
            var sessionUserId = _acc.HttpContext?.Session.GetString("UserId");
            var UserId = _acc.HttpContext?.Session.GetString("UserId");
            try
            {
                if (request == null)
                {
                    return new BaseResponse
                    {
                        Message = "Data is required.",
                        HttpStatusCode = HttpStatusCode.BadRequest,
                        Success = false
                    };
                }

                // Get current user's name
                var currentUser = _user.Find(x => x.Id == sessionUserId).FirstOrDefault();
                var createdByName = currentUser?.FullName ?? "System";

                // Check if user already exists (by Id or Email)
                tbl_user existingUser = null;
                if (!string.IsNullOrEmpty(request.UserId))
                {
                    existingUser = _user.Find(x => x.Id == request.UserId).FirstOrDefault();
                }
                else
                {
                    // Optional: Check if same email already exists
                    existingUser = _user.Find(x => x.EmailId == request.Email).FirstOrDefault();
                }

                if (existingUser != null)
                {
                    // --- UPDATE FLOW ---
                    existingUser.FullName = request.FullName;
                    existingUser.EmailId = request.Email;
                    existingUser.ContactNo = request.Mobile;
                    existingUser.Password = request.Password;
                    existingUser.UpdatedAt = DateTime.UtcNow;
                    existingUser.UpdatedBy = sessionUserId;
                    existingUser.FkCertificate = request.CertificateId;
                    existingUser.CertificateName = _masterCertificate.Find(x => x.Id == request.CertificateId).FirstOrDefault().Certificate_Name;

                    await _user.ReplaceOneAsync(x => x.Id == existingUser.Id, existingUser);

                    response.Message = "User updated successfully.";
                }
                else
                {
                    // --- ADD NEW FLOW ---
                    var newUser = new tbl_user
                    {
                        FullName = request.FullName,
                        EmailId = request.Email,
                        ContactNo = request.Mobile,
                        Password = request.Password,
                        IsDelete = 0,
                        CreatedBy = createdByName,
                        UpdatedBy = sessionUserId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CertificateName = _masterCertificate.Find(x=>x.Id==request.CertificateId).FirstOrDefault().Certificate_Name,
                        Type = "MasterAdmin",
                        Fk_RoleID = "686fc53af41f7edee9b89cd5",
                        UserName = request.Email,
                        FkCertificate = request.CertificateId
                    };

                    await _user.InsertOneAsync(newUser);
                    response.Message = "User added successfully.";
                }

                response.Success = true;
                response.HttpStatusCode = HttpStatusCode.OK;
                response.ResponseCode = 0;
            }
            catch (Exception ex)
            {
                response.Message = $"Error: {ex.Message}";
                response.HttpStatusCode = HttpStatusCode.InternalServerError;
                response.Success = false;
            }

            return response;
        }
        [HttpGet("GetMasterUsers")]
        public async Task<GetMasterUsersResponse> GetMasterUsers(GetMasterUsersRequest request)
        {
            var response = new GetMasterUsersResponse();

            try
            {
                if (!string.IsNullOrEmpty(request.UserId))
                {
                    // Get single user
                    var user = await _user.Find(x => x.Id == request.UserId && x.IsDelete == 0 && x.Type == "MasterAdmin").FirstOrDefaultAsync();
                    response.Data = user != null ? new List<tbl_user> { user } : new List<tbl_user>();
                }
                else
                {
                    // Get all active users
                    response.Data = await _user.Find(x => x.IsDelete == 0 && x.Type == "MasterAdmin").ToListAsync();
                }

                response.Success = true;
                response.HttpStatusCode = HttpStatusCode.OK;
                response.Message = "Users fetched successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Error: " + ex.Message;
                response.HttpStatusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }
        //new code by swapnil 
        


        //new code by swapnil 



        protected override void Disposing()
        { 
            //throw new NotImplementedException();
        }
    }
}
