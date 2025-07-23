using System;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ZenithApp.Models;
using ZenithApp.Settings;
using ZenithApp.ZenithEntities;
using ZenithApp.ZenithMessage;

namespace ZenithApp.ZenithRepository
{
    public class CustomerRepository : BaseRepository
    {
        private readonly IMongoCollection<tbl_customer_application> _customer;
        private readonly IMongoCollection<tbl_User_Role> _role;
        private readonly IMongoCollection<tbl_user> _user;
        private readonly IMongoCollection<tbl_master_certificates> _masterCertificate;
        private readonly IMongoCollection<tbl_customer_certificates> _customercertificates;
        private readonly IMongoCollection<tbl_customer_key_personnels> _customerKeyPersonnel;
        private readonly IMongoCollection<tbl_customer_site> _customersite;
        private readonly IMongoCollection<tbl_customer_Entity> _customerentity;
        private readonly IMongoCollection<tbl_master_certificates> _mastercertificate;



        private readonly IHttpContextAccessor _acc;

        public CustomerRepository(IOptions<MongoDbSettings> settings, IHttpContextAccessor acc)
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
            _mastercertificate = database.GetCollection<tbl_master_certificates>("tbl_master_certificates");

            _acc = acc;
        }

        public List<tbl_customer_application> GetAll() => _customer.Find(s => true).ToList();



        public void Create(tbl_customer_application customer_Application) =>
            _customer.InsertOne(customer_Application);

        public List<tbl_customer_certificates> GetCustomerCertificates() => _customercertificates.Find(s => true).ToList();



        public void CreateCustomerCertification(tbl_customer_certificates customer_Certificates) =>
            _customercertificates.InsertOne(customer_Certificates);






