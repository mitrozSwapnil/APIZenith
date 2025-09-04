using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Net;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Xml.Linq;
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
        private readonly IMongoCollection<tbl_ICMED_Application> _icmed;
        private readonly IMongoCollection<tbl_ICMED_PLUS_Application> _icmedplus;
        private readonly IMongoCollection<tbl_IMDR_Application> _imdr;
        private readonly IMongoCollection<tbl_Application_Remark> _remark;
        private readonly IMongoCollection<tbl_Application_Threat> _threat;
        private readonly IMongoCollection<tbl_Master_Threat> _masterthreat;
        private readonly IMongoCollection<tbl_Master_Remark> _masterremark;
        private readonly IMongoCollection<tbl_FSSC_Application> _fssc;
        private readonly MongoDbService _mongoDbService;
        private readonly IMongoCollection<tbl_ApplicationReview> _reviews;
        private readonly IMongoCollection<tbl_ApplicationFieldHistory> _history;
        private readonly IMongoCollection<tbl_ApplicationFieldComment> _comments;

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
            _icmed = database.GetCollection<tbl_ICMED_Application>("tbl_ICMED_Application");
            _icmedplus = database.GetCollection<tbl_ICMED_PLUS_Application>("tbl_ICMED_PLUS_Application");
            _imdr = database.GetCollection<tbl_IMDR_Application>("tbl_IMDR_Application");
            _remark = database.GetCollection<tbl_Application_Remark>("tbl_Application_Remark");
            _threat = database.GetCollection<tbl_Application_Threat>("tbl_Application_Threat");
            _masterthreat = database.GetCollection<tbl_Master_Threat>("tbl_Master_Threat");
            _fssc = database.GetCollection<tbl_FSSC_Application>("tbl_FSSC_Application");
            _mongoDbService = mongoDbService;
            _acc = acc;




            _reviews = database.GetCollection<tbl_ApplicationReview>("tbl_ApplicationReview");
            _history = database.GetCollection<tbl_ApplicationFieldHistory>("tbl_ApplicationFieldHistory");
            _comments = database.GetCollection<tbl_ApplicationFieldComment>("tbl_ApplicationFieldComment");





            _reviews.Indexes.CreateOne(
                new CreateIndexModel<tbl_ApplicationReview>(
                    Builders<tbl_ApplicationReview>.IndexKeys
                        .Ascending(x => x.CertificationName)
                        .Ascending(x => x.Fk_ApplicationId)
                        .Ascending(x => x.ReviewerId),
                    new CreateIndexOptions { Unique = true }
                )
            );

            _history.Indexes.CreateOne(
                new CreateIndexModel<tbl_ApplicationFieldHistory>(
                    Builders<tbl_ApplicationFieldHistory>.IndexKeys
                        .Ascending(x => x.CertificationName)
                        .Ascending(x => x.Fk_ApplicationId)
                        .Ascending(x => x.FieldName)
                        .Ascending(x => x.ChangedOn)
                )
            );

            _comments.Indexes.CreateOne(
                new CreateIndexModel<tbl_ApplicationFieldComment>(
                    Builders<tbl_ApplicationFieldComment>.IndexKeys
                        .Ascending(x => x.CertificationName)
                        .Ascending(x => x.ApplicationId)
                        .Ascending(x => x.FieldName)
                        .Ascending(x => x.CreatedOn)
                )
            );
        }



        //public async Task<getDashboardResponse> GetReviewerDashboard(getDashboardRequest request)
        //{
        //    getDashboardResponse response = new getDashboardResponse();
        //    var UserId = _acc.HttpContext?.Session.GetString("UserId");
        //    var userFkRole = _user.Find(x => x.Id == UserId).FirstOrDefault()?.Fk_RoleID;
        //    var department = _user.Find(x => x.Id == UserId).FirstOrDefault()?.Department;
        //    var usertype = _role.Find(x => x.Id == userFkRole).FirstOrDefault()?.roleName;

        //    if (usertype?.Trim().ToLower() == "reviewer")
        //    {
        //        try
        //        {
        //            List<CustomerDashboardData> dashboardList = new List<CustomerDashboardData>();
        //            if (department?.Trim().ToLower() == "iso")
        //            {
        //                // Fetch all applications assigned to the user in the certification department
        //                var applications = _iso
        //                    .Find(x => x.IsDelete == false && x.AssignTo == UserId)
        //                    .ToList();
        //                foreach (var app in applications)
        //                {
        //                    var masterCert = _masterCertificate.Find(x => x.Id == app.Fk_Certificate).FirstOrDefault();
        //                    if (masterCert != null)
        //                    {
        //                        var dashboardRecord = new CustomerDashboardData
        //                        {
        //                            Id = app.Id,
        //                            ApplicationId = app.ApplicationId,
        //                            CompanyName = app.Orgnization_Name,
        //                            Certification_Name = masterCert.Certificate_Name,
        //                            Status = (await _status.Find(x => x.Id == app.Status).FirstOrDefaultAsync())?.StatusName ?? "Pending",
        //                            ReceiveDate = app.Application_Received_date,
        //                            AssignedUserName = _user.Find(x => x.Id == app.Fk_UserId).FirstOrDefault()?.FullName
        //                        };
        //                        dashboardList.Add(dashboardRecord);
        //                    }
        //                }
        //                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        //                {
        //                    var searchTerm = request.SearchTerm.Trim().ToLower();
        //                    dashboardList = dashboardList
        //                        .Where(x =>
        //                            (!string.IsNullOrEmpty(x.ApplicationId) && x.ApplicationId.ToLower().Contains(searchTerm)) ||
        //                            (!string.IsNullOrEmpty(x.Certification_Name) && x.Certification_Name.ToLower().Contains(searchTerm))
        //                        )
        //                        .ToList();
        //                }

        //                // Step 6 — Pagination
        //                var totalCount = dashboardList.Count;
        //                var skip = (request.PageNumber - 1) * request.PageSize;

        //                var paginatedList = dashboardList
        //                    .Skip(skip)
        //                    .Take(request.PageSize)
        //                    .ToList();

        //                var pagination = new PageinationDto
        //                {
        //                    PageNumber = request.PageNumber,
        //                    PageSize = request.PageSize,
        //                    TotalRecords = totalCount
        //                };

        //                response.Data = paginatedList;
        //                response.Pagination = pagination;
        //                response.Message = "Dashboard fetched successfully.";
        //                response.HttpStatusCode = System.Net.HttpStatusCode.OK;
        //                response.Success = true;
        //            }
        //           else if (department?.Trim().ToLower() == "icmed")
        //           {
        //                // Fetch all applications assigned to the user in the certification department
        //                var applications = _icmed
        //                    .Find(x => x.IsDelete == false && x.AssignTo == UserId)
        //                    .ToList();
        //                foreach (var app in applications)
        //                {
        //                    var masterCert = _masterCertificate.Find(x => x.Id == app.Fk_Certificate).FirstOrDefault();
        //                    if (masterCert != null)
        //                    {
        //                        var dashboardRecord = new CustomerDashboardData
        //                        {
        //                            Id = app.Id,
        //                            ApplicationId = app.ApplicationId,
        //                            CompanyName = app.Orgnization_Name,
        //                            Certification_Name = masterCert.Certificate_Name,
        //                            Status = (await _status.Find(x => x.Id == app.Status).FirstOrDefaultAsync())?.StatusName ?? "Pending",
        //                            ReceiveDate = app.Application_Received_date,
        //                            AssignedUserName = _user.Find(x => x.Id == app.Fk_UserId).FirstOrDefault()?.FullName
        //                        };
        //                        dashboardList.Add(dashboardRecord);
        //                    }
        //                }
        //                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        //                {
        //                    var searchTerm = request.SearchTerm.Trim().ToLower();
        //                    dashboardList = dashboardList
        //                        .Where(x =>
        //                            (!string.IsNullOrEmpty(x.ApplicationId) && x.ApplicationId.ToLower().Contains(searchTerm)) ||
        //                            (!string.IsNullOrEmpty(x.Certification_Name) && x.Certification_Name.ToLower().Contains(searchTerm))
        //                        )
        //                        .ToList();
        //                }

        //                // Step 6 — Pagination
        //                var totalCount = dashboardList.Count;
        //                var skip = (request.PageNumber - 1) * request.PageSize;

        //                var paginatedList = dashboardList
        //                    .Skip(skip)
        //                    .Take(request.PageSize)
        //                    .ToList();

        //                var pagination = new PageinationDto
        //                {
        //                    PageNumber = request.PageNumber,
        //                    PageSize = request.PageSize,
        //                    TotalRecords = totalCount
        //                };

        //                response.Data = paginatedList;
        //                response.Pagination = pagination;
        //                response.Message = "Dashboard fetched successfully.";
        //                response.HttpStatusCode = System.Net.HttpStatusCode.OK;
        //                response.Success = true;
        //           }
        //           else if (department?.Trim().ToLower() == "icmed_plus")
        //           {
        //                // Fetch all applications assigned to the user in the certification department
        //                var applications = _icmedplus
        //                    .Find(x => x.IsDelete == false && x.AssignTo == UserId)
        //                    .ToList();
        //                foreach (var app in applications)
        //                {
        //                    var masterCert = _masterCertificate.Find(x => x.Id == app.Fk_Certificate).FirstOrDefault();
        //                    if (masterCert != null)
        //                    {
        //                        var dashboardRecord = new CustomerDashboardData
        //                        {
        //                            Id = app.Id,
        //                            ApplicationId = app.ApplicationId,
        //                            CompanyName = app.Orgnization_Name,
        //                            Certification_Name = masterCert.Certificate_Name,
        //                            Status = (await _status.Find(x => x.Id == app.Status).FirstOrDefaultAsync())?.StatusName ?? "Pending",
        //                            ReceiveDate = app.Application_Received_date,
        //                            AssignedUserName = _user.Find(x => x.Id == app.Fk_UserId).FirstOrDefault()?.FullName
        //                        };
        //                        dashboardList.Add(dashboardRecord);
        //                    }
        //                }
        //                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        //                {
        //                    var searchTerm = request.SearchTerm.Trim().ToLower();
        //                    dashboardList = dashboardList
        //                        .Where(x =>
        //                            (!string.IsNullOrEmpty(x.ApplicationId) && x.ApplicationId.ToLower().Contains(searchTerm)) ||
        //                            (!string.IsNullOrEmpty(x.Certification_Name) && x.Certification_Name.ToLower().Contains(searchTerm))
        //                        )
        //                        .ToList();
        //                }

        //                // Step 6 — Pagination
        //                var totalCount = dashboardList.Count;
        //                var skip = (request.PageNumber - 1) * request.PageSize;

        //                var paginatedList = dashboardList
        //                    .Skip(skip)
        //                    .Take(request.PageSize)
        //                    .ToList();

        //                var pagination = new PageinationDto
        //                {
        //                    PageNumber = request.PageNumber,
        //                    PageSize = request.PageSize,
        //                    TotalRecords = totalCount
        //                };

        //                response.Data = paginatedList;
        //                response.Pagination = pagination;
        //                response.Message = "Dashboard fetched successfully.";
        //                response.HttpStatusCode = System.Net.HttpStatusCode.OK;
        //                response.Success = true;
        //           }
        //           else if (department?.Trim().ToLower() == "imdr")
        //           {
        //                var applications = _imdr
        //                   .Find(x => /*x.IsDelete == false &&*/ x.AssignTo == UserId)
        //                   .ToList();
        //                foreach (var app in applications)
        //                {
        //                    var masterCert = _masterCertificate.Find(x => x.Id == app.Fk_Certificate).FirstOrDefault();
        //                    if (masterCert != null)
        //                    {
        //                        var dashboardRecord = new CustomerDashboardData
        //                        {
        //                            Id = app.Id,
        //                            ApplicationId = app.ApplicationId,
        //                            CompanyName = app.Orgnization_Name,
        //                            Certification_Name = masterCert.Certificate_Name,
        //                            Status = (await _status.Find(x => x.Id == app.Status).FirstOrDefaultAsync())?.StatusName ?? "Pending",
        //                            ReceiveDate = app.Application_Received_date,
        //                            AssignedUserName = _user.Find(x => x.Id == app.Fk_UserId).FirstOrDefault()?.FullName
        //                        };
        //                        dashboardList.Add(dashboardRecord);
        //                    }
        //                }
        //                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        //                {
        //                    var searchTerm = request.SearchTerm.Trim().ToLower();
        //                    dashboardList = dashboardList
        //                        .Where(x =>
        //                            (!string.IsNullOrEmpty(x.ApplicationId) && x.ApplicationId.ToLower().Contains(searchTerm)) ||
        //                            (!string.IsNullOrEmpty(x.Certification_Name) && x.Certification_Name.ToLower().Contains(searchTerm))
        //                        )
        //                        .ToList();
        //                }

        //                // Step 6 — Pagination
        //                var totalCount = dashboardList.Count;
        //                var skip = (request.PageNumber - 1) * request.PageSize;

        //                var paginatedList = dashboardList
        //                    .Skip(skip)
        //                    .Take(request.PageSize)
        //                    .ToList();

        //                var pagination = new PageinationDto
        //                {
        //                    PageNumber = request.PageNumber,
        //                    PageSize = request.PageSize,
        //                    TotalRecords = totalCount
        //                };

        //                response.Data = paginatedList;
        //                response.Pagination = pagination;
        //                response.Message = "Dashboard fetched successfully.";
        //                response.HttpStatusCode = System.Net.HttpStatusCode.OK;
        //                response.Success = true;
        //           }
        //           else if (department?.Trim().ToLower() == "fssc")
        //           {
        //                // Fetch all applications assigned to the user in the certification department
        //                var applications = _fssc.Find(x => x.IsDelete == false && x.AssignTo == UserId)
        //                    .ToList();
        //                foreach (var app in applications)
        //                {
        //                    var masterCert = _masterCertificate.Find(x => x.Id == app.Fk_Certificate).FirstOrDefault();
        //                    if (masterCert != null)
        //                    {
        //                        var dashboardRecord = new CustomerDashboardData
        //                        {
        //                            Id = app.Id,
        //                            ApplicationId = app.ApplicationId,
        //                            CompanyName = app.Orgnization_Name,
        //                            Certification_Name = masterCert.Certificate_Name,
        //                            Status = (await _status.Find(x => x.Id == app.Status).FirstOrDefaultAsync())?.StatusName ?? "Pending",
        //                            ReceiveDate = app.Application_Received_date,
        //                            AssignedUserName = _user.Find(x => x.Id == app.Fk_UserId).FirstOrDefault()?.FullName
        //                        };
        //                        dashboardList.Add(dashboardRecord);
        //                    }
        //                }
        //                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        //                {
        //                    var searchTerm = request.SearchTerm.Trim().ToLower();
        //                    dashboardList = dashboardList
        //                        .Where(x =>
        //                            (!string.IsNullOrEmpty(x.ApplicationId) && x.ApplicationId.ToLower().Contains(searchTerm)) ||
        //                            (!string.IsNullOrEmpty(x.Certification_Name) && x.Certification_Name.ToLower().Contains(searchTerm))
        //                        )
        //                        .ToList();
        //                }

        //                // Step 6 — Pagination
        //                var totalCount = dashboardList.Count;
        //                var skip = (request.PageNumber - 1) * request.PageSize;

        //                var paginatedList = dashboardList
        //                    .Skip(skip)
        //                    .Take(request.PageSize)
        //                    .ToList();

        //                var pagination = new PageinationDto
        //                {
        //                    PageNumber = request.PageNumber,
        //                    PageSize = request.PageSize,
        //                    TotalRecords = totalCount
        //                };

        //                response.Data = paginatedList;
        //                response.Pagination = pagination;
        //                response.Message = "Dashboard fetched successfully.";
        //                response.HttpStatusCode = System.Net.HttpStatusCode.OK;
        //                response.Success = true;
        //           }
        //           else
        //           {
        //                response.Message = "Login User Not a ICMED Department";
        //                response.Success = false;
        //                response.HttpStatusCode = System.Net.HttpStatusCode.Unauthorized;
        //                response.ResponseCode = 1;
        //           }
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





        // Common interface for query filtering
        public async Task<getDashboardResponse> GetReviewerDashboard(getDashboardRequest request)
        {
            getDashboardResponse response = new getDashboardResponse();
            var UserId = _acc.HttpContext?.Session.GetString("UserId");

            var userRecord = _user.Find(x => x.Id == UserId).FirstOrDefault();
            var departmentList = userRecord?.Department; // List<string>
            var userFkRole = userRecord?.Fk_RoleID;
            var usertype = _role.Find(x => x.Id == userFkRole).FirstOrDefault()?.roleName;

            if (usertype?.Trim().ToLower() != "reviewer")
            {
                response.Message = "Invalid Token.";
                response.Success = false;
                response.HttpStatusCode = System.Net.HttpStatusCode.Unauthorized;
                response.ResponseCode = 1;
                return response;
            }

            try
            {
                List<CustomerDashboardData> dashboardList = new List<CustomerDashboardData>();

                if (departmentList != null && departmentList.Any())
                {
                    foreach (var dept in departmentList)
                    {
                        switch (dept.Trim().ToLower())
                        {
                            case "iso":
                                dashboardList.AddRange(GetDepartmentApplications(_iso, UserId));
                                break;
                            case "icmed":
                                dashboardList.AddRange(GetDepartmentApplications(_icmed, UserId));
                                break;
                            case "icmed_plus":
                                dashboardList.AddRange(GetDepartmentApplications(_icmedplus, UserId));
                                break;
                            case "imdr":
                                dashboardList.AddRange(GetDepartmentApplications(_imdr, UserId));
                                break;
                            case "fssc":
                                dashboardList.AddRange(GetDepartmentApplications(_fssc, UserId));
                                break;
                        }
                    }
                }

                // Search filter
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

                // Pagination
                var totalCount = dashboardList.Count;
                var skip = (request.PageNumber - 1) * request.PageSize;
                var paginatedList = dashboardList.Skip(skip).Take(request.PageSize).ToList();

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
            catch (Exception ex)
            {
                response.Message = "GetCustomerDashboard Exception: " + ex.Message;
                response.Success = false;
                response.HttpStatusCode = System.Net.HttpStatusCode.InternalServerError;
                response.ResponseCode = 1;
            }

            return response;
        }


        private List<CustomerDashboardData> GetDepartmentApplications<T>(IMongoCollection<T> collection, string userId)where T : class
        {
            var results = new List<CustomerDashboardData>();

            // Pull all documents and filter in memory using reflection
            var allData = collection.Find(Builders<T>.Filter.Empty).ToList();

            var applications = allData
                .Where(x =>
                    (x.GetType().GetProperty("IsDelete")?.GetValue(x) as bool? == false) &&
                    (x.GetType().GetProperty("AssignTo")?.GetValue(x)?.ToString() == userId)
                )
                .ToList();

            foreach (var app in applications)
            {
                var fkCertificate = app.GetType().GetProperty("Fk_Certificate")?.GetValue(app)?.ToString();
                var statusId = app.GetType().GetProperty("Status")?.GetValue(app)?.ToString();
                var fkUserId = app.GetType().GetProperty("Fk_UserId")?.GetValue(app)?.ToString();

                var masterCert = _masterCertificate.Find(x => x.Id == fkCertificate).FirstOrDefault();
                if (masterCert != null)
                {
                    results.Add(new CustomerDashboardData
                    {
                        Id = app.GetType().GetProperty("Id")?.GetValue(app)?.ToString(),
                        ApplicationId = app.GetType().GetProperty("ApplicationId")?.GetValue(app)?.ToString(),
                        ApplicationName = app.GetType().GetProperty("ApplicationName")?.GetValue(app)?.ToString(),
                        CompanyName = app.GetType().GetProperty("Orgnization_Name")?.GetValue(app)?.ToString(),
                        Certification_Name = masterCert.Certificate_Name,
                        Status = _status.Find(x => x.Id == statusId).FirstOrDefault()?.StatusName ?? "Pending",
                        ReceiveDate = app.GetType().GetProperty("Application_Received_date")?.GetValue(app) as DateTime?,
                        AssignedUserName = _user.Find(x => x.Id == fkUserId).FirstOrDefault()?.FullName
                    });
                }
            }

            return results;
        }


        //new code 


        //public async Task<getReviewerApplicationResponse> GetReviewerApplication(getReviewerApplicationRequest request)
        //{
        //    var response = new getReviewerApplicationResponse();
        //    try
        //    {
        //        var userId = _acc.HttpContext?.Session.GetString("UserId");
        //        var userFkRole = _user.Find(x => x.Id == userId).FirstOrDefault()?.Fk_RoleID;
        //        var userType = _role.Find(x => x.Id == userFkRole).FirstOrDefault()?.roleName;

        //        if (userType?.Trim().ToLower() != "reviewer")
        //            return UnauthorizedResponse("Invalid token.");

        //        if (string.IsNullOrWhiteSpace(request.applicationId))
        //            return ErrorResponse("ApplicationId is required.", HttpStatusCode.BadRequest);

        //        // Map CertificationName to collection & type
        //        switch (request.CertificationName?.Trim().ToUpper())
        //        {
        //            case "ISO":
        //                return await GetReviewerApplicationData(_iso, request.applicationId);
        //            case "FSSC":
        //                return await GetReviewerApplicationData(_fssc, request.applicationId);
        //            case "ICMED":
        //                return await GetReviewerApplicationData(_icmed, request.applicationId);
        //            case "ICMED_PLUS":
        //                return await GetReviewerApplicationData(_icmedplus, request.applicationId);
        //            case "IMDR":
        //                return await GetReviewerApplicationData(_imdr, request.applicationId);
        //            default:
        //                return ErrorResponse("Invalid Certification Name.", HttpStatusCode.BadRequest);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return ErrorResponse("GetCustomerApplication Exception: " + ex.Message, HttpStatusCode.InternalServerError);
        //    }
        //}

        // Generic helper method
        //private async Task<getReviewerApplicationResponse> GetReviewerApplicationData<T>(IMongoCollection<T> collection,string applicationId) where T : class
        //{
        //    var response = new getReviewerApplicationResponse();

        //    var filter = Builders<T>.Filter.Eq("Id", applicationId); // Using string property name
        //    var data = await collection.Find(filter).FirstOrDefaultAsync();

        //    if (data == null)
        //        return ErrorResponse("Application not found.", HttpStatusCode.NotFound);

        //    // Reflection to get values dynamically
        //    var fkCertificate = data.GetType().GetProperty("Fk_Certificate")?.GetValue(data)?.ToString();
        //    var assignTo = data.GetType().GetProperty("AssignTo")?.GetValue(data)?.ToString();
        //    var statusId = data.GetType().GetProperty("Status")?.GetValue(data)?.ToString();

        //    var certificateName = _mongoDbService.Getcertificatename(fkCertificate);
        //    var statusName = _mongoDbService.StatusName(statusId);

        //    response.Data = data;
        //    response.CertificateName = certificateName;
        //    response.statusName = statusName;
        //    response.Message = "Data fetched successfully.";
        //    response.HttpStatusCode = HttpStatusCode.OK;
        //    response.Success = true;
        //    response.ResponseCode = 0;

        //    return response;
        //}

        // Helper for errors
        private getReviewerApplicationResponse ErrorResponse(string message, HttpStatusCode status)
        {
            return new getReviewerApplicationResponse
            {
                Message = message,
                HttpStatusCode = status,
                Success = false,
                ResponseCode = 1
            };
        }

        // Helper for unauthorized
        private getReviewerApplicationResponse UnauthorizedResponse(string message)
        {
            return ErrorResponse(message, HttpStatusCode.Unauthorized);
        }


        /// <summary> parller view coed

        public async Task<getReviewerApplicationResponse> GetReviewerApsplication(getReviewerApplicationRequest request)
        {
            var userId = _acc.HttpContext?.Session.GetString("UserId");
            var userFkRole = _user.Find(x => x.Id == userId).FirstOrDefault()?.Fk_RoleID;
            var userType = _role.Find(x => x.Id == userFkRole).FirstOrDefault()?.roleName;

            if (userType?.Trim().ToLower() != "reviewer")
                return UnauthorizedResponse<getReviewerApplicationResponse>("Invalid token.");

            if (string.IsNullOrWhiteSpace(request.applicationId) || string.IsNullOrWhiteSpace(request.CertificationName))
                return ErrorResponse<getReviewerApplicationResponse>("applicationId and CertificationName are required.", System.Net.HttpStatusCode.BadRequest);

            var cert = request.CertificationName.Trim().ToUpper();

            try
            {
                switch (cert)
                {
                    case "ISO":
                        return await GetReviewerApplicationDataGeneric(_iso, request.applicationId, cert, userId);
                    case "FSSC":
                        return await GetReviewerApplicationDataGeneric(_fssc, request.applicationId, cert, userId);
                    case "ICMED":
                        return await GetReviewerApplicationDataGeneric(_icmed, request.applicationId, cert, userId);
                    case "ICMED_PLUS":
                        return await GetReviewerApplicationDataGeneric(_icmedplus, request.applicationId, cert, userId);
                    case "IMDR":
                        return await GetReviewerApplicationDataGeneric(_imdr, request.applicationId, cert, userId);
                    default:
                        return ErrorResponse<getReviewerApplicationResponse>("Invalid Certification Name.", System.Net.HttpStatusCode.BadRequest);
                }
            }
            catch (Exception ex)
            {
                return ErrorResponse<getReviewerApplicationResponse>("GetReviewerApplication Exception: " + ex.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }

        // Generic: load typed base, plus parallel drafts, diffs, comments, history
        private async Task<getReviewerApplicationResponse> GetReviewerApplicationDataGeneric<T>(IMongoCollection<T> collection,string applicationId,string certificationName,string currentReviewerId) where T : class
        {
            // Load base per-cert doc by Id (typed)
            var baseDoc = await collection.Find(Builders<T>.Filter.Eq("Id", applicationId)).FirstOrDefaultAsync();
            if (baseDoc == null)
                return ErrorResponse<getReviewerApplicationResponse>("Application not found.", System.Net.HttpStatusCode.NotFound);

            // Resolve certificate FK and status for display (optional)
            var fkCertificate = baseDoc.GetType().GetProperty("Fk_Certificate")?.GetValue(baseDoc)?.ToString();
            var statusId = baseDoc.GetType().GetProperty("Status")?.GetValue(baseDoc)?.ToString();
            var certificateName = _mongoDbService.Getcertificatename(fkCertificate);
            var statusName = _mongoDbService.StatusName(statusId);

            // We linked _reviews to tbl_customer_certificates.Id earlier. If you link to per-cert Id, use applicationId here instead.
            // Assume you used per-cert Id for simplicity (it matches this method signature nicely).
            var appKey = applicationId;

            // pull your review
            var yourReview = await _reviews.Find(x =>
                x.CertificationName == certificationName &&
                x.Fk_ApplicationId == appKey &&
                x.ReviewerId == currentReviewerId).FirstOrDefaultAsync();

            // pull the other review (if any)
            var otherReview = await _reviews.Find(x =>
                x.CertificationName == certificationName &&
                x.Fk_ApplicationId == appKey &&
                x.ReviewerId != currentReviewerId).FirstOrDefaultAsync();

            var yourDraft = yourReview?.Fields ?? new BsonDocument();
            var otherDraft = otherReview?.Fields ?? new BsonDocument();

            // build diffs on overlapping keys (you can expand the diff policy as needed)
            var diffs = BuildDiffs(yourDraft, otherDraft);

            // comments + history
            var comments = await _comments.Find(x =>
                x.CertificationName == certificationName /*&& x.Fk_ApplicationId == appKey*/).ToListAsync();

            var history = await _history.Find(x =>
                x.CertificationName == certificationName && x.Fk_ApplicationId == appKey)
                .SortBy(x => x.ChangedOn).ToListAsync();

            // pack a VM with typed base doc for your UI
            var vm = new ReviewerApplicationVM<T>
            {
                Base = baseDoc,
                YourDraft = yourDraft,
                OtherDraft = otherDraft,
                Diffs = diffs,
                Comments = comments,
                History = history,
                CertificateName = certificateName,
                StatusName = statusName
            };

            return new getReviewerApplicationResponse
            {
                Success = true,
                Message = "Data fetched successfully.",
                HttpStatusCode = System.Net.HttpStatusCode.OK,
                ResponseCode = 0,
                Data = vm
            };
        }

        private List<FieldDiff> BuildDiffs(BsonDocument a, BsonDocument b)
        {
            var result = new List<FieldDiff>();
            var allKeys = a.Names.Union(b.Names).Distinct();

            foreach (var key in allKeys)
            {
                var hasA = a.TryGetValue(key, out var va);
                var hasB = b.TryGetValue(key, out var vb);

                if (!hasA && !hasB) continue;

                bool conflict = hasA && hasB && !BsonValue.Equals(va, vb);
                result.Add(new FieldDiff
                {
                    Field = key,
                    Reviewer1Value = va,      // caller can map who is R1/R2
                    Reviewer2Value = vb,
                    HasConflict = conflict
                });
            }
            return result;
        }

        // Small helpers mirroring your pattern
        private TResp UnauthorizedResponse<TResp>(string msg) where TResp : new()
        {
            dynamic r = new TResp();
            r.Success = false; r.Message = msg; r.HttpStatusCode = System.Net.HttpStatusCode.Unauthorized; r.ResponseCode = 1;
            return r;
        }
        private TResp ErrorResponse<TResp>(string msg, System.Net.HttpStatusCode code) where TResp : new()
        {
            dynamic r = new TResp();
            r.Success = false; r.Message = msg; r.HttpStatusCode = code; r.ResponseCode = 1;
            return r;
        }


        /// </summary>


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

                        //  var data = await _iso.Find(filter).FirstOrDefaultAsync();
                        var data = await _iso.Find(filter).FirstOrDefaultAsync();
                        var allVersions = await _iso
                       .Find(x => x.Id == request.applicationId)
                       .ToListAsync();


                        if (allVersions == null || !allVersions.Any())
                        {
                            response.Message = "Application not found..";
                            response.HttpStatusCode = System.Net.HttpStatusCode.NotFound;
                            response.Success = false;
                            response.ResponseCode = 1;
                            return response;
                        }

                        var ownreviewerfilter = Builders<tbl_ISO_Application>.Filter.And(
                   Builders<tbl_ISO_Application>.Filter.Eq(x => x.ApplicationId, data.ApplicationId),
                   Builders<tbl_ISO_Application>.Filter.Eq(x => x.Fk_Certificate, data.Fk_Certificate),
                  Builders<tbl_ISO_Application>.Filter.Eq(x => x.AssignTo, data.AssignTo),
                   Builders<tbl_ISO_Application>.Filter.Ne(x => x.AssignTo, "686fc25880b29ec6e7867768")
               );
                        var ownReviewerfilter = await _iso.Find(ownreviewerfilter).FirstOrDefaultAsync();

                        var anotherreviewerfilter = Builders<tbl_ISO_Application>.Filter.And(
                            Builders<tbl_ISO_Application>.Filter.Eq(x => x.ApplicationId, data.ApplicationId),
                            Builders<tbl_ISO_Application>.Filter.Eq(x => x.Fk_Certificate, data.Fk_Certificate),
                           Builders<tbl_ISO_Application>.Filter.Ne(x => x.AssignTo, data.AssignTo),
                            Builders<tbl_ISO_Application>.Filter.Ne(x => x.AssignTo, "686fc25880b29ec6e7867768")
                        );
                        var anotherReviewerData = await _iso.Find(anotherreviewerfilter).FirstOrDefaultAsync();

                        var adminfilter = Builders<tbl_ISO_Application>.Filter.And(
                           Builders<tbl_ISO_Application>.Filter.Eq(x => x.ApplicationId, data.ApplicationId),
                           Builders<tbl_ISO_Application>.Filter.Eq(x => x.Fk_Certificate, data.Fk_Certificate),
                           Builders<tbl_ISO_Application>.Filter.Ne(x => x.AssignTo, data.AssignTo),
                           Builders<tbl_ISO_Application>.Filter.Ne(x => x.AssignTo, anotherReviewerData.AssignTo)
                        );

                        var adminData = await _iso.Find(adminfilter).FirstOrDefaultAsync();


                        if (anotherReviewerData != null)
                        {
                            if (adminData != null)
                            {
                                // Priority: Admin > Reviewer > Trainee
                                MergeDataInPlace(data, adminData, anotherReviewerData);
                            }
                            else
                            {
                                // Only Reviewer > Trainee
                                //MergeDataInPlace(anotherReviewerData, null,ownReviewerfilter);
                                MergeDataInPlace(data, anotherReviewerData, ownReviewerfilter);
                            }
                        }


                        // Corrected code to fetch and merge all versions




                        //// Find the version for the current user and other relevant versions.
                        //var currentUserVersion = allVersions.FirstOrDefault(x => x.AssignTo == UserId.ToString());
                        //var baseVersion = allVersions.FirstOrDefault(x => string.IsNullOrEmpty(x.AssignTo));
                        //var otherUserVersions = allVersions.Where(x => x.AssignTo != UserId.ToString() && !string.IsNullOrEmpty(x.AssignTo));

                        // Start with the base document or a new one if it doesn't exist.
                        //var mergedDocument = baseVersion ?? new tbl_ISO_Application();

                        // Merge all other versions, including the current user's, into the base document.
                        // This ensures all changes are combined.
                        //foreach (var version in allVersions.Where(x => x.ApplicationId == request.applicationId))
                        //{
                        //    mergedDocument = MergeDataInPlace(baseVersion, version);
                        //}



                        var cerificateName = _mongoDbService.Getcertificatename(data.Fk_Certificate);
                        var assignmane = _mongoDbService.ReviewerName(data.AssignTo);
                        var status = _mongoDbService.StatusName(data.Status);
                        var comments = await GetFieldCommentsAsync(data.ApplicationId, data.Fk_Certificate, UserId, _comments);

                        response.Data = data;
                        response.Comments = comments;
                        response.CertificateName = cerificateName;
                        response.statusName = status;

                    }
                    else if (request.CertificationName == "FSSC")
                    {
                        var filter = Builders<tbl_FSSC_Application>.Filter.Eq(x => x.Id, request.applicationId);

                        var data = await _fssc.Find(filter).FirstOrDefaultAsync();
                        var cerificateName = _mongoDbService.Getcertificatename(data.Fk_Certificate);
                        var assignmane = _mongoDbService.ReviewerName(data.AssignTo);
                        var status = _mongoDbService.StatusName(data.Status);


                        response.Data = data;
                        response.CertificateName = cerificateName;
                        response.statusName = status;
                    }
                    else if (request.CertificationName == "ICMED")
                    {
                        var filter = Builders<tbl_ICMED_Application>.Filter.Eq(x => x.Id, request.applicationId);

                        var data = await _icmed.Find(filter).FirstOrDefaultAsync();
                        var cerificateName = _mongoDbService.Getcertificatename(data.Fk_Certificate);
                        var assignmane = _mongoDbService.ReviewerName(data.AssignTo);
                        var status = _mongoDbService.StatusName(data.Status);


                        response.Data = data;
                        response.CertificateName = cerificateName;
                        response.statusName = status;
                    }
                    else if (request.CertificationName == "ICMED_PLUS")
                    {
                        var filter = Builders<tbl_ICMED_PLUS_Application>.Filter.Eq(x => x.Id, request.applicationId);

                        var data = await _icmedplus.Find(filter).FirstOrDefaultAsync();
                        var cerificateName = _mongoDbService.Getcertificatename(data.Fk_Certificate);
                        var assignmane = _mongoDbService.ReviewerName(data.AssignTo);
                        var status = _mongoDbService.StatusName(data.Status);


                        response.Data = data;
                        response.CertificateName = cerificateName;
                        response.statusName = status;
                    }
                    else if (request.CertificationName == "IMDR")
                    {
                        var filter = Builders<tbl_IMDR_Application>.Filter.Eq(x => x.Id, request.applicationId);

                        var data = await _imdr.Find(filter).FirstOrDefaultAsync();
                        var cerificateName = _mongoDbService.Getcertificatename(data.Fk_Certificate);
                        var assignmane = _mongoDbService.ReviewerName(data.AssignTo);
                        var status = _mongoDbService.StatusName(data.Status);


                        response.Data = data;
                        response.CertificateName = cerificateName;
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



        public async Task<getReviewerApplicationResponse> GetApplicationHistory(getApplicationHistoryRequest request)
        {
            getReviewerApplicationResponse response = new getReviewerApplicationResponse();
            try
            {
                var UserId = _acc.HttpContext?.Session.GetString("UserId");
                var userFkRole = _user.Find(x => x.Id == UserId).FirstOrDefault()?.Fk_RoleID;
                var userType = _role.Find(x => x.Id == userFkRole).FirstOrDefault()?.roleName;

                if (userType?.Trim().ToLower() !="customer")
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
                        var filter = Builders<tbl_ISO_Application>.Filter.Eq(x => x.ApplicationId, request.applicationId)
                             & Builders<tbl_ISO_Application>.Filter.Eq(x => x.AssignTo, request.userId);

                        var data = await _iso.Find(filter).FirstOrDefaultAsync();


                        var cerificateName = _mongoDbService.Getcertificatename(data.Fk_Certificate);
                        var assignmane = _mongoDbService.ReviewerName(data.AssignTo);
                        var status = _mongoDbService.StatusName(data.Status);
                        var comments = await GetFieldCommentsAsync(data.ApplicationId, data.Fk_Certificate, UserId, _comments);
                        var assignrole = _mongoDbService.UserRoleType(data.AssignTo);

                        response.Data = data;
                        response.Comments = comments;
                        response.AssigntToName = assignmane;
                        response.AssignRole = assignrole;
                        response.CertificateName = cerificateName;
                        response.statusName = status;

                    }
                    else if (request.CertificationName == "FSSC")
                    {
                        var filter = Builders<tbl_FSSC_Application>.Filter.Eq(x => x.Id, request.applicationId);

                        var data = await _fssc.Find(filter).FirstOrDefaultAsync();
                        var cerificateName = _mongoDbService.Getcertificatename(data.Fk_Certificate);
                        var assignmane = _mongoDbService.ReviewerName(data.AssignTo);
                        var assignrole= _mongoDbService.UserRoleType(data.AssignTo);
                        var status = _mongoDbService.StatusName(data.Status);


                        response.Data = data;
                        response.CertificateName = cerificateName;
                        response.statusName = status;
                    }
                    else if (request.CertificationName == "ICMED")
                    {
                        var filter = Builders<tbl_ICMED_Application>.Filter.Eq(x => x.Id, request.applicationId);

                        var data = await _icmed.Find(filter).FirstOrDefaultAsync();
                        var cerificateName = _mongoDbService.Getcertificatename(data.Fk_Certificate);
                        var assignmane = _mongoDbService.ReviewerName(data.AssignTo);
                        var status = _mongoDbService.StatusName(data.Status);


                        response.Data = data;
                        response.CertificateName = cerificateName;
                        response.statusName = status;
                    }
                    else if (request.CertificationName == "ICMED_PLUS")
                    {
                        var filter = Builders<tbl_ICMED_PLUS_Application>.Filter.Eq(x => x.Id, request.applicationId);

                        var data = await _icmedplus.Find(filter).FirstOrDefaultAsync();
                        var cerificateName = _mongoDbService.Getcertificatename(data.Fk_Certificate);
                        var assignmane = _mongoDbService.ReviewerName(data.AssignTo);
                        var status = _mongoDbService.StatusName(data.Status);


                        response.Data = data;
                        response.CertificateName = cerificateName;
                        response.statusName = status;
                    }
                    else if (request.CertificationName == "IMDR")
                    {
                        var filter = Builders<tbl_IMDR_Application>.Filter.Eq(x => x.Id, request.applicationId);

                        var data = await _imdr.Find(filter).FirstOrDefaultAsync();
                        var cerificateName = _mongoDbService.Getcertificatename(data.Fk_Certificate);
                        var assignmane = _mongoDbService.ReviewerName(data.AssignTo);
                        var status = _mongoDbService.StatusName(data.Status);


                        response.Data = data;
                        response.CertificateName = cerificateName;
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

        private void MergeDataInPlace<T>(T target, T source)
        {
            var excludedFields = new[] { "AssignTo", "Id", "UserFk", "ActiveState" };

            // Step 1: Check if source has at least one non-empty field
            bool hasAnyValue = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => !excludedFields.Contains(p.Name))
                .Any(p => HasValue(p.GetValue(source)));

            if (!hasAnyValue)
            {
                // Nothing to merge, return
                return;
            }

            // Step 2: Merge values (field-by-field) from source to target
            foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (excludedFields.Contains(prop.Name))
                    continue;

                var sourceValue = prop.GetValue(source);
                if (HasValue(sourceValue))
                {
                    prop.SetValue(target, sourceValue);
                }
            }
        }



        private void MergeDataInPlace<T>(T target, params T[] sources)
        {
            var excludedFields = new[] { "AssignTo", "Id", "UserFk", "ActiveState", "ActiveReviwer" };

            // Expect that all sources have an UpdatedAt property
            var updatedAtProp = typeof(T).GetProperty("UpdatedAt");

            foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (excludedFields.Contains(prop.Name))
                    continue;

                object selectedValue = null;
                DateTime latestUpdate = DateTime.MinValue;

                foreach (var source in sources.Where(s => s != null))
                {
                    var value = prop.GetValue(source);
                    var updatedAt = (DateTime?)updatedAtProp?.GetValue(source);

                    if (HasValue(value) && updatedAt.HasValue && updatedAt.Value > latestUpdate)
                    {
                        latestUpdate = updatedAt.Value;
                        selectedValue = value;
                    }
                }

                if (selectedValue != null)
                {
                    prop.SetValue(target, selectedValue);
                }
            }
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

        //private bool HasValue(object value)
        //{
        //    if (value == null) return false;
        //    if (value is string str) return !string.IsNullOrWhiteSpace(str);

        //    var type = value.GetType();
        //    if (type.IsValueType)
        //        return !value.Equals(Activator.CreateInstance(type));

        //    if (value is System.Collections.IEnumerable enumerable && !(value is string))
        //        return enumerable.GetEnumerator().MoveNext();

        //    return true;
        //}

        //private void MergeDataInPlace<T>(T target, T admin, T reviewer)
        //{
        //    var excludedFields = new[] { "AssignTo", "Id", "UserFk", "ActiveState", "ActiveReviwer" };

        //    foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        //    {
        //        if (excludedFields.Contains(prop.Name))
        //            continue;

        //        var currentValue = prop.GetValue(target);   // trainee
        //        var adminValue = admin != null ? prop.GetValue(admin) : null;
        //        var reviewerValue = reviewer != null ? prop.GetValue(reviewer) : null;

        //        // Handle collections (like reviewerKeyPersonnel)
        //        if (typeof(System.Collections.IEnumerable).IsAssignableFrom(prop.PropertyType) && prop.PropertyType != typeof(string))
        //        {
        //            var merged = MergeCollections(currentValue, adminValue, reviewerValue);
        //            prop.SetValue(target, merged);
        //            continue;
        //        }

        //        // Normal scalar merge
        //        if (HasValue(adminValue))
        //        {
        //            prop.SetValue(target, adminValue);
        //        }
        //        else if (HasValue(reviewerValue))
        //        {
        //            prop.SetValue(target, reviewerValue);
        //        }
        //    }
        //}

        //private object MergeCollections(object traineeVal, object adminVal, object reviewerVal)
        //{
        //    var traineeList = (traineeVal as System.Collections.IEnumerable)?.Cast<object>().ToList() ?? new List<object>();
        //    var adminList = (adminVal as System.Collections.IEnumerable)?.Cast<object>().ToList() ?? new List<object>();
        //    var reviewerList = (reviewerVal as System.Collections.IEnumerable)?.Cast<object>().ToList() ?? new List<object>();

        //    // Detect element type (LabelValue instead of object)
        //    var elementType = traineeVal?.GetType().GetGenericArguments().FirstOrDefault()
        //                   ?? reviewerVal?.GetType().GetGenericArguments().FirstOrDefault()
        //                   ?? adminVal?.GetType().GetGenericArguments().FirstOrDefault()
        //                   ?? typeof(object);

        //    var listType = typeof(List<>).MakeGenericType(elementType);
        //    var mergedList = (System.Collections.IList)Activator.CreateInstance(listType);

        //    // Try to detect a "key property" like ActivityName
        //    string keyProp = traineeList.Concat(reviewerList).Concat(adminList)
        //                                .SelectMany(x => x.GetType().GetProperties())
        //                                .Select(p => p.Name)
        //                                .FirstOrDefault(n => n.Equals("ActivityName", StringComparison.OrdinalIgnoreCase));

        //    if (keyProp != null)
        //    {
        //        // Merge by key
        //        var allItems = traineeList.Concat(reviewerList).Concat(adminList)
        //                                  .GroupBy(x => x.GetType().GetProperty(keyProp)?.GetValue(x)?.ToString());

        //        foreach (var group in allItems)
        //        {
        //            var traineeItem = group.FirstOrDefault(x => traineeList.Contains(x));
        //            var reviewerItem = group.FirstOrDefault(x => reviewerList.Contains(x));
        //            var adminItem = group.FirstOrDefault(x => adminList.Contains(x));

        //            mergedList.Add(MergeObject(traineeItem, reviewerItem, adminItem, elementType));
        //        }
        //    }
        //    else
        //    {
        //        // No key property → merge by index
        //        int maxCount = new[] { traineeList.Count, reviewerList.Count, adminList.Count }.Max();

        //        for (int i = 0; i < maxCount; i++)
        //        {
        //            var traineeItem = i < traineeList.Count ? traineeList[i] : null;
        //            var reviewerItem = i < reviewerList.Count ? reviewerList[i] : null;
        //            var adminItem = i < adminList.Count ? adminList[i] : null;

        //            mergedList.Add(MergeObject(traineeItem, reviewerItem, adminItem, elementType));
        //        }
        //    }

        //    return mergedList;
        //}

        //private object MergeObject(object trainee, object reviewer, object admin, Type elementType)
        //{
        //    var instance = Activator.CreateInstance(elementType);

        //    foreach (var prop in elementType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        //    {
        //        var traineeValue = trainee != null ? prop.GetValue(trainee) : null;
        //        var reviewerValue = reviewer != null ? prop.GetValue(reviewer) : null;
        //        var adminValue = admin != null ? prop.GetValue(admin) : null;

        //        if (HasValue(adminValue))
        //            prop.SetValue(instance, adminValue);
        //        else if (HasValue(reviewerValue))
        //            prop.SetValue(instance, reviewerValue);
        //        else
        //            prop.SetValue(instance, traineeValue);
        //    }

        //    return instance;
        //}


        private async Task<List<tbl_ApplicationFieldComment>> GetFieldCommentsAsync(string applicationId,string fkCertificate,string UserId, IMongoCollection<tbl_ApplicationFieldComment> commentCollection)
        {
            var filter = Builders<tbl_ApplicationFieldComment>.Filter.And(
                Builders<tbl_ApplicationFieldComment>.Filter.Eq(c => c.ApplicationId, applicationId),
                Builders<tbl_ApplicationFieldComment>.Filter.Eq(c => c.CertificationName, fkCertificate)
                //Builders<tbl_ApplicationFieldComment>.Filter.Ne(c => c.Fk_User, UserId)
            );

          
            var pipeline = commentCollection.Aggregate()
        .Match(filter)
        .SortByDescending(c => c.CreatedOn) // or use c.Id
        .Group(
            key => key.FieldName,
            g => g.First() 
        );

            return await pipeline.ToListAsync();
        }

        public string ProcessSubmit(addReviewerApplicationRequest request)
        {
            string Adminstatus = "687a2925694d00158c9bf265"; // InProgress
           

            if (!string.IsNullOrWhiteSpace(request.IsFinalSubmit)
                && request.IsFinalSubmit.Trim().ToLower() == "true")
            {
                if (request.ActiveReviwer == "ReviwerOne")
                    Adminstatus = "68930d9066a57e1b128af2e9"; // Reviewer One Submit
                else if (request.ActiveReviwer == "ReviwerTwo")
                    Adminstatus = "6895d649f3fbe9ce595243cc"; // Reviewer Two Submit
                else
                    Adminstatus = "687a2925694d00158c9bf267"; // SendToApproval
            }

            return Adminstatus;
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
                    string status = ProcessSubmit(request); ;//InProgress
                    string Adminstatus = ProcessSubmit(request); ;//InProgress

                    // If user provided isFinalSubmit flag
                    //if (!string.IsNullOrWhiteSpace(request.IsFinalSubmit))
                    //{
                    //    isFinalSubmit = request.IsFinalSubmit.Trim().ToLower() == "true";

                    //    if (isFinalSubmit == true)
                    //    {
                    //        submitDate = DateTime.Now;
                    //        status = "687a2925694d00158c9bf267"; //SendToApproval
                    //        if (request.ActiveReviwer == "ReviwerOne")
                    //        {
                    //            Adminstatus = "68930d9066a57e1b128af2e9"; //  Reviewer One Submit
                    //        }
                    //        else if (request.ActiveReviwer == "ReviwerTwo")
                    //        {
                    //            Adminstatus = "6895d649f3fbe9ce595243cc"; // Reviewer Two Submit
                    //        }
                    //    }


                    //}

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

                        var application = await _customercertificates.Find(x => x.Id == request.ApplicationId).FirstOrDefaultAsync();
                        if (application != null)
                        {
                            var updatestatus = Builders<tbl_customer_certificates>.Update.Set(x => x.status, Adminstatus);
                            await _customercertificates.UpdateOneAsync(x => x.Id == request.ApplicationId, updatestatus);
                        }

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

            if (userType?.Trim().ToLower() == "reviewer")
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
                            .Set(x => x.AssignTo, request.AssignTo)
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
                        var application = await _customercertificates.Find(x => x.Id == request.ApplicationId).FirstOrDefaultAsync();
                        if (application != null)
                        {
                            var updatestatus = Builders<tbl_customer_certificates>.Update.Set(x => x.status, Adminstatus);
                            await _customercertificates.UpdateOneAsync(x => x.Id == request.ApplicationId, updatestatus);
                        }
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

            if (userType?.Trim().ToLower() == "reviewer")
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
                            .Set(x => x.remark, request.remark)
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


        public async Task<addReviewerApplicationResponse> SaveIMDRApplication(addIMDRApplicationRequest request)
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
                    string Adminstatus = "687a2925694d00158c9bf265";

                    // If user provided isFinalSubmit flag
                    if (!string.IsNullOrWhiteSpace(request.IsFinalSubmit))
                    {
                        isFinalSubmit = request.IsFinalSubmit.Trim().ToLower() == "true";

                        if (isFinalSubmit == true)
                        {
                            submitDate = DateTime.Now;
                            status = "687a2925694d00158c9bf267"; // Final Submit status
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
                        .Set(x => x.CreatedBy,  UserId)
                        .Set(x => x.UpdatedBy,  UserId)
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
                        var application = await _customercertificates.Find(x => x.Id == request.ApplicationId).FirstOrDefaultAsync();
                        if (application != null)
                        {
                            var updatestatus = Builders<tbl_customer_certificates>.Update.Set(x => x.status, Adminstatus);
                            await _customercertificates.UpdateOneAsync(x => x.Id == request.ApplicationId, updatestatus);
                        }
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


            try
            {
                var userId = _acc.HttpContext?.Session.GetString("UserId");
                var filter = Builders<tbl_IMDR_Application>.Filter.Eq(x => x.Id, request.Id);

                

                response.Message = "IMDR application saved successfully.";
                response.Success = true;
                response.HttpStatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                response.Message = $"Error saving application: {ex.Message}";
                response.Success = false;
                response.HttpStatusCode = HttpStatusCode.InternalServerError;
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

            if (userType?.Trim().ToLower() == "reviewer")
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


        public async Task<BaseResponse> AddFieldComment(FieldCommentRequest request)
        {
            var response = new addReviewerApplicationResponse();
            var UserId = _acc.HttpContext?.Session.GetString("UserId");

            // Basic validation
            if (string.IsNullOrWhiteSpace(request.ApplicationId) ||
                string.IsNullOrWhiteSpace(request.FieldName) ||
                string.IsNullOrWhiteSpace(UserId) ||
                string.IsNullOrWhiteSpace(request.CommentText))
            {
                response.Message = "ApplicationId, FieldName, Reviewer, and CommentText are required.";
                response.HttpStatusCode = System.Net.HttpStatusCode.BadRequest;
                response.Success = false;
                response.ResponseCode = 1;
                return response;
            }

            // Fetch application
            var application = await _iso
                .Find(x => x.ApplicationId == request.ApplicationId)
                .FirstOrDefaultAsync();

            if (application == null)
            {
                response.Message = "Application not found.";
                response.HttpStatusCode = System.Net.HttpStatusCode.NotFound;
                response.Success = false;
                response.ResponseCode = 1;
                return response;
            }

            // Fetch certificate
            var certificate = await _masterCertificate
                .Find(x => x.Id == application.Fk_Certificate)
                .FirstOrDefaultAsync();

            if (certificate == null)
            {
                response.Message = "Certificate not found.";
                response.HttpStatusCode = System.Net.HttpStatusCode.NotFound;
                response.Success = false;
                response.ResponseCode = 1;
                return response;
            }

            // Fetch reviewer info
            var reviewerUser = await _user
                .Find(x => x.Id == UserId)
                .FirstOrDefaultAsync();

            if (reviewerUser == null)
            {
                response.Message = "Reviewer user not found.";
                response.HttpStatusCode = System.Net.HttpStatusCode.NotFound;
                response.Success = false;
                response.ResponseCode = 1;
                return response;
            }

            // Create comment object
            var comment = new tbl_ApplicationFieldComment
            {
                CertificationName = certificate.Id,
                ApplicationId = request.ApplicationId,
                Fk_User = UserId,
                FieldName = request.FieldName,
                FieldUserType = reviewerUser.Type,
                CommentText = request.CommentText,
                CreatedBy = UserId,
                CreatedOn = DateTime.Now
            };

            await _comments.InsertOneAsync(comment);

            // Success response
            response.Message = "Comment added successfully.";
            response.HttpStatusCode = System.Net.HttpStatusCode.OK;
            response.Success = true;
            response.ResponseCode = 0;

            return response;
        }


        protected override void Disposing()
        {
            //throw new NotImplementedException();
        }
    }
}