        public async Task<getCretificationsbyAppIdResponse> getCretificationsbyAppId(getCretificationsbyAppIdRequest request)
        {
            var response = new getCretificationsbyAppIdResponse();
            try
            {
                var userId = _acc.HttpContext?.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    response.Message = "Session expired or invalid user.";
                    response.Success = false;
                    response.HttpStatusCode = System.Net.HttpStatusCode.Unauthorized;
                    response.ResponseCode = 1;
                    return response;
                }
                var userFkRole = _user.Find(x => x.Id == userId).FirstOrDefault()?.Fk_RoleID;
                var userType = _role.Find(x => x.Id == userFkRole).FirstOrDefault()?.roleName;

                if (userType?.Trim().ToLower() != "customer")
                {
                    response.Message = "Invalid token or unauthorized user.";
                    response.Success = false;
                    response.HttpStatusCode = System.Net.HttpStatusCode.Unauthorized;
                    response.ResponseCode = 1;
                    return response;
                }



                var applications = _customer
                    .Find(x => x.IsDelete == false && x.Id==request.apllicationId)
                    .ToList();


                var dashboardList = new List<CustomerDashboardData>();
                foreach (var app in applications)
                {
                    var certificates = _customercertificates
                        .Find(x => x.Fk_Customer_Application ==request.apllicationId && x.Is_Delete == false)
                        .ToList();

                    foreach (var cert in certificates)
                    {
                        var masterCert = _masterCertificate
                            .Find(x => x.Id == cert.Fk_Certificates)
                            .FirstOrDefault();

                        if (masterCert != null)
                        {
                            dashboardList.Add(new CustomerDashboardData
                            {
                                Id = app.Id,
                                ApplicationId = app.ApplicationId,
                                Certification_Name = masterCert.Certificate_Name,
                                Status = app.status ?? "Pending",

                                ReceiveDate = app.CreatedAt
                            });
                        }
                    }
                }


               


                var totalCount = dashboardList.Count;
                
                response.Data = dashboardList;
             
                response.Message = "Dashboard fetched successfully.";
                response.HttpStatusCode = System.Net.HttpStatusCode.OK;
                response.Success = true;
                response.ResponseCode = 0;

            }
            catch (Exception ex)
            {

                response.Message = "Exception occurred: " + ex.Message;
                response.Success = false;
                response.HttpStatusCode = System.Net.HttpStatusCode.InternalServerError;
                response.ResponseCode = 1;
            }
            return response;
        }



        public async Task<getDashboardResponse> GetCustomerDashboard(getDashboardRequest request)
        {
            var response = new getDashboardResponse();

            try
            {

                var userId = _acc.HttpContext?.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    response.Message = "Session expired or invalid user.";
                    response.Success = false;
                    response.HttpStatusCode = System.Net.HttpStatusCode.Unauthorized;
                    response.ResponseCode = 1;
                    return response;
                }


                var userFkRole = _user.Find(x => x.Id == userId).FirstOrDefault()?.Fk_RoleID;
                var userType = _role.Find(x => x.Id == userFkRole).FirstOrDefault()?.roleName;

                if (userType?.Trim().ToLower() != "customer")
                {
                    response.Message = "Invalid token or unauthorized user.";
                    response.Success = false;
                    response.HttpStatusCode = System.Net.HttpStatusCode.Unauthorized;
                    response.ResponseCode = 1;
                    return response;
                }


                var applications = _customer
                    .Find(x => x.IsDelete == false)
                    .ToList();

                var dashboardList = new List<CustomerDashboardData>();

                foreach (var app in applications)
                {
                    var certificates = _customercertificates
                        .Find(x => x.Fk_Customer_Application == app.Id && x.Is_Delete == false)
                        .ToList();

                    foreach (var cert in certificates)
                    {
                        var masterCert = _masterCertificate
                            .Find(x => x.Id == cert.Fk_Certificates)
                            .FirstOrDefault();

                        if (masterCert != null)
                        {
                            dashboardList.Add(new CustomerDashboardData
                            {
                                Id = app.Id,
                                ApplicationId = app.ApplicationId,
                                Certification_Name = masterCert.Certificate_Name,
                                 Status = app.status ?? "Pending",

                                ReceiveDate = app.CreatedAt
                            });
                        }
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


                var totalCount = dashboardList.Count;
                var skip = (request.PageNumber - 1) * request.PageSize;

                var paginatedList = dashboardList
                    .Skip(skip)
                    .Take(request.PageSize)
                    .ToList();
                response.Data = paginatedList;
                response.TotalRecords = totalCount;
                response.Message = "Dashboard fetched successfully.";
                response.HttpStatusCode = System.Net.HttpStatusCode.OK;
                response.Success = true;
                response.ResponseCode = 0;
            }
            catch (Exception ex)
            {

                response.Message = "Exception occurred: " + ex.Message;
                response.Success = false;
                response.HttpStatusCode = System.Net.HttpStatusCode.InternalServerError;
                response.ResponseCode = 1;
            }

            return response;
        }







        public addCustomerApplicationResponse AddCustomerApplication(addCustomerApplicationRequest request)
        {

            addCustomerApplicationResponse response = new addCustomerApplicationResponse();
            var UserId = _acc.HttpContext?.Session.GetString("UserId");
            var userFkRole = _user
                            .Find(x => x.Id == UserId)
                            .FirstOrDefault()?.Fk_RoleID;
            var usertype = _role
                            .Find(x => x.Id == userFkRole)
                            .FirstOrDefault()?.roleName;

            if (usertype?.Trim().ToLower() == "customer")
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(request.ApplicationId))
                    {
                        response.Message = "ApplicationId is required.";
                        response.Success = false;
                        response.HttpStatusCode = System.Net.HttpStatusCode.BadRequest;
                        return response;
                    }
                    var existingApplication = _customer
                        .Find(x => x.ApplicationId == request.ApplicationId)
                        .FirstOrDefault();

                    if (existingApplication != null)
                    {
                        if (existingApplication != null)
                        {
                            var updateDefinition = Builders<tbl_customer_application>.Update
                                .Set(x => x.Orgnization_Name, string.IsNullOrWhiteSpace(request.Orgnization_Name) ? existingApplication.Orgnization_Name : request.Orgnization_Name)
                                .Set(x => x.Constituation_of_Orgnization, string.IsNullOrWhiteSpace(request.Constituation_of_Orgnization) ? existingApplication.Constituation_of_Orgnization : request.Constituation_of_Orgnization)
                                .Set(x => x.EmailId, string.IsNullOrWhiteSpace(request.EmailId) ? existingApplication.EmailId : request.EmailId)
                                .Set(x => x.Expected_Audit_Date, request.Expected_Audit_Date ?? existingApplication.Expected_Audit_Date)
                                .Set(x => x.Holiday, string.IsNullOrWhiteSpace(request.Holiday) ? existingApplication.Holiday : request.Holiday)
                                .Set(x => x.Audit_Language, string.IsNullOrWhiteSpace(request.Audit_Language) ? existingApplication.Audit_Language : request.Audit_Language)
                                .Set(x => x.ConsultantName, string.IsNullOrWhiteSpace(request.ConsultantName) ? existingApplication.ConsultantName : request.ConsultantName)
                                .Set(x => x.FileName, string.IsNullOrWhiteSpace(request.FileName) ? existingApplication.FileName : request.FileName)
                                .Set(x => x.Name, string.IsNullOrWhiteSpace(request.Name) ? existingApplication.Name : request.Name)
                                .Set(x => x.Contact_details, string.IsNullOrWhiteSpace(request.Contact_details) ? existingApplication.Contact_details : request.Contact_details)
                                .Set(x => x.Datetime, request.Datetime ?? existingApplication.Datetime)
                                .Set(x => x.Designation, string.IsNullOrWhiteSpace(request.Designation) ? existingApplication.Designation : request.Designation)
                                .Set(x => x.Fk_Annaxture, string.IsNullOrWhiteSpace(request.Fk_Annaxture) ? existingApplication.Fk_Annaxture : request.Fk_Annaxture)
                                .Set(x => x.UpdatedAt, DateTime.Now)
                                .Set(x => x.UpdatedBy, UserId);

                            _customer.UpdateOne(x => x.Id == existingApplication.Id, updateDefinition);

                            if (request.Fk_ApplicationCertificates?.Any() == true)
                            {
                                foreach (var certName in request.Fk_ApplicationCertificates)
                                {
                                    var masterCert = _masterCertificate.Find(x => x.Certificate_Name.Trim() == certName.Trim()).FirstOrDefault();

                                    if (masterCert != null)
                                    {
                                        var existingCert = _customercertificates.Find(x =>
                                            x.Fk_Customer_Application == existingApplication.Id &&
                                            x.Fk_Certificates == masterCert.Id && x.Is_Delete == false &&
                                            x.CertificateType == "Regular").FirstOrDefault();

                                        if (existingCert != null)
                                        {
                                            var updateCert = Builders<tbl_customer_certificates>.Update
                                                .Set(x => x.UpdatedAt, DateTime.Now)
                                                .Set(x => x.UpdatedBy, UserId);
                                            _customercertificates.UpdateOne(x => x.Id == existingCert.Id, updateCert);
                                        }
                                        else
                                        {
                                            var customerCertificate = new tbl_customer_certificates
                                            {
                                                Fk_Customer_Application = existingApplication.Id,
                                                Fk_Certificates = masterCert.Id,
                                                CertificateType = "Regular",
                                                CreatedAt = DateTime.Now,
                                                UpdatedAt = DateTime.Now,
                                                CreatedBy = UserId,
                                                UpdatedBy = UserId,
                                                Is_Delete = false
                                            };
                                            _customercertificates.InsertOne(customerCertificate);
                                        }
                                    }
                                }
                            }

                            if (request.Fk_Product_Certificates?.Any() == true)
                            {
                                foreach (var productCertName in request.Fk_Product_Certificates)
                                {
                                    var masterCert = _masterCertificate.Find(x => x.Certificate_Name.Trim() == productCertName.Trim()).FirstOrDefault();

                                    if (masterCert != null)
                                    {
                                        var existingProductCert = _customercertificates.Find(x =>
                                            x.Fk_Customer_Application == existingApplication.Id &&
                                            x.Fk_Certificates == masterCert.Id && x.Is_Delete == false &&
                                            x.CertificateType == "Product").FirstOrDefault();

                                        if (existingProductCert != null)
                                        {
                                            var update = Builders<tbl_customer_certificates>.Update
                                                .Set(x => x.UpdatedAt, DateTime.Now)
                                                .Set(x => x.UpdatedBy, UserId);
                                            _customercertificates.UpdateOne(x => x.Id == existingProductCert.Id, update);
                                        }
                                        else
                                        {
                                            var newProductCert = new tbl_customer_certificates
                                            {
                                                Fk_Customer_Application = existingApplication.Id,
                                                Fk_Certificates = masterCert.Id,
                                                CertificateType = "Product",
                                                CreatedAt = DateTime.Now,
                                                UpdatedAt = DateTime.Now,
                                                CreatedBy = UserId,
                                                UpdatedBy = UserId,
                                                Is_Delete = false
                                            };
                                            _customercertificates.InsertOne(newProductCert);
                                        }
                                    }
                                }
                            }
                            if (request.Fk_Key_Personnels?.Any() == true)
                            {
                                foreach (var personnel in request.Fk_Key_Personnels)
                                {
                                    var existingPersonnel = _customerKeyPersonnel.Find(x =>
                                        x.Fk_Customer_Application == existingApplication.Id && x.Is_Delete == false &&
                                        x.Id == personnel.customerKeyPersonnelId).FirstOrDefault();

                                    if (existingPersonnel != null)
                                    {
                                        var update = Builders<tbl_customer_key_personnels>.Update
                                            .Set(x => x.FullName, string.IsNullOrWhiteSpace(personnel.Name) ? existingPersonnel.FullName : personnel.Name)
                                            .Set(x => x.Department, string.IsNullOrWhiteSpace(personnel.Designation) ? existingPersonnel.Department : personnel.Designation)
                                            .Set(x => x.Contact, string.IsNullOrWhiteSpace(personnel.Contact_No) ? existingPersonnel.Contact : personnel.Contact_No)
                                            .Set(x => x.TypeOfPersonnel, string.IsNullOrWhiteSpace(personnel.Type) ? existingPersonnel.TypeOfPersonnel : personnel.Type)
                                            .Set(x => x.UpdatedAt, DateTime.Now)
                                            .Set(x => x.UpdatedBy, UserId);
                                        _customerKeyPersonnel.UpdateOne(x => x.Id == existingPersonnel.Id, update);
                                    }
                                    else
                                    {
                                        var newPersonnel = new tbl_customer_key_personnels
                                        {
                                            Fk_Customer_Application = existingApplication.Id,
                                            FullName = personnel.Name,
                                            Department = personnel.Designation,
                                            EmailId = personnel.EmailId,
                                            Contact = personnel.Contact_No,
                                            TypeOfPersonnel = personnel.Type,
                                            CreatedAt = DateTime.Now,
                                            UpdatedAt = DateTime.Now,
                                            CreatedBy = UserId,
                                            UpdatedBy = UserId,
                                            Is_Delete = false
                                        };
                                        _customerKeyPersonnel.InsertOne(newPersonnel);
                                    }
                                }
                            }

                            if (request.Fk_Customer_Sites?.Any() == true)
                            {
                                foreach (var site in request.Fk_Customer_Sites)
                                {
                                    var existingSite = _customersite.Find(x =>
                                        x.Fk_Customer_Application == existingApplication.Id &&
                                        x.Id == site.customer_SiteId &&
                                        x.Is_Delete == false).FirstOrDefault();

                                    if (existingSite != null)
                                    {
                                        var update = Builders<tbl_customer_site>.Update
                                            .Set(x => x.Telephone, string.IsNullOrWhiteSpace(site.Telephone) ? existingSite.Telephone : site.Telephone)
                                            .Set(x => x.Web, string.IsNullOrWhiteSpace(site.Web) ? existingSite.Web : site.Web)
                                            .Set(x => x.Activity_Department, string.IsNullOrWhiteSpace(site.Activity_Department) ? existingSite.Activity_Department : site.Activity_Department)
                                            .Set(x => x.Manpower, string.IsNullOrWhiteSpace(site.Manpower) ? existingSite.Manpower : site.Manpower)
                                            .Set(x => x.Shift_Name, string.IsNullOrWhiteSpace(site.Shift_Name) ? existingSite.Shift_Name : site.Shift_Name)
                                            .Set(x => x.UpdatedAt, DateTime.Now)
                                            .Set(x => x.UpdatedBy, UserId);
                                        _customersite.UpdateOne(x => x.Id == existingSite.Id, update);
                                    }
                                    else
                                    {
                                        var newSite = new tbl_customer_site
                                        {
                                            Fk_Customer_Application = existingApplication.Id,
                                            Address = site.Address,
                                            Telephone = site.Telephone,
                                            Web = site.Web,
                                            Email = site.Email,
                                            Activity_Department = site.Activity_Department,
                                            Manpower = site.Manpower,
                                            Shift_Name = site.Shift_Name,
                                            CreatedAt = DateTime.Now,
                                            UpdatedAt = DateTime.Now,
                                            CreatedBy = UserId,
                                            UpdatedBy = UserId,
                                            Is_Delete = false
                                        };
                                        _customersite.InsertOne(newSite);
                                    }
                                }
                            }
                            if (request.Fk_Customer_Entity?.Any() == true)
                            {
                                foreach (var entity in request.Fk_Customer_Entity)
                                {
                                    var existingEntity = _customerentity.Find(x =>
                                        x.Fk_Customer_Application == existingApplication.Id &&
                                        x.Id == entity.customer_EntityId).FirstOrDefault();

                                    if (existingEntity != null)
                                    {
                                        var update = Builders<tbl_customer_Entity>.Update
                                            .Set(x => x.Identification_Number, string.IsNullOrWhiteSpace(entity.Identification_Number) ? existingEntity.Identification_Number : entity.Identification_Number)
                                            .Set(x => x.file, string.IsNullOrWhiteSpace(entity.file) ? existingEntity.file : entity.file)
                                            .Set(x => x.UpdatedAt, DateTime.Now)
                                            .Set(x => x.UpdatedBy, UserId);
                                        _customerentity.UpdateOne(x => x.Id == existingEntity.Id, update);
                                    }
                                    else
                                    {
                                        var newEntity = new tbl_customer_Entity
                                        {
                                            Fk_Customer_Application = existingApplication.Id,
                                            Entity_Name = entity.Name_of_Entity,
                                            Identification_Number = entity.Identification_Number,
                                            file = entity.file,
                                            CreatedAt = DateTime.Now,
                                            UpdatedAt = DateTime.Now,
                                            CreatedBy = UserId,
                                            UpdatedBy = UserId,
                                            Is_Delete = false
                                        };
                                        _customerentity.InsertOne(newEntity);
                                    }
                                }
                            }




                            response.Message = "Customer Application Updated Successfully.";
                            response.HttpStatusCode = System.Net.HttpStatusCode.OK;
                            response.Success = true;
                            response.ResponseCode = 0;

                            return response;
                        }

                    }
                    bool? isFinalSubmit = null;
                    DateTime? submitDate = null;

                    // If user provided isFinalSubmit flag
                    if (!string.IsNullOrWhiteSpace(request.isFinalSubmit))
                    {
                        isFinalSubmit = request.isFinalSubmit.Trim().ToLower() == "true";

                        if (isFinalSubmit == true)
                        {
                            submitDate = DateTime.Now;

                        }
                    }
                    var customerApplication = new tbl_customer_application
                    {
                        ApplicationId = request.ApplicationId,
                        Orgnization_Name = request.Orgnization_Name,
                        Constituation_of_Orgnization = request.Constituation_of_Orgnization,
                        EmailId = request.EmailId,
                        Expected_Audit_Date = request.Expected_Audit_Date,
                        Holiday = request.Holiday,
                        Audit_Language = request.Audit_Language,
                        ConsultantName = request.ConsultantName,
                        FileName = request.FileName,
                        Name = request.Name,
                        Contact_details = request.Contact_details,
                        Datetime = request.Datetime,
                        Designation = request.Designation,
                        Fk_Annaxture = request.Fk_Annaxture,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        CreatedBy = UserId,
                        UpdatedBy = UserId,
                        IsDelete = false,
                        IsFinalSubmit = isFinalSubmit,
                        SubmitDate = submitDate,
                        status = "687a2925694d00158c9bf265"
                    };
                    _customer.InsertOne(customerApplication);





                    if (request.Fk_ApplicationCertificates?.Any() == true)
                    {
                        foreach (var certificateId in request.Fk_ApplicationCertificates)
                        {
                            var findedCertificate = _masterCertificate
                                     .Find(x => x.Certificate_Name.Trim() == certificateId.Trim())
                                     .FirstOrDefault();

                            if (findedCertificate != null)
                            {
                                var customerCertificate = new tbl_customer_certificates
                                {
                                    Fk_Customer_Application = customerApplication.Id,
                                    Fk_Certificates = findedCertificate.Id,
                                    Is_Delete = false,
                                    CertificateType = "Regular",
                                    CreatedAt = DateTime.Now,
                                    UpdatedAt = DateTime.Now,
                                    CreatedBy = UserId,
                                    UpdatedBy = UserId
                                };
                                _customercertificates.InsertOne(customerCertificate);
                            }

                        }
                    }

                    if (request.Fk_Product_Certificates?.Any() == true)
                    {
                        foreach (var productCertificateId in request.Fk_Product_Certificates)
                        {
                            var findedProductCertificateId = _masterCertificate
                                .Find(x => x.Certificate_Name.Trim() == productCertificateId.Trim())
                                .FirstOrDefault()?.Id;
                            var customerProductCertificate = new tbl_customer_certificates
                            {
                                Fk_Customer_Application = customerApplication.Id,
                                Fk_Certificates = findedProductCertificateId,
                                Is_Delete = false,
                                CertificateType = "Product",
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now,
                                CreatedBy = UserId,
                                UpdatedBy = UserId
                            };
                            _customercertificates.InsertOne(customerProductCertificate);
                        }
                    }
                    if (request.Fk_Key_Personnels?.Any() == true)
                    {
                        foreach (var keyPersonnel in request.Fk_Key_Personnels)
                        {
                            var customerKeyPersonnel = new tbl_customer_key_personnels
                            {
                                Fk_Customer_Application = customerApplication.Id,
                                FullName = keyPersonnel.Name,
                                Department = keyPersonnel.Designation,
                                EmailId = keyPersonnel.EmailId,
                                Contact = keyPersonnel.Contact_No,
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now,
                                CreatedBy = UserId,
                                UpdatedBy = UserId,
                                TypeOfPersonnel = keyPersonnel.Type,
                                Is_Delete = false

                            };
                            _customerKeyPersonnel.InsertOne(customerKeyPersonnel);
                        }
                    }
                    if (request.Fk_Customer_Sites?.Any() == true)
                    {
                        foreach (var siteDetails in request.Fk_Customer_Sites)
                        {
                            var customer_Site = new tbl_customer_site
                            {
                                Fk_Customer_Application = customerApplication.Id,
                                Address = siteDetails.Address,
                                Telephone = siteDetails.Telephone,
                                Web = siteDetails.Web,
                                Email = siteDetails.Email,
                                Activity_Department = siteDetails.Activity_Department,
                                Manpower = siteDetails.Manpower,
                                Shift_Name = siteDetails.Shift_Name,
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now,
                                CreatedBy = UserId,
                                UpdatedBy = UserId,
                                Is_Delete = false
                            };
                            _customersite.InsertOne(customer_Site);
                        }
                    }
                    if (request.Fk_Customer_Entity?.Any() == true)
                    {
                        foreach (var entityList in request.Fk_Customer_Entity)
                        {
                            var customer_Entity = new tbl_customer_Entity
                            {
                                Fk_Customer_Application = customerApplication.Id,
                                Entity_Name = entityList.Name_of_Entity,
                                Identification_Number = entityList.Identification_Number,
                                file = entityList.file,
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now,
                                CreatedBy = UserId,
                                UpdatedBy = UserId,
                                Is_Delete = false
                            };
                            _customerentity.InsertOne(customer_Entity);
                        }
                    }

                    if (customerApplication.Id != null)
                    {
                       
                        var updateDefinition = Builders<tbl_customer_application>.Update
                            .Set(x => x.OutsourceProcess, request.OutsourceProcess?.Select(p => new OutsourceProcessItem
                            {
                                Process = p.Process,
                                Sub_contractorName = p.Sub_contractorName,
                                Location = p.Location,
                                controll_establish_level = p.controll_establish_level
                            }).ToList())
                            .Set(x => x.UpdatedAt, DateTime.Now)
                            .Set(x => x.UpdatedBy, UserId);

                        _customer.UpdateOne(x => x.Id == customerApplication.Id, updateDefinition);

                    }



                    response.Message = "Customer Application Submited Successfully.";
                    response.HttpStatusCode = System.Net.HttpStatusCode.OK;
                    response.Success = true;
                    response.ResponseCode = 0;

                }
                catch (Exception ex)
                {
                    response.Message = "AddCustomerApplication Exception: " + ex.Message;
                    response.Success = false;
                    response.HttpStatusCode = System.Net.HttpStatusCode.InternalServerError;
                    response.ResponseCode = 1;
                }
            }
            else
            {
                response.Message = "Invalid Token. ";
                response.Success = false;
                response.HttpStatusCode = System.Net.HttpStatusCode.OK;
                response.ResponseCode = 1;
            }


            return response; 
        }

        //public async Task<getDashboardResponse> GetCustomerDashboard(getDashboardRequest request)
        //{
        //    getDashboardResponse response = new getDashboardResponse();
        //    var UserId = _acc.HttpContext?.Session.GetString("UserId");
        //    var userFkRole = _user
        //                    .Find(x => x.Id == UserId)
        //                    .FirstOrDefault()?.Fk_RoleID;
        //    var usertype = _role
        //                    .Find(x => x.Id == userFkRole)
        //                    .FirstOrDefault()?.roleName;

        //    if (usertype?.Trim().ToLower() == "customer")
        //    {
        //        try
        //        {
        //            List<CustomerDashboardData> dashboardList = new List<CustomerDashboardData>();

        //            var applications = _customer
        //               .Find(x => x.IsDelete == false)
        //               .ToList();

        //            foreach (var app in applications)
        //            {
        //                var certificates = _customercertificates
        //                    .Find(x => x.Fk_Customer_Application == app.Id && x.Is_Delete == false)
        //                    .ToList();

        //                foreach (var cert in certificates)
        //                {
        //                    var masterCert = _masterCertificate.Find(x => x.Id == cert.Fk_Certificates).FirstOrDefault();
        //                    if (masterCert != null)
        //                    {
        //                        var dashboardRecord = new CustomerDashboardData
        //                        {
        //                            Id = app.Id,
        //                            ApplicationId = app.ApplicationId,
        //                            Certification_Name = masterCert.Certificate_Name,
        //                            Status = app.status,

        //                        };

        //                        dashboardList.Add(dashboardRecord);
        //                    }
        //                }
        //                var pendingDocs = new List<pendingDocumentList>
        //                {
        //                    new pendingDocumentList
        //                    {
        //                        DocumentName = "Document 1",
        //                        ApplicationName = "Iso Application",
        //                        DocumentCreatedDate = DateTime.Now
        //                    },
        //                    new pendingDocumentList
        //                    {
        //                        DocumentName = "Process Flow",
        //                        ApplicationName = "ICMED",
        //                        DocumentCreatedDate = DateTime.Now
        //                    },
        //                    new pendingDocumentList
        //                    {
        //                        DocumentName = "Personnel",
        //                        ApplicationName = "FSSC",
        //                        DocumentCreatedDate = DateTime.Now
        //                    },
        //                    new pendingDocumentList
        //                    {
        //                        DocumentName = "Site Add Wrong",
        //                        ApplicationName = "ISO",
        //                        DocumentCreatedDate = DateTime.Now
        //                    }
        //                };
        //            }

        //            response.Data = dashboardList;
        //            response.Message = "Dashboard fetched successfully.";
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



        public getCustomerApplicationResponse GetCustomerApplication(getCustomerApplicationRequest request)
        {
            getCustomerApplicationResponse response = new getCustomerApplicationResponse();
            try
            {
                var UserId = _acc.HttpContext?.Session.GetString("UserId");
                var userFkRole = _user.Find(x => x.Id == UserId).FirstOrDefault()?.Fk_RoleID;
                var userType = _role.Find(x => x.Id == userFkRole).FirstOrDefault()?.roleName;

                if (userType?.Trim().ToLower() == "customer")
                {
                    if (string.IsNullOrWhiteSpace(request.applicationId))
                    {
                        response.Message = "ApplicationId is required.";
                        response.HttpStatusCode = System.Net.HttpStatusCode.BadRequest;
                        response.Success = false;
                        response.ResponseCode = 1;
                        return response;
                    }

                    var app = _customer
                        .Find(x => x.Id == request.applicationId && x.IsDelete == false)
                        .FirstOrDefault();

                    if (app == null)
                    {
                        response.Message = "No data found for the given ApplicationId.";
                        response.HttpStatusCode = System.Net.HttpStatusCode.NotFound;
                        response.Success = false;
                        response.ResponseCode = 1;
                        return response;
                    }

                    var result = new CustomerApplicationData
                    {
                        Id = app.Id,
                        ApplicationId = app.ApplicationId,
                        Orgnization_Name = app.Orgnization_Name,
                        Constituation_of_Orgnization = app.Constituation_of_Orgnization,
                        EmailId = app.EmailId,
                        Expected_Audit_Date = app.Expected_Audit_Date,
                        Holiday = app.Holiday,
                        Audit_Language = app.Audit_Language,
                        ConsultantName = app.ConsultantName,
                        FileName = app.FileName,
                        Name = app.Name,
                        Contact_details = app.Contact_details,
                        Datetime = app.Datetime,
                        Designation = app.Designation,
                        Fk_Annaxture = app.Fk_Annaxture,

                        ApplicationCertificates = _customercertificates
                            .Find(x => x.Fk_Customer_Application == app.Id && x.CertificateType == "Regular" && x.Is_Delete == false)
                            .ToList()
                            .Select(cert => _masterCertificate.Find(mc => mc.Id == cert.Fk_Certificates).FirstOrDefault()?.Certificate_Name)
                            .Where(name => !string.IsNullOrEmpty(name))
                            .ToList(),

                        ProductCertificates = _customercertificates
                            .Find(x => x.Fk_Customer_Application == app.Id && x.CertificateType == "Product" && x.Is_Delete == false)
                            .ToList()
                            .Select(cert => _masterCertificate.Find(mc => mc.Id == cert.Fk_Certificates).FirstOrDefault()?.Certificate_Name)
                            .Where(name => !string.IsNullOrEmpty(name))
                            .ToList(),

                        KeyPersonnels = _customerKeyPersonnel
                            .Find(x => x.Fk_Customer_Application == app.Id && x.Is_Delete == false)
                            .ToList()
                            .Select(kp => new KeyPersonnelList
                            {
                                customerKeyPersonnelId = kp.Id,
                                Name = kp.FullName,
                                Designation = kp.Department,
                                EmailId = kp.EmailId,
                                Contact_No = kp.Contact,
                                Type = kp.TypeOfPersonnel
                            }).ToList(),

                        CustomerSites = _customersite
                            .Find(x => x.Fk_Customer_Application == app.Id && x.Is_Delete == false)
                            .ToList()
                            .Select(site => new CustomerSiteList
                            {
                                customer_SiteId = site.Id,
                                Address = site.Address,
                                Telephone = site.Telephone,
                                Web = site.Web,
                                Email = site.Email,
                                Activity_Department = site.Activity_Department,
                                Manpower = site.Manpower,
                                Shift_Name = site.Shift_Name
                            }).ToList(),

                        CustomerEntities = _customerentity
                            .Find(x => x.Fk_Customer_Application == app.Id && x.Is_Delete == false)
                            .ToList()
                            .Select(entity => new CustomerEntityList
                            {
                                customer_EntityId = entity.Id,
                                Name_of_Entity = entity.Entity_Name,
                                Identification_Number = entity.Identification_Number,
                                file = entity.file
                            }).ToList()
                    };

                    response.Data = new List<CustomerApplicationData> { result };
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



        public async Task<addCustomerApplicationResponse> CreateCustomerApplication(getDashboardRequest request)
        {
            var response = new addCustomerApplicationResponse();
            var UserId = _acc.HttpContext?.Session.GetString("UserId");
            var userFkRole = _user
                .Find(x => x.Id == UserId)
                .FirstOrDefault()?.Fk_RoleID;

            var usertype = _role
                .Find(x => x.Id == userFkRole)
                .FirstOrDefault()?.roleName;

            if (usertype?.Trim().ToLower() == "customer")
            {
                try
                {
                    var nextAppId = await GenerateNextApplicationId();

                    var customerApplication = new tbl_customer_application
                    {
                        ApplicationId = nextAppId,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        CreatedBy = UserId,
                        UpdatedBy = UserId,
                        IsDelete = false,
                        IsFinalSubmit = false // Mark as draft or not submitted
                    };

                    await _customer.InsertOneAsync(customerApplication);
                    response.Data = customerApplication;
                    response.Message = "Customer Application Submitted Successfully.";
                    response.HttpStatusCode = System.Net.HttpStatusCode.OK;
                    response.Success = true;
                    response.ResponseCode = 0;
                }
                catch (Exception ex)
                {
                    response.Message = "AddCustomerApplication Exception: " + ex.Message;
                    response.Success = false;
                    response.HttpStatusCode = System.Net.HttpStatusCode.InternalServerError;
                    response.ResponseCode = 1;
                }
            }
            else
            {
                response.Message = "Invalid Token.";
                response.Success = false;
                response.HttpStatusCode = System.Net.HttpStatusCode.OK;
                response.ResponseCode = 1;
            }

            return response;
        }


        public async Task<string> GenerateNextApplicationId()
        {
            var currentYear = DateTime.Now.Year.ToString();

            var lastApplication = await _customer
                .Find(x => x.IsDelete == false && x.ApplicationId.StartsWith($"ZQAPL-{currentYear}"))
                .SortByDescending(x => x.ApplicationId)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (lastApplication != null)
            {
                var lastAppId = lastApplication.ApplicationId; // Example: ZAQPL-2025-005
                var parts = lastAppId.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            var nextApplicationId = $"ZQAPL-{currentYear}-{nextNumber.ToString("D3")}";
            return nextApplicationId;
        }

        public async Task<userDropdownResponse> GetAllDropdown(userDropdownRequest request)
        {
            var response = new userDropdownResponse();

            try
            {
                var userId = _acc.HttpContext?.Session.GetString("UserId");
                var userFkRole = (await _user.Find(x => x.Id == userId).FirstOrDefaultAsync())?.Fk_RoleID;
                var usertype = (await _role.Find(x => x.Id == userFkRole).FirstOrDefaultAsync())?.roleName;

                if (!string.IsNullOrWhiteSpace(request.type))
                {
                    if (request.type.Trim().ToLower() == "userlist")
                    {
                        var userList = await _user
                            .Find(x => x.IsDelete == 0 && x.Fk_RoleID=="686fc4eff41f7edee9b89cd3")
                            .Project(x => new UserDropdownDto
                            {
                                Id = x.Id,
                                Name = x.FullName
                            }).ToListAsync();

                        response.Data = userList;
                    }
                    else if (request.type.Trim().ToLower() == "regular")
                    {
                        var products = await _mastercertificate
                            .Find(x => x.CertificationType != null && x.CertificationType.ToLower() == "regular")
                            .Project(x => new UserDropdownDto
                            {
                                Id = x.Id,
                                Name = x.Certificate_Name
                            }).ToListAsync();

                        response.Data = products;
                    }
                    else if (request.type.Trim().ToLower() == "product")
                    {
                        var products = await _mastercertificate
                            .Find(x => x.CertificationType.ToLower() == "product")
                            .Project(x => new UserDropdownDto
                            {
                                Id = x.Id,
                                Name = x.Certificate_Name
                            }).ToListAsync();

                        response.Data = products;
                    }
                    else
                    {
                        response.Data = new List<UserDropdownDto>();
                    }
                }
                

                response.Message = "Data fetched successfully.";
                response.Success = true;
                response.HttpStatusCode = System.Net.HttpStatusCode.OK;
                response.ResponseCode = 0;
            }
            catch (Exception ex)
            {
                response.Message = "GetDropdown Exception: " + ex.Message;
                response.Success = false;
                response.HttpStatusCode = System.Net.HttpStatusCode.InternalServerError;
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
