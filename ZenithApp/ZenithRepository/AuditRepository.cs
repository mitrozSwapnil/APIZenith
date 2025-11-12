using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Net;
using ThirdParty.Json.LitJson;
using ZenithApp.CommonServices;
using ZenithApp.Settings;
using ZenithApp.ZenithEntities;
using ZenithApp.ZenithMessage;

namespace ZenithApp.ZenithRepository
{
    public class AuditRepository : BaseRepository
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
        private readonly IMongoCollection<tbl_quoatation> _quotation;
        private readonly MongoDbService _mongoDbService;
        private readonly IMongoCollection<tbl_ApplicationReview> _reviews;
        private readonly IMongoCollection<tbl_ApplicationFieldHistory> _history;
        private readonly IMongoCollection<tbl_ApplicationFieldComment> _comments;
        private readonly IMongoCollection<tbl_Audit> _audit;
        private readonly IMongoCollection<tbl_dynamic_audit_template> _dynamic;
        private readonly IMongoCollection<tbl_competencyProfile> _competency;
        private readonly IMongoCollection<tbl_AssignAudit> _assignaudit;
        private readonly IMongoCollection<tbl_master_technicalArea> _tech;
        private readonly IMongoCollection<tbl_audit_administration> _auditAdministration;
        private readonly IMongoCollection<tbl_audit_administration_technical> _auditAdministrationTechnical;
        private readonly IMongoCollection<tbl_AuditProcess> _auditprocess;
        private readonly IMongoCollection<tbl_RecentUpdate> _recentupdate;

        private readonly IHttpContextAccessor _acc;

        public AuditRepository(IOptions<MongoDbSettings> settings, IHttpContextAccessor acc, MongoDbService mongoDbService)
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
            _audit = database.GetCollection<tbl_Audit>("tbl_Audit");
            _mongoDbService = mongoDbService;
            _acc = acc;
            _reviews = database.GetCollection<tbl_ApplicationReview>("tbl_ApplicationReview");
            _history = database.GetCollection<tbl_ApplicationFieldHistory>("tbl_ApplicationFieldHistory");
            _comments = database.GetCollection<tbl_ApplicationFieldComment>("tbl_ApplicationFieldComment");
            _quotation = database.GetCollection<tbl_quoatation>("tbl_quoatation");
            _dynamic = database.GetCollection<tbl_dynamic_audit_template>("tbl_dynamic_audit_template");
            _competency = database.GetCollection<tbl_competencyProfile>("tbl_competencyProfile");
            _assignaudit = database.GetCollection<tbl_AssignAudit>("tbl_AssignAudit");
            _tech = database.GetCollection<tbl_master_technicalArea>("tbl_master_technicalArea");
            _auditAdministration = database.GetCollection<tbl_audit_administration>("tbl_audit_administration");
            _auditAdministrationTechnical = database.GetCollection<tbl_audit_administration_technical>("tbl_audit_administration_technical");
            _auditprocess = database.GetCollection<tbl_AuditProcess>("tbl_AuditProcess");
            _recentupdate = database.GetCollection<tbl_RecentUpdate>("tbl_RecentUpdate");
        }




        public async Task<getAuditResponse> GetAuditDashboard(getDashboardRequest request)
        {
            getAuditResponse response = new getAuditResponse();
            var UserId = _acc.HttpContext?.Session.GetString("UserId");

            var userRecord = _user.Find(x => x.Id == UserId).FirstOrDefault();
            var departmentList = userRecord?.Department; // List<string>
            var userFkRole = userRecord?.Fk_RoleID;
            var usertype = _role.Find(x => x.Id == userFkRole).FirstOrDefault()?.roleName;

            if (usertype?.Trim().ToLower() == "Admin")
            {
                response.Message = "Invalid Token.";
                response.Success = false;
                response.HttpStatusCode = System.Net.HttpStatusCode.Unauthorized;
                response.ResponseCode = 1;
                return response;
            }

            try
            {
                List<AuditDashboardData> dashboardList = new List<AuditDashboardData>();

                // Pagination
                var auditapplications = _audit.Find(Builders<tbl_Audit>.Filter.Empty).ToList();

                foreach (var app in auditapplications)
                {
                    var certificate = _masterCertificate
                       .Find(x => x.Id == app.Fk_CertificateId)
                       .FirstOrDefault();
                    var IsoData = _customer.Find(x => x.Id == app.ApplicationId).FirstOrDefault();
                    var ApplicatioName = IsoData.ApplicationName;
                    var company = IsoData.Orgnization_Name;

                    //string ApplicatioName = "";
                    //string company = "";
                    //if (certificate.Certificate_Name == "ISO")
                    //{
                    //    var IsoData = _customer.Find(x => x.Id == app.ApplicationId).FirstOrDefault();
                    //    ApplicatioName = IsoData.ApplicationName;
                    //    company = IsoData.Orgnization_Name;


                    //}
                    //else if (certificate.Certificate_Name == "FSSC")
                    //{
                    //    var IsoData = _fssc.Find(x => x.Id == app.Fk_sub_applicationId).FirstOrDefault();
                    //    ApplicatioName = IsoData.ApplicationName;
                    //    company = IsoData.Orgnization_Name;
                    //}
                    //else if (certificate.Certificate_Name == "Imdr")
                    //{
                    //    var IsoData = _imdr.Find(x => x.Id == app.Fk_sub_applicationId).FirstOrDefault();
                    //    ApplicatioName = IsoData.ApplicationName;
                    //    company = IsoData.Orgnization_Name;
                    //}
                    //else if (certificate.Certificate_Name == "ICMED")
                    //{
                    //    var IsoData = _icmed.Find(x => x.Id == app.Fk_sub_applicationId).FirstOrDefault();
                    //    ApplicatioName = IsoData.ApplicationName;
                    //    company = IsoData.Orgnization_Name;
                    //}
                    //else if (certificate.Certificate_Name == "ICMEDPLUS")
                    //{
                    //    var IsoData = _icmedplus.Find(x => x.Id == app.Fk_sub_applicationId).FirstOrDefault();
                    //    ApplicatioName = IsoData.ApplicationId;
                    //    company = IsoData.Orgnization_Name;
                    //}
                    //else
                    //{
                    //    throw new Exception("No document found for given ApplicationId and CertificateTypeId");
                    //}
                    

                    dashboardList.Add(new AuditDashboardData
                    {
                        Id = app.Id,
                        ReceiveDate = app.Received_Date,
                        FileNumber = app.FileNumber,
                        QuotationId = app.Fk_quotation,
                        QuotationNumber = _quotation.Find(x => x.Id == app.Fk_quotation).FirstOrDefault().QuotationId,
                        sub_applicationId = app.Fk_sub_applicationId,
                        ApplicationName = ApplicatioName,
                        ApplicationId = app.ApplicationId,
                        CompanyName = company,
                        Certification_Name = certificate.Certificate_Name,
                        Certification_Id = app.Fk_CertificateId,
                        IsAuditNominationDone = app.IsAuditNominationDone,
                        status = _status.Find(x => x.Id == app.Fk_status).FirstOrDefault()?.StatusName

                    });

                }


                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    var searchTerm = request.SearchTerm.Trim().ToLower();
                    dashboardList = dashboardList
                        .Where(x =>
                            (!string.IsNullOrEmpty(x.ApplicationName) && x.ApplicationName.ToLower().Contains(searchTerm)) ||
                            (!string.IsNullOrEmpty(x.Certification_Name) && x.Certification_Name.ToLower().Contains(searchTerm)) ||
                            (!string.IsNullOrEmpty(x.FileNumber) && x.FileNumber.ToLower().Contains(searchTerm))
                        )
                        .ToList();
                }

                var totalCount = dashboardList.Count;
                var skip = (request.PageNumber - 1) * request.PageSize;
                var pagination = new PageinationDto
                {
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalRecords = totalCount
                };
                var paginatedList = dashboardList
                    .Skip(skip)
                    .Take(request.PageSize)
                    .ToList();

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




        public async Task<BaseResponse> CreateAudit(AuditRequest request)
        {
            var response = new BaseResponse();

            try
            {
                if (string.IsNullOrEmpty(request.sub_applicationId) || string.IsNullOrEmpty(request.fileNumber))
                {
                    response.Message = "ApplicationId or FileNumber is missing.";
                    response.HttpStatusCode = HttpStatusCode.BadRequest;
                    response.Success = false;
                    return response;
                }
                var UserId = _acc.HttpContext?.Session.GetString("UserId");
                
                var audit = new tbl_Audit
                {
                    ApplicationId = request.ApplicationId ?? "",
                    Fk_quotation = request.quotationId,
                    Fk_sub_applicationId = request.sub_applicationId,
                    FileNumber = request.fileNumber,
                    Fk_CertificateId = request.certificateid,
                    Fk_status = null,
                    CreatedAt = DateTime.UtcNow,
                    Received_Date = DateTime.UtcNow,
                    CreatedBy = UserId, // from BaseRequest if available
                    isDelete = false,
                    
                };

                await _audit.InsertOneAsync(audit);

                await _quotation.UpdateOneAsync(
                    x => x.sub_applicationId == request.sub_applicationId && x.Id == request.quotationId,
                    Builders<tbl_quoatation>.Update
                        .Set(x => x.IsAuditAssign, true)
                        .Set(x => x.AuditFileNumber, request.fileNumber)
                );



                response.Message = "Audit File Saved successfully.";
                response.HttpStatusCode = HttpStatusCode.OK;
                response.Success = true;
                response.ResponseCode = 0;
            }
            catch (Exception ex)
            {
                response.Message = "CreateAudit Exception: " + ex.Message;
                response.HttpStatusCode = HttpStatusCode.InternalServerError;
                response.Success = false;
            }

            return response;
        }

        public async Task<AuditResponse> GenerateFileNumber(BaseRequest request)
        {
            var response = new AuditResponse();

            try
            {
                string prefix = "QMS/91/A/I/";
                int nextNumber = 1;

                // Find last record
                var lastRecord = await _audit
                    .Find(FilterDefinition<tbl_Audit>.Empty)
                    .SortByDescending(x => x.FileNumber)
                    .FirstOrDefaultAsync();

                if (lastRecord != null && !string.IsNullOrEmpty(lastRecord.FileNumber))
                {
                    // Split file number and extract last numeric part
                    var parts = lastRecord.FileNumber.Split('/');
                    if (parts.Length > 4 && int.TryParse(parts[4], out int lastNumber))
                    {
                        nextNumber = lastNumber + 1;
                    }
                }

                // Generate new file number (QMS/91/A/I/0002)
                string newFileNumber = $"{prefix}{nextNumber.ToString("D4")}";
                response.FileNumber = newFileNumber;
                response.Message = "Audit File Saved successfully.";
                response.HttpStatusCode = HttpStatusCode.OK;
                response.Success = true;
                response.ResponseCode = 0;
            }
            catch (Exception ex)
            {
                response.Message = "CreateAudit Exception: " + ex.Message;
                response.HttpStatusCode = HttpStatusCode.InternalServerError;
                response.Success = false;
            }

            return response;
        }


        public async Task<BaseResponse> CreateDynamicAuditForm(formRequest request)
        {
            var response = new BaseResponse();

            try
            {
                if (string.IsNullOrEmpty(request.Fk_certificateId) || string.IsNullOrEmpty(request.AuditStage))
                {
                    response.Message = "CertificateId or Stage is missing.";
                    response.HttpStatusCode = HttpStatusCode.BadRequest;
                    response.Success = false;
                    return response;
                }

                var userId = _acc.HttpContext?.Session.GetString("UserId");
                var user = await _user.Find(x => x.Id == userId).FirstOrDefaultAsync();
                var name = user?.FullName;


                // --- Check if document for this certificate already exists ---
                var existingDoc = await _dynamic
                    .Find(x => x.Fk_certificateId == request.Fk_certificateId && !x.IsDelete)
                    .FirstOrDefaultAsync();

                // Prepare new steps to insert
                List<ZenithEntities.StepTemplateModel> newSteps = new List<ZenithEntities.StepTemplateModel>();

                // If Step_Template list is passed
                if (request.Step_Template != null && request.Step_Template.Any())
                {
                    newSteps.AddRange(
                        request.Step_Template.Select(x => new ZenithEntities.StepTemplateModel
                        {
                            id = ObjectId.GenerateNewId().ToString(),
                            StepNumber = Convert.ToInt32(request.stepNumber),
                            StepName = request.StepName,
                            AttachmentType = request.AttcahmentType,
                            file=request.file ?? "",
                            FormName=request.formName,
                            UpdatedAt=DateTime.UtcNow,
                            UpdatedBy = name,
                            isApproval=request.isApproval,
                            Status ="Pending"

                        })
                    );
                }
                else
                {
                    // fallback for single step case
                    newSteps.Add(new ZenithEntities.StepTemplateModel
                    {
                        id = ObjectId.GenerateNewId().ToString(),
                        StepNumber = Convert.ToInt32(request.stepNumber),
                        StepName = request.StepName,
                        AttachmentType = request.AttcahmentType,
                        FormName = request.formName,
                        file= request.file,
                        UpdatedAt = DateTime.UtcNow,
                        UpdatedBy = name,
                        isApproval = request.isApproval,
                        Status ="Pending"
                    });
                }

                if (existingDoc == null)
                {
                    // --- Create new document for certificate ---
                    var newDoc = new tbl_dynamic_audit_template
                    {
                        Fk_certificateId = request.Fk_certificateId,
                        CreatedBy = userId,
                        UpdatedBy = userId,
                        isApproval =request.isApproval,
                        AttachmentType =request.AttcahmentType,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        status ="Pending",
                        Stages = new List<StageTemplateModel>
                        {
                            new StageTemplateModel
                            {
                                AuditStage = request.AuditStage,
                                Steps = newSteps
                            }
                        }
                    };

                    await _dynamic.InsertOneAsync(newDoc);
                }
                else
                {
                    // --- Update existing certificate document ---
                    if (existingDoc.Stages == null)
                        existingDoc.Stages = new List<StageTemplateModel>();

                    // --- Find the stage ---
                    var stage = existingDoc.Stages.FirstOrDefault(s => s.AuditStage == request.AuditStage);

                    if (stage == null)
                    {
                        // Add new stage
                        existingDoc.Stages.Add(new StageTemplateModel
                        {
                            AuditStage = request.AuditStage,
                            Steps = newSteps
                        });
                    }
                    else
                    {
                        // Append new steps into same stage
                        if (stage.Steps == null)
                            stage.Steps = new List<ZenithEntities.StepTemplateModel>();

                        stage.Steps.AddRange(newSteps);
                    }

                    existingDoc.UpdatedAt = DateTime.UtcNow;
                    existingDoc.UpdatedBy = userId;

                    await _dynamic.ReplaceOneAsync(x => x.Id == existingDoc.Id, existingDoc);
                }

                response.Message = "Audit step(s) saved successfully.";
                response.HttpStatusCode = HttpStatusCode.OK;
                response.Success = true;
                response.ResponseCode = 0;
            }
            catch (Exception ex)
            {
                response.Message = "CreateDynamicAuditForm Exception: " + ex.Message;
                response.HttpStatusCode = HttpStatusCode.InternalServerError;
                response.Success = false;
            }

            return response;
        }

        
        public async Task<GetDynamicAuditFormResponse> GetDynamicAuditTemplate(GetDynamicAuditRequest request)
        {
            var response = new GetDynamicAuditFormResponse();

            try
            {
                if (string.IsNullOrEmpty(request.certificateId))
                {
                    response.Message = "CertificateId is required.";
                    response.HttpStatusCode = HttpStatusCode.BadRequest;
                    response.Success = false;
                    return response;
                }

                // Base filter: by certificate and not deleted
                var filter = Builders<tbl_dynamic_audit_template>.Filter.And(
                    Builders<tbl_dynamic_audit_template>.Filter.Eq(x => x.Fk_certificateId, request.certificateId),
                    Builders<tbl_dynamic_audit_template>.Filter.Eq(x => x.IsDelete, false)
                );
                
                // Fetch documents
                var docs = await _dynamic.Find(filter).ToListAsync();

                response.Data = docs;
                response.Message = docs.Any() ? "Audit data retrieved successfully." : "No records found.";
                response.HttpStatusCode = HttpStatusCode.OK;
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Message = "GetDynamicAuditTemplate Exception: " + ex.Message;
                response.HttpStatusCode = HttpStatusCode.InternalServerError;
                response.Success = false;
            }

            return response;
        }



        //[HttpPost("SaveCompetency")]
        //public async Task<BaseResponse> SaveCompetency([FromBody] CompetencyRequest request)
        //{
        //    var response = new BaseResponse();

        //    try
        //    {
        //        if (request == null)
        //        {
        //            response.Message = "Request body is required.";
        //            response.HttpStatusCode = HttpStatusCode.BadRequest;
        //            response.Success = false;
        //            return response;
        //        }

        //        // 🔹 Check by ApplicantId (not Id)
        //        var existingProfile = await _competency
        //            .Find(x => x.ApplicantId == request.ApplicantId && !x.IsDelete)
        //            .FirstOrDefaultAsync();

        //        if (existingProfile == null)
        //        {
        //            // === CREATE NEW PROFILE ===
        //            var newProfile = new tbl_competencyProfile
        //            {
        //                ApplicantId = request.ApplicantId,
        //                CreatedBy = request.CreatedBy,
        //                CreatedAt = DateTime.UtcNow,
        //                UpdatedAt = DateTime.UtcNow,
        //                IsDelete = false,

        //                PersonalInfo = new PersonalInfo
        //                {
        //                    Name = request.Name,
        //                    Nationality = request.Nationality,
        //                    DateOfBirth = DateTime.TryParse(request.DateOfBirth, out var dob) ? dob : DateTime.MinValue,
        //                    Gender = request.Gender,
        //                    AddressCurrent = request.AddressCurrent,
        //                    AddressPermanent = request.AddressPermanent,
        //                    Phone = request.Phone,
        //                    Email = request.Email,
        //                    EmploymentType = request.EmploymentType
        //                },

        //                RolesApplied = request.Roles ?? new List<string>(),
        //                SchemesApplied = request.Schemes ?? new List<string>(),
        //                Languages = request.Languages ?? new List<LanguageProficiency>(),
        //                DigitalCompetence = request.DigitalCompetence ?? new List<string>(),
        //                AcademicQualifications = request.Qualifications ?? new List<AcademicQualification>(),
        //                Training = request.Training,
        //                ProductExpertise = request.Products ?? new List<ProductExpertise>(),
        //                AuditorRegistrations = request.AuditorRegistration ?? new List<AuditorRegistration>(),
        //                AuditingExperience = request.AuditingExperience ?? new List<ExperienceDetails>(),
        //                TechnicalFileReview = request.TechnicalFileReview ?? new List<ExperienceDetails>(),
        //                ConsultancyExperience = request.ConsultancyExperience ?? new List<ExperienceDetails>(),
        //                Achievements = request.Achievements,
        //                Enclosures = request.Enclosures ?? new List<bool>(),
        //                Signature = request.Signature,
        //                Date = DateTime.TryParse(request.Date, out var parsedDate) ? parsedDate : null,

        //                // Part B (store empty/null if not sent)
        //                ApplicantName = request.ApplicantName,
        //                ApplyingRole = request.ApplyingRole,
        //                ApplicationType = request.ApplicationType,
        //                Expertise = request.Expertise ?? new Dictionary<string, Expertise>(),
        //                CompetenceAreas = request.CompetenceAreas ?? new List<CompetenceArea>(),
        //                OverallAssessment = request.OverallAssessment,
        //                Recommendations = request.Recommendations,
        //                AssessorName = request.AssessorName,
        //                AssessorSignature = request.AssessorSignature,
        //                PartBDate = DateTime.TryParse(request.PartBDate, out var pbDate) ? pbDate : null
        //            };

        //            await _competency.InsertOneAsync(newProfile);
        //            response.Message = "Competency profile created successfully.";
        //        }
        //        else
        //        {
        //            // === UPDATE EXISTING PROFILE ===
        //            var updateDef = Builders<tbl_competencyProfile>.Update
        //                .Set(x => x.UpdatedAt, DateTime.UtcNow);

        //            // Update only non-null/non-empty values
        //            if (request.Name != null) updateDef = updateDef.Set(x => x.PersonalInfo.Name, request.Name);
        //            if (request.Nationality != null) updateDef = updateDef.Set(x => x.PersonalInfo.Nationality, request.Nationality);
        //            if (!string.IsNullOrEmpty(request.DateOfBirth) && DateTime.TryParse(request.DateOfBirth, out var dob))
        //                updateDef = updateDef.Set(x => x.PersonalInfo.DateOfBirth, dob);
        //            if (request.Gender != null) updateDef = updateDef.Set(x => x.PersonalInfo.Gender, request.Gender);
        //            if (request.AddressCurrent != null) updateDef = updateDef.Set(x => x.PersonalInfo.AddressCurrent, request.AddressCurrent);
        //            if (request.AddressPermanent != null) updateDef = updateDef.Set(x => x.PersonalInfo.AddressPermanent, request.AddressPermanent);
        //            if (request.Phone != null) updateDef = updateDef.Set(x => x.PersonalInfo.Phone, request.Phone);
        //            if (request.Email != null) updateDef = updateDef.Set(x => x.PersonalInfo.Email, request.Email);
        //            if (request.EmploymentType != null) updateDef = updateDef.Set(x => x.PersonalInfo.EmploymentType, request.EmploymentType);

        //            // Collections - only overwrite if new data present
        //            if (request.Roles?.Any() == true) updateDef = updateDef.Set(x => x.RolesApplied, request.Roles);
        //            if (request.Schemes?.Any() == true) updateDef = updateDef.Set(x => x.SchemesApplied, request.Schemes);
        //            if (request.Languages?.Any() == true) updateDef = updateDef.Set(x => x.Languages, request.Languages);
        //            if (request.DigitalCompetence?.Any() == true) updateDef = updateDef.Set(x => x.DigitalCompetence, request.DigitalCompetence);
        //            if (request.Qualifications?.Any() == true) updateDef = updateDef.Set(x => x.AcademicQualifications, request.Qualifications);
        //            if (request.Training != null) updateDef = updateDef.Set(x => x.Training, request.Training);
        //            if (request.Products?.Any() == true) updateDef = updateDef.Set(x => x.ProductExpertise, request.Products);
        //            if (request.AuditorRegistration?.Any() == true) updateDef = updateDef.Set(x => x.AuditorRegistrations, request.AuditorRegistration);
        //            if (request.AuditingExperience?.Any() == true) updateDef = updateDef.Set(x => x.AuditingExperience, request.AuditingExperience);
        //            if (request.TechnicalFileReview?.Any() == true) updateDef = updateDef.Set(x => x.TechnicalFileReview, request.TechnicalFileReview);
        //            if (request.ConsultancyExperience?.Any() == true) updateDef = updateDef.Set(x => x.ConsultancyExperience, request.ConsultancyExperience);
        //            if (request.Achievements != null) updateDef = updateDef.Set(x => x.Achievements, request.Achievements);
        //            if (request.Enclosures?.Any() == true) updateDef = updateDef.Set(x => x.Enclosures, request.Enclosures);
        //            if (request.Signature != null) updateDef = updateDef.Set(x => x.Signature, request.Signature);
        //            if (!string.IsNullOrEmpty(request.Date) && DateTime.TryParse(request.Date, out var parsedDate))
        //                updateDef = updateDef.Set(x => x.Date, parsedDate);

        //            // === Part B fields ===
        //            if (request.ApplicantName != null) updateDef = updateDef.Set(x => x.ApplicantName, request.ApplicantName);
        //            if (request.ApplyingRole != null) updateDef = updateDef.Set(x => x.ApplyingRole, request.ApplyingRole);
        //            if (request.ApplicationType != null) updateDef = updateDef.Set(x => x.ApplicationType, request.ApplicationType);
        //            if (request.Expertise?.Any() == true) updateDef = updateDef.Set(x => x.Expertise, request.Expertise);
        //            if (request.CompetenceAreas?.Any() == true) updateDef = updateDef.Set(x => x.CompetenceAreas, request.CompetenceAreas);
        //            if (request.OverallAssessment != null) updateDef = updateDef.Set(x => x.OverallAssessment, request.OverallAssessment);
        //            if (request.Recommendations != null) updateDef = updateDef.Set(x => x.Recommendations, request.Recommendations);
        //            if (request.AssessorName != null) updateDef = updateDef.Set(x => x.AssessorName, request.AssessorName);
        //            if (request.AssessorSignature != null) updateDef = updateDef.Set(x => x.AssessorSignature, request.AssessorSignature);
        //            if (!string.IsNullOrEmpty(request.PartBDate) && DateTime.TryParse(request.PartBDate, out var pbDate))
        //                updateDef = updateDef.Set(x => x.PartBDate, pbDate);

        //            await _competency.UpdateOneAsync(x => x.ApplicantId == request.ApplicantId, updateDef);
        //            response.Message = "Competency profile updated successfully.";
        //        }

        //        response.Success = true;
        //        response.HttpStatusCode = HttpStatusCode.OK;
        //        response.ResponseCode = 0;
        //    }
        //    catch (Exception ex)
        //    {
        //        response.Message = "Error: " + ex.Message;
        //        response.HttpStatusCode = HttpStatusCode.InternalServerError;
        //        response.Success = false;
        //    }

        //    return response;
        //}



        [HttpPost("SaveCompetency")]
        public async Task<BaseResponse> SaveCompetency([FromBody] CompetencyRequest request)
        {
            var response = new BaseResponse();

            try
            {
                if (request == null)
                {
                    response.Message = "Data is required.";
                    response.HttpStatusCode = HttpStatusCode.BadRequest;
                    response.Success = false;
                    return response;
                }

                // 🔹 Check if this competency already exists (for update)
                tbl_competencyProfile existingProfile = null;

                if (!string.IsNullOrEmpty(request.Id))
                {
                    existingProfile = await _competency
                        .Find(x => x.Id == request.Id && !x.IsDelete)
                        .FirstOrDefaultAsync();
                }

                if (existingProfile == null)
                {
                    // 🆕 CREATE NEW PROFILE
                    var newEntity = new tbl_competencyProfile
                    {
                        PersonalInfo = new PersonalInfo
                        {
                            Name = request.Name,
                            Nationality = request.Nationality,
                            DateOfBirth = DateTime.TryParse(request.DateOfBirth, out var dob) ? dob : DateTime.MinValue,
                            Gender = request.Gender,
                            AddressCurrent = request.AddressCurrent,
                            AddressPermanent = request.AddressPermanent,
                            Phone = request.Phone,
                            Email = request.Email,
                            EmploymentType = request.EmploymentType
                        },
                        ApplicantId = request.ApplicantId,
                        RolesApplied = request.Roles,
                        SchemesApplied = request.Schemes,
                        Languages = request.Languages,
                        DigitalCompetence = request.DigitalCompetence,
                        AcademicQualifications = request.Qualifications,
                        Training = request.Training,
                        ProductExpertise = request.Products,
                        AuditorRegistrations = request.AuditorRegistration,
                        AuditingExperience = request.AuditingExperience,
                        TechnicalFileReview = request.TechnicalFileReview,
                        ConsultancyExperience = request.ConsultancyExperience,
                        Achievements = request.Achievements,
                        Enclosures = request.Enclosures,
                        Signature = request.Signature,
                        Date = DateTime.TryParse(request.Date, out var parsedDate) ? parsedDate : DateTime.UtcNow,
                        CreatedBy = request.CreatedBy,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsDelete = false
                    };

                    await _competency.InsertOneAsync(newEntity);

                    response.Message = "Competency profile created successfully.";
                    response.Success = true;
                    response.HttpStatusCode = HttpStatusCode.OK;
                    response.ResponseCode = 0;
                }
                else
                {
                    // ✏️ UPDATE EXISTING PROFILE
                    var updateDef = Builders<tbl_competencyProfile>.Update
                        .Set(x => x.PersonalInfo.Name, request.Name ?? existingProfile.PersonalInfo.Name)
                        .Set(x => x.PersonalInfo.Nationality, request.Nationality ?? existingProfile.PersonalInfo.Nationality)
                        .Set(x => x.PersonalInfo.DateOfBirth, DateTime.TryParse(request.DateOfBirth, out var dob) ? dob : existingProfile.PersonalInfo.DateOfBirth)
                        .Set(x => x.PersonalInfo.Gender, request.Gender ?? existingProfile.PersonalInfo.Gender)
                        .Set(x => x.PersonalInfo.AddressCurrent, request.AddressCurrent ?? existingProfile.PersonalInfo.AddressCurrent)
                        .Set(x => x.PersonalInfo.AddressPermanent, request.AddressPermanent ?? existingProfile.PersonalInfo.AddressPermanent)
                        .Set(x => x.PersonalInfo.Phone, request.Phone ?? existingProfile.PersonalInfo.Phone)
                        .Set(x => x.PersonalInfo.Email, request.Email ?? existingProfile.PersonalInfo.Email)
                        .Set(x => x.PersonalInfo.EmploymentType, request.EmploymentType ?? existingProfile.PersonalInfo.EmploymentType)
                        .Set(x => x.RolesApplied, request.Roles?.Any() == true ? request.Roles : existingProfile.RolesApplied)
                        .Set(x => x.SchemesApplied, request.Schemes?.Any() == true ? request.Schemes : existingProfile.SchemesApplied)
                        .Set(x => x.Languages, request.Languages?.Any() == true ? request.Languages : existingProfile.Languages)
                        .Set(x => x.DigitalCompetence, request.DigitalCompetence ?? existingProfile.DigitalCompetence)
                        .Set(x => x.AcademicQualifications, request.Qualifications?.Any() == true ? request.Qualifications : existingProfile.AcademicQualifications)
                        .Set(x => x.Training, request.Training ?? existingProfile.Training)
                        .Set(x => x.ProductExpertise, request.Products?.Any() == true ? request.Products : existingProfile.ProductExpertise)
                        .Set(x => x.AuditorRegistrations, request.AuditorRegistration?.Any() == true ? request.AuditorRegistration : existingProfile.AuditorRegistrations)
                        .Set(x => x.AuditingExperience, request.AuditingExperience ?? existingProfile.AuditingExperience)
                        .Set(x => x.TechnicalFileReview, request.TechnicalFileReview ?? existingProfile.TechnicalFileReview)
                        .Set(x => x.ConsultancyExperience, request.ConsultancyExperience ?? existingProfile.ConsultancyExperience)
                        .Set(x => x.Achievements, request.Achievements ?? existingProfile.Achievements)
                        .Set(x => x.Enclosures, request.Enclosures ?? existingProfile.Enclosures)
                        .Set(x => x.Signature, request.Signature ?? existingProfile.Signature)
                        .Set(x => x.Date, DateTime.TryParse(request.Date, out var parsedDate) ? parsedDate : existingProfile.Date)
                        .Set(x => x.UpdatedAt, DateTime.UtcNow);

                    await _competency.UpdateOneAsync(x => x.Id == request.Id, updateDef);

                    response.Message = "Competency profile updated successfully.";
                    response.Success = true;
                    response.HttpStatusCode = HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                response.Message = "Error: " + ex.Message;
                response.HttpStatusCode = HttpStatusCode.InternalServerError;
                response.Success = false;
            }

            return response;
        }
        [HttpPost("GetCompetency")]
        public async Task<GetCompetencyResponse> GetCompetency([FromBody] GetCompetencyRequest request)
        {
            var response = new GetCompetencyResponse();

            try
            {
                // 🛑 Validation
                if (request == null || string.IsNullOrEmpty(request.CompetencyId))
                {
                    response.Message = "Competency ID is required.";
                    response.HttpStatusCode = HttpStatusCode.BadRequest;
                    response.Success = false;
                    response.ResponseCode = 1;
                    return response;
                }

                // 🔍 Find competency document
                var competency = await _competency
                    .Find(x => x.Id == request.CompetencyId && !x.IsDelete)
                    .FirstOrDefaultAsync();

                // ❌ Not Found
                if (competency == null)
                {
                    response.Message = "No competency profile found.";
                    response.HttpStatusCode = HttpStatusCode.NotFound;
                    response.Success = false;
                    response.ResponseCode = 1;
                    return response;
                }

                // ✅ Found
                response.Message = "Competency profile fetched successfully.";
                response.HttpStatusCode = HttpStatusCode.OK;
                response.Success = true;
                response.ResponseCode = 0;
                response.Data = competency;
            }
            catch (Exception ex)
            {
                response.Message = "GetCompetency Exception: " + ex.Message;
                response.HttpStatusCode = HttpStatusCode.InternalServerError;
                response.Success = false;
                response.ResponseCode = -1;
            }

            return response;
        }




        //[HttpPost("SaveCompetency")]
        //public async Task<BaseResponse> SaveCompetency(CompetencyRequest request)
        //{
        //    var response = new BaseResponse();

        //    if (request == null)
        //    {
        //        response.Message = "Data is required.";
        //        response.HttpStatusCode = HttpStatusCode.BadRequest;
        //        response.Success = false;
        //        return response;
        //    }

        //    var entity = new tbl_competencyProfile
        //    {
        //        PersonalInfo = new PersonalInfo
        //        {
        //            Name = request.Name,
        //            Nationality = request.Nationality,
        //            DateOfBirth = DateTime.TryParse(request.DateOfBirth, out var dob) ? dob : DateTime.MinValue,
        //            Gender = request.Gender,
        //            AddressCurrent = request.AddressCurrent,
        //            AddressPermanent = request.AddressPermanent,
        //            Phone = request.Phone,
        //            Email = request.Email,
        //            EmploymentType = request.EmploymentType
        //        },
        //        RolesApplied = request.Roles,
        //        SchemesApplied = request.Schemes,
        //        Languages = request.Languages,
        //        DigitalCompetence = request.DigitalCompetence,
        //        AcademicQualifications = request.Qualifications,
        //        Training = request.Training,
        //        ProductExpertise = request.Products,
        //        AuditorRegistrations = request.AuditorRegistration,
        //        AuditingExperience = request.AuditingExperience,
        //        TechnicalFileReview = request.TechnicalFileReview,
        //        ConsultancyExperience = request.ConsultancyExperience,
        //        Achievements = request.Achievements,
        //        Enclosures = request.Enclosures,
        //        Signature = request.Signature,
        //        Date = DateTime.TryParse(request.Date, out var parsedDate) ? parsedDate : DateTime.UtcNow,
        //        CreatedBy = request.CreatedBy,
        //        CreatedAt = DateTime.UtcNow,
        //        IsDelete = false
        //    };

        //    await _competency.InsertOneAsync(entity);

        //    response.Message = "Competency profile saved successfully";
        //    response.HttpStatusCode = HttpStatusCode.OK;
        //    response.Success = true;
        //    return response;
        //}


        [HttpPost("SaveAuditNomination")]
        public async Task<BaseResponse> SaveAuditNomination([FromBody] AssignAuditRequest request)
        {
            var response = new BaseResponse();

            try
            {
                // 1️⃣ Validation
                if (string.IsNullOrEmpty(request.ApplicationId) || string.IsNullOrEmpty(request.CertificationId))
                {
                    response.Message = "ApplicationId or CertificationId is missing.";
                    response.HttpStatusCode = HttpStatusCode.BadRequest;
                    response.Success = false;
                    return response;
                }

                // 2️⃣ Get current user
                var userId = _acc.HttpContext?.Session.GetString("UserId");

                // 3️⃣ Find if nomination already exists
                var existingAudit = await _assignaudit
                    .Find(x => x.ApplicationId == request.ApplicationId && x.CertificationId == request.CertificationId)
                    .FirstOrDefaultAsync();

                // Extract assigned users (based on email/mobile mapping logic or user IDs directly if available)
                var assignedUserIds = request.TeamDetails?
                    .Where(t => !string.IsNullOrEmpty(t.UserId)) // assume frontend passes UserId for each team member
                    .Select(t => t.UserId)
                    .Distinct()
                    .ToList() ?? new List<string>();

                if (existingAudit == null)
                {
                    // 4️⃣ Create new nomination
                    var newAudit = new tbl_AssignAudit
                    {
                        ApplicationId = request.ApplicationId,
                        AuditId = request.AuditId,
                        CertificationId = request.CertificationId,
                        StartDate = request.StartDate,
                        EndDate = request.EndDate,
                        StartTime = request.StartTime,
                        EndTime = request.EndTime,
                        TotalAuditTime = request.TotalAuditTime,
                        OnsiteAuditTime = request.OnsiteAuditTime,
                        OffsiteActivityTime = request.OffsiteActivityTime,
                        AdditionalInfo = request.AdditionalInfo,
                        TeamDetails = request.TeamDetails?.ConvertAll(x => new NominatedTeam
                        {
                            Name = x.Name,
                            Role = x.Role,
                            TechnicalArea = x.TechnicalArea,
                            Email = x.Email,
                            Mobile = x.Mobile,
                            UserId = x.UserId
                        }),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _assignaudit.InsertOneAsync(newAudit);

                    // ✅ Update tbl_Audit to assign these users
                    var audit = await _audit.Find(x => x.Id == request.AuditId).FirstOrDefaultAsync();

                    if (audit != null)
                    {
                        audit.IsAuditNominationDone = true;

                        // Update the document by replacing the modified object
                        await _audit.ReplaceOneAsync(x => x.Id == audit.Id, audit);
                    }

                }
                else
                {
                    // 5️⃣ Update existing nomination
                    existingAudit.StartDate = request.StartDate;
                    existingAudit.EndDate = request.EndDate;
                    existingAudit.StartTime = request.StartTime;
                    existingAudit.EndTime = request.EndTime;
                    existingAudit.TotalAuditTime = request.TotalAuditTime;
                    existingAudit.OnsiteAuditTime = request.OnsiteAuditTime;
                    existingAudit.OffsiteActivityTime = request.OffsiteActivityTime;
                    existingAudit.AdditionalInfo = request.AdditionalInfo;
                    existingAudit.TeamDetails = request.TeamDetails?.ConvertAll(x => new NominatedTeam
                    {
                        Name = x.Name,
                        Role = x.Role,
                        TechnicalArea = x.TechnicalArea,
                        Email = x.Email,
                        Mobile = x.Mobile,
                        UserId = x.UserId
                    });
                    existingAudit.UpdatedAt = DateTime.UtcNow;

                    var audit = await _audit.Find(x => x.Id == request.AuditId).FirstOrDefaultAsync();

                    if (audit != null)
                    {
                        audit.IsAuditNominationDone = true;

                        // Update the document by replacing the modified object
                        await _audit.ReplaceOneAsync(x => x.Id == audit.Id, audit);
                    }

                    await _assignaudit.ReplaceOneAsync(x => x.Id == existingAudit.Id, existingAudit);
                }

                // 6️⃣ Update tbl_Audit to assign these users
                var auditRecord = await _audit.Find(x =>
                    x.ApplicationId == request.ApplicationId &&
                    x.Fk_CertificateId == request.CertificationId).FirstOrDefaultAsync();

                if (auditRecord != null)
                {
                    var updateDef = Builders<tbl_Audit>.Update.Set(x => x.AssignedUsers, assignedUserIds);
                    await _audit.UpdateOneAsync(x => x.Id == auditRecord.Id, updateDef);
                }

                response.Message = "Audit nomination saved and assigned successfully.";
                response.HttpStatusCode = HttpStatusCode.OK;
                response.Success = true;
                response.ResponseCode = 0;
            }
            catch (Exception ex)
            {
                response.Message = "SaveAuditNomination Exception: " + ex.Message;
                response.HttpStatusCode = HttpStatusCode.InternalServerError;
                response.Success = false;
            }

            return response;
        }



        //[HttpPost("SaveAuditNomination")]
        //public async Task<BaseResponse> SaveAuditNomination([FromBody] AssignAuditRequest request)
        //{
        //    var response = new BaseResponse();

        //    try
        //    {
        //        // 1️⃣ Validation
        //        if (string.IsNullOrEmpty(request.ApplicationId) || string.IsNullOrEmpty(request.CertificationId))
        //        {
        //            response.Message = "ApplicationId or CertificationId is missing.";
        //            response.HttpStatusCode = HttpStatusCode.BadRequest;
        //            response.Success = false;
        //            return response;
        //        }

        //        // 2️⃣ Get user id (if session available)
        //        var userId = _acc.HttpContext?.Session.GetString("UserId");

        //        // 3️⃣ Check if record already exists
        //        var existingAudit = await _assignaudit
        //            .Find(x => x.ApplicationId == request.ApplicationId && x.CertificationId == request.CertificationId)
        //            .FirstOrDefaultAsync();

        //        // 4️⃣ If record does not exist → Create new
        //        if (existingAudit == null)
        //        {
        //            var newAudit = new tbl_AssignAudit
        //            {
        //                ApplicationId = request.ApplicationId,
        //                CertificationId = request.CertificationId,
        //                AuditDate = request.AuditDate,
        //                TotalAuditTime = request.TotalAuditTime,
        //                OnsiteAuditTime = request.OnsiteAuditTime,
        //                OffsiteActivityTime = request.OffsiteActivityTime,
        //                AdditionalInfo = request.AdditionalInfo,
        //                TeamDetails = request.TeamDetails?.ConvertAll(x => new NominatedTeam
        //                {
        //                    Name = x.Name,
        //                    Role = x.Role,
        //                    TechnicalArea = x.TechnicalArea,
        //                    Email = x.Email,
        //                    Mobile = x.Mobile
        //                }),
        //                CreatedAt = DateTime.UtcNow
        //            };

        //            await _assignaudit.InsertOneAsync(newAudit);

        //            response.Message = "Audit nomination created successfully.";
        //            response.HttpStatusCode = HttpStatusCode.OK;
        //            response.Success = true;
        //            response.ResponseCode = 0;
        //        }
        //        else
        //        {
        //            // 5️⃣ Update existing record
        //            existingAudit.AuditDate = request.AuditDate;
        //            existingAudit.TotalAuditTime = request.TotalAuditTime;
        //            existingAudit.OnsiteAuditTime = request.OnsiteAuditTime;
        //            existingAudit.OffsiteActivityTime = request.OffsiteActivityTime;
        //            existingAudit.AdditionalInfo = request.AdditionalInfo;
        //            existingAudit.TeamDetails = request.TeamDetails?.ConvertAll(x => new NominatedTeam
        //            {
        //                Name = x.Name,
        //                Role = x.Role,
        //                TechnicalArea = x.TechnicalArea,
        //                Email = x.Email,
        //                Mobile = x.Mobile
        //            });
        //            existingAudit.CreatedAt = existingAudit.CreatedAt; // keep old created date
        //            existingAudit.UpdatedAt = DateTime.UtcNow;

        //            await _assignaudit.ReplaceOneAsync(x => x.Id == existingAudit.Id, existingAudit);

        //            response.Message = "Audit nomination updated successfully.";
        //            response.HttpStatusCode = HttpStatusCode.OK;
        //            response.Success = true;
        //            response.ResponseCode = 0;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        response.Message = "SaveAuditNomination Exception: " + ex.Message;
        //        response.HttpStatusCode = HttpStatusCode.InternalServerError;
        //        response.Success = false;
        //    }

        //    return response;
        //}


        // Safely parse string -> double, returns 0 if null, empty or invalid
        private double ParseToDouble(string value)
        {
            if (double.TryParse(value, out double result))
                return result;
            return 0;
        }




        [HttpPost("GetAuditNomination")]
        public async Task<getAuditNominationResponse> GetAuditNomination([FromBody] getAuditNominationRequest request)
        {
            var response = new getAuditNominationResponse();

            try
            {
                if (string.IsNullOrEmpty(request.ApplicationId) || string.IsNullOrEmpty(request.CertificationId))
                {
                    response.Message = "ApplicationId or CertificationId is missing.";
                    response.HttpStatusCode = HttpStatusCode.BadRequest;
                    response.Success = false;
                    return response;
                }



                var certificate = await _masterCertificate
                    .Find(x => x.Id == request.CertificationId)
                    .FirstOrDefaultAsync();

                var nomination = await _assignaudit
                    .Find(x => x.ApplicationId == request.ApplicationId && x.CertificationId == request.CertificationId)
                    .FirstOrDefaultAsync();

                if (nomination == null)
                {
                    if (certificate == null)
                    {
                        response.Message = "CertificateId is required.";
                        response.HttpStatusCode = HttpStatusCode.BadRequest;
                        response.Success = false;
                        return response;
                    }
                    var IsoData = new tbl_ISO_Application();
                    if (certificate.Certificate_Name == "ISO")
                    {
                        IsoData = _iso.Find(x => x.ApplicationId == request.ApplicationId && x.Fk_Certificate == request.CertificationId).FirstOrDefault();

                    }
                    else if (certificate.Certificate_Name == "FSSC")
                    {
                        IsoData = _iso.Find(x => x.ApplicationId == request.ApplicationId && x.Fk_Certificate == request.CertificationId).FirstOrDefault();
                    }
                    else if (certificate.Certificate_Name == "Imdr")
                    {
                        IsoData = _iso.Find(x => x.ApplicationId == request.ApplicationId && x.Fk_Certificate == request.CertificationId).FirstOrDefault();
                    }
                    else if (certificate.Certificate_Name == "ICMED")
                    {
                        IsoData = _iso.Find(x => x.ApplicationId == request.ApplicationId && x.Fk_Certificate == request.CertificationId).FirstOrDefault();
                    }
                    else if (certificate.Certificate_Name == "ICMEDPLUS")
                    {
                        IsoData = _iso.Find(x => x.ApplicationId == request.ApplicationId && x.Fk_Certificate == request.CertificationId).FirstOrDefault();
                    }
                    else
                    {
                        throw new Exception("No document found for given ApplicationId and CertificateTypeId");
                    }
                }

                if (nomination != null)
                {
                    var auditNomination = new AuditNominationDetails
                    {
                        Id = nomination.Id.ToString(),
                        AuditId = request.AuditId,
                        ApplicationId = nomination.ApplicationId,
                        CertificationId = nomination.CertificationId,
                        StartDate = nomination.StartDate,
                        EndDate = nomination.EndDate,
                        StartTime = nomination.StartTime,
                        EndTime = nomination.EndTime,
                        TotalAuditTime = nomination.TotalAuditTime,
                        OnsiteAuditTime = nomination.OnsiteAuditTime,
                        OffsiteActivityTime = nomination.OffsiteActivityTime,
                        AdditionalInfo = nomination.AdditionalInfo,
                        TeamDetails = nomination.TeamDetails?.Select(x => new NominatedTeamResponse
                        {
                            UserId = x.UserId,
                            Name = x.Name,
                            Role = x.Role,
                            TechnicalArea = x.TechnicalArea,
                            Email = x.Email,
                            Mobile = x.Mobile
                        }).ToList()
                    };
                    response.Data = new List<AuditNominationDetails> { auditNomination };
                    response.Message = "Audit nomination fetched successfully.";
                    response.HttpStatusCode = HttpStatusCode.OK;
                    response.Success = true;
                    response.ResponseCode = 0;
                }

                

               
            }
            catch (Exception ex)
            {
                response.Message = "GetAuditNomination Exception: " + ex.Message;
                response.HttpStatusCode = HttpStatusCode.InternalServerError;
                response.Success = false;
            }

            return response;
        }

        [HttpPost("GetAuditorList")]
        public async Task<GetAuditorListResponse> GetAuditorList(BaseRequest request)
        {
            var response = new GetAuditorListResponse();

            try
            {
                var users = await _user
            .Find(x => x.IsDelete == 0 &&
                      (x.Fk_RoleID == "68f0f445c61aaa8e75ac2448" ||
                       x.Fk_RoleID == "68f0f47dc61aaa8e75ac2449"))
            .ToListAsync();

                // 2️⃣ Fetch all Technical Areas
                var technicalAreas = await _tech.Find(_ => true).ToListAsync();


                // 3️⃣ Map data
                var data = users.Select(u => new AuditorDetails
                {
                    UserId =u.Id,
                    Name = u.FullName,
                    Role = u.Fk_RoleID == "68f0f47dc61aaa8e75ac2449" ? "Lead Auditor" : "Auditor",
                    TechnicalArea = string.Join(", ", technicalAreas.Select(t => t.Name)),
                    Email = u.EmailId,
                    Mobile = u.ContactNo
                }).ToList();

                response.Data = data;
                response.Success = true;
                response.Message = "Auditor details fetched successfully.";
            }
            catch (Exception ex)
            {
                response.Message = "GetAuditorList Exception: " + ex.Message;
                response.HttpStatusCode = HttpStatusCode.InternalServerError;
                response.Success = false;
            }

            return response;
        }



        [HttpPost("SaveAuditAdministration")]
        public async Task<BaseResponse> SaveAuditAdministration([FromBody] AuditAdministrationRequest request)
        {
            var response = new BaseResponse();

            try
            {
                if (string.IsNullOrEmpty(request.AuditId))
                {
                    response.Message = "AuditId is required.";
                    response.HttpStatusCode = HttpStatusCode.BadRequest;
                    response.Success = false;
                    return response;
                }

                // Check if already exists for this audit
                var existing = await _auditAdministration
                    .Find(x => x.AuditId == request.AuditId && !x.IsDelete)
                    .FirstOrDefaultAsync();

                if (existing == null)
                {
                    // Insert new record
                    var entity = new tbl_audit_administration
                    {
                        AuditId = request.AuditId,
                        Type = request.Type,
                        AdministrationHardCopy = request.AdministrationHardCopy ?? new(),
                        AdministrationSoftCopy = request.AdministrationSoftCopy ?? new(),
                        AdditionalComments = request.AdditionalComments ?? "",
                        ReviewedBy = request.ReviewedBy ?? "",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsDelete = false
                    };

                    await _auditAdministration.InsertOneAsync(entity);
                    response.Message = "Audit administration data saved successfully.";
                }
                else
                {
                    // Update existing record
                    var updateDef = Builders<tbl_audit_administration>.Update
                        .Set(x => x.Type, request.Type ?? existing.Type)
                        .Set(x => x.AdministrationHardCopy, request.AdministrationHardCopy ?? existing.AdministrationHardCopy)
                        .Set(x => x.AdministrationSoftCopy, request.AdministrationSoftCopy ?? existing.AdministrationSoftCopy)
                        .Set(x => x.AdditionalComments, request.AdditionalComments ?? existing.AdditionalComments)
                        .Set(x => x.ReviewedBy, request.ReviewedBy ?? existing.ReviewedBy)
                        .Set(x => x.UpdatedAt, DateTime.UtcNow);

                    await _auditAdministration.UpdateOneAsync(x => x.Id == existing.Id, updateDef);
                    response.Message = "Audit administration data updated successfully.";
                }

                response.Success = true;
                response.HttpStatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                response.Message = "Error saving audit administration: " + ex.Message;
                response.HttpStatusCode = HttpStatusCode.InternalServerError;
                response.Success = false;
            }

            return response;
        }

        [HttpPost("GetAuditAdministration")]
        public async Task<GetAuditAdministrationResponse> GetAuditAdministration([FromBody] GetAuditAdministrationRequest request)
        {
            var response = new GetAuditAdministrationResponse();

            try
            {
                if (string.IsNullOrEmpty(request.AuditId))
                {
                    response.Success = false;
                    response.Message = "AuditId is required.";
                    response.HttpStatusCode = HttpStatusCode.BadRequest;
                    return response;
                }

                // 🔹 Fetch audit
                var audit = await _audit
                    .Find(x => x.Id == request.AuditId )
                    .FirstOrDefaultAsync();

                

                // 🔹 Fetch certificate name
                var certificate = await _masterCertificate
                    .Find(x => x.Id == audit.Fk_CertificateId)
                    .FirstOrDefaultAsync();

                // 🔹 Fetch assigned audit team (TeamDetails array)
                var assignedAudit = await _assignaudit
                    .Find(x => x.ApplicationId == audit.ApplicationId)
                    .FirstOrDefaultAsync();

                // 🔹 Fetch assigned audit team (TeamDetails array)
                var certificationName = await _customer
                    .Find(x => x.Id == audit.ApplicationId)
                    .FirstOrDefaultAsync();

                var teamNames = assignedAudit?.TeamDetails?
                    .Select(t => t.Name)
                    .ToList() ?? new List<string>();

                // 🔹 Fetch administration data
                var adminData = await _auditAdministration
                    .Find(x => x.AuditId == request.AuditId && !x.IsDelete)
                    .FirstOrDefaultAsync();

                //if (adminData == null)
                //{
                //    response.Success = false;
                //    response.Message = "Audit administration data not found.";
                //    response.HttpStatusCode = HttpStatusCode.NotFound;
                //    return response;
                //}

                // 🔹 Build auditList
                response.auditList = new List<AuditList>
                {
                    new AuditList
                    {
                        AuditId = audit.Id,
                        FileNumber = audit.FileNumber,
                        Standard = certificate?.Certificate_Name ?? "",
                        AuditType = "Initial Audit",
                        ClientName = certificationName.Orgnization_Name ?? "",
                        Certification = certificate?.Certificate_Name ?? "",
                        Fk_CertificateId = audit.Fk_CertificateId,
                        AuditDate = audit.Received_Date,
                        Received_Date = audit.Received_Date,
                        auditPeople = teamNames
                    }
                };

                // 🔹 Add admin data
                response.Data = adminData;

                response.Success = true;
                response.Message = "Audit administration data fetched successfully.";
                response.HttpStatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Error fetching audit administration data: " + ex.Message;
                response.HttpStatusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }

        [HttpPost("SaveAuditAdministrationTechnical")]
        public async Task<SaveAuditTechResponse> SaveAuditAdministrationTechnical([FromBody] SaveAuditTechRequest request)
        {
            var response = new SaveAuditTechResponse();
            var UserId = _acc.HttpContext?.Session.GetString("UserId");
            try
            {
                // 🔸 Validate
                if (string.IsNullOrEmpty(request.AuditId))
                {
                    response.Success = false;
                    response.Message = "AuditId is required.";
                    response.HttpStatusCode = HttpStatusCode.BadRequest;
                    return response;
                }

                if (request.Technical == null || !request.Technical.Any())
                {
                    response.Success = false;
                    response.Message = "Technical items are required.";
                    response.HttpStatusCode = HttpStatusCode.BadRequest;
                    return response;
                }

                // 🔹 Check if technical data already exists for the audit
                var existing = await _auditAdministrationTechnical
                    .Find(x => x.AuditId == request.AuditId && !x.IsDelete)
                    .FirstOrDefaultAsync();

                if (existing == null)
                {
                    // 🔹 Create new record
                    var entity = new tbl_audit_administration_technical
                    {
                        AuditId = request.AuditId,
                        Type = request.Type,
                        Technical = request.Technical,
                        AdditionalComments = request.AdditionalComments,
                        ReviewedBy = request.ReviewedBy,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = UserId
                    };

                    await _auditAdministrationTechnical.InsertOneAsync(entity);

                    response.Data = entity;
                    response.Success = true;
                    response.Message = "Technical audit administration data saved successfully.";
                    response.HttpStatusCode = HttpStatusCode.OK;
                }
                else
                {
                    // 🔹 Update existing record
                    existing.Type = request.Type;
                    existing.Technical = request.Technical;
                    existing.AdditionalComments = request.AdditionalComments;
                    existing.ReviewedBy = request.ReviewedBy;
                    existing.UpdatedAt = DateTime.UtcNow;
                    existing.UpdatedBy = UserId;

                    await _auditAdministrationTechnical.ReplaceOneAsync(x => x.Id == existing.Id, existing);

                    response.Data = existing;
                    response.Success = true;
                    response.Message = "Technical audit administration data updated successfully.";
                    response.HttpStatusCode = HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Error saving technical audit administration data: " + ex.Message;
                response.HttpStatusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }


        [HttpPost("GetAuditTechnicalReview")]
        public async Task<GetAuditTechnicalReviewResponse> GetAuditTechnicalReview([FromBody] GetAuditAdministrationRequest request)
        {
            var response = new GetAuditTechnicalReviewResponse();

            try
            {
                if (string.IsNullOrEmpty(request.AuditId))
                {
                    response.Success = false;
                    response.Message = "AuditId is required.";
                    response.HttpStatusCode = HttpStatusCode.BadRequest;
                    return response;
                }

                // 🔹 Fetch audit
                var audit = await _audit
                    .Find(x => x.Id == request.AuditId)
                    .FirstOrDefaultAsync();

                if (audit == null)
                {
                    response.Success = false;
                    response.Message = "Audit not found.";
                    response.HttpStatusCode = HttpStatusCode.NotFound;
                    return response;
                }

                // 🔹 Fetch certificate name
                var certificate = await _masterCertificate
                    .Find(x => x.Id == audit.Fk_CertificateId)
                    .FirstOrDefaultAsync();

                // 🔹 Fetch assigned audit team (TeamDetails array)
                var assignedAudit = await _assignaudit
                    .Find(x => x.ApplicationId == audit.ApplicationId)
                    .FirstOrDefaultAsync();

                // 🔹 Fetch assigned audit team (TeamDetails array)
                var certificationName = await _customer
                    .Find(x => x.Id == audit.ApplicationId)
                    .FirstOrDefaultAsync();

                var teamNames = assignedAudit?.TeamDetails?
                    .Select(t => t.Name)
                    .ToList() ?? new List<string>();

                // 🔹 Fetch administration data
                var adminData = await _auditAdministrationTechnical
                    .Find(x => x.AuditId == request.AuditId && !x.IsDelete)
                    .FirstOrDefaultAsync();

                //if (adminData == null)
                //{
                //    response.Success = false;
                //    response.Message = "Audit administration data not found.";
                //    response.HttpStatusCode = HttpStatusCode.NotFound;
                //    return response;
                //}

                // 🔹 Build auditList
                response.auditList = new List<AuditList>
                {
                    new AuditList
                    {
                        AuditId = audit.Id,
                        FileNumber = audit.FileNumber,
                        Standard = certificate?.Certificate_Name ?? "",
                        AuditType = "Initial Audit",
                        ClientName = certificationName.Orgnization_Name ?? "",
                        Certification = certificate?.Certificate_Name ?? "",
                        Fk_CertificateId = audit.Fk_CertificateId,
                        AuditDate = audit.Received_Date,
                        Received_Date = audit.Received_Date,
                        auditPeople = teamNames
                    }
                };

                // 🔹 Add admin data
                response.Data = adminData;

                response.Success = true;
                response.Message = "Audit administration data fetched successfully.";
                response.HttpStatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Error fetching audit administration data: " + ex.Message;
                response.HttpStatusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }



        [HttpPost("SaveAuditProcess")]
        public async Task<BaseResponse> SaveAuditProcess([FromBody] saveAuditProcessRequest request)
        {
            var response = new BaseResponse();
            var UserId = _acc.HttpContext?.Session.GetString("UserId");
            try
            {
                if (request == null)
                {
                    response.Message = "Data is required.";
                    response.HttpStatusCode = HttpStatusCode.BadRequest;
                    response.Success = false;
                    return response;
                }
                var name = this._user.Find(x => x.Id == UserId).FirstOrDefault().FullName;
                var Status = "Pending";
                if (request.url != null)
                {
                    Status = "File Uploaded";
                }
                
                var newEntity = new tbl_AuditProcess
                {
                    AuditId= request.AuditId,
                    StepObjectId=request.StepObjectId,
                    Url=request.url,
                    ApplicationId=request.ApplicationId,
                    CertificateId =request.CertificateId,
                    CreatedBy = name ?? "",
                    UpdatedBy = UserId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Status = Status
                };

                 await _auditprocess.InsertOneAsync(newEntity);

                // 🔹 Get user name
                var user = await _user.Find(x => x.Id == UserId).FirstOrDefaultAsync();
                var userName = user?.FullName ?? "Unknown User";

                // 🔹 Build message here dynamically
                var timeStamp = DateTime.Now.ToString("dd-MMM-yyyy hh:mm tt");
                var updateMessage = $"{request.StepName} document uploaded by {userName} on {timeStamp}.";

                // 🔹 Call helper with separate fields
                await SaveRecentUpdateAsync(
                    request.AuditId,
                    request.ApplicationId,
                    request.CertificateId,
                    updateMessage,
                    userName
                );



                response.Message = "Audit Process Added successfully.";
                response.Success = true;
                response.HttpStatusCode = HttpStatusCode.OK;
                response.ResponseCode = 0;
                
            }
            catch (Exception ex)
            {
                response.Message = "Error: " + ex.Message;
                response.HttpStatusCode = HttpStatusCode.InternalServerError;
                response.Success = false;
            }

            return response;
        }
        [HttpPost("GetAuditProcess")]
        public async Task<GetAuditProcessResponse> GetAuditProcess([FromBody] GetAuditProcessRequest request)
        {
            var response = new GetAuditProcessResponse();

            try
            {
                if (request == null || string.IsNullOrEmpty(request.AuditId) || string.IsNullOrEmpty(request.StepObjectId))
                {
                    response.Message = "AuditId and StepObjectId are required.";
                    response.HttpStatusCode = HttpStatusCode.BadRequest;
                    response.Success = false;
                    return response;
                }

                // Fetch matching records from MongoDB
                var auditProcesses = await _auditprocess
                    .Find(x => x.AuditId == request.AuditId && x.StepObjectId == request.StepObjectId)
                    .SortByDescending(x => x.CreatedAt)
                    .ToListAsync();

                if (auditProcesses == null || auditProcesses.Count == 0)
                {
                    response.Message = "No data found for given AuditId and StepObjectId.";
                    response.HttpStatusCode = HttpStatusCode.NotFound;
                    response.Success = false;
                    return response;
                }

                response.Data = auditProcesses;
                response.Message = "Audit process data retrieved successfully.";
                response.HttpStatusCode = HttpStatusCode.OK;
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Message = "Error: " + ex.Message;
                response.HttpStatusCode = HttpStatusCode.InternalServerError;
                response.Success = false;
            }

            return response;
        }

        //[HttpPost("SaveUpdates")]
        //public async Task<BaseResponse> SaveUpdates([FromBody] SaveUpdatesRequest request)
        //{
        //    var response = new BaseResponse();
        //    var UserId = _acc.HttpContext?.Session.GetString("UserId");
        //    try
        //    {
                

        //        var newEntity = new tbl_RecentUpdate
        //        {
        //            AuditId = request.AuditId,
        //            Updates = request.Updates,
        //            Url = request.url,
        //            ApplicationId = request.ApplicationId,
        //            CertificateId = request.CertificateId,
        //            CreatedBy = UserId,
        //            UpdatedBy = UserId,
        //            CreatedAt = DateTime.UtcNow,
        //            UpdatedAt = DateTime.UtcNow,
        //        };

        //        await _auditprocess.InsertOneAsync(newEntity);

        //        response.Message = "Audit Process Added successfully.";
        //        response.Success = true;
        //        response.HttpStatusCode = HttpStatusCode.OK;
        //        response.ResponseCode = 0;

        //    }
        //    catch (Exception ex)
        //    {
        //        response.Message = "Error: " + ex.Message;
        //        response.HttpStatusCode = HttpStatusCode.InternalServerError;
        //        response.Success = false;
        //    }

        //    return response;
        //}

        protected override void Disposing()
        {
            //throw new NotImplementedException();
        }




        private async Task SaveRecentUpdateAsync(string auditId, string applicationId, string certificateId, string message, string userId)
        {
            try
            {
                var updateEntity = new tbl_RecentUpdate
                {
                    AuditId = auditId,
                    Updates = message,
                    ApplicationId = applicationId,
                    CertificateId = certificateId,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                };

                await _recentupdate.InsertOneAsync(updateEntity);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving recent update: {ex.Message}");
            }
        }

        [HttpPost("GetRecentUpdates")]
        public async Task<GetRecentUpdateResponse> GetRecentUpdates([FromBody] GetRecentUpdateRequest request)
        {
            var response = new GetRecentUpdateResponse();

            try
            {
                

                // 🔹 Fetch all updates for given AuditId (optionally filter by ApplicationId / CertificateId)
                var filter = Builders<tbl_RecentUpdate>.Filter.Eq(x => x.AuditId, request.AuditId);

                if (!string.IsNullOrEmpty(request.ApplicationId))
                    filter &= Builders<tbl_RecentUpdate>.Filter.Eq(x => x.ApplicationId, request.ApplicationId);

                if (!string.IsNullOrEmpty(request.CertificateId))
                    filter &= Builders<tbl_RecentUpdate>.Filter.Eq(x => x.CertificateId, request.CertificateId);

                var updates = await _recentupdate
                    .Find(filter)
                    .SortByDescending(x => x.CreatedAt)
                    .ToListAsync();

                if (updates == null || !updates.Any())
                {
                    response.Message = "No updates found for this AuditId.";
                    response.HttpStatusCode = HttpStatusCode.NotFound;
                    response.Success = false;
                    return response;
                }

                response.Data = updates;
                response.Message = "Recent updates retrieved successfully.";
                response.HttpStatusCode = HttpStatusCode.OK;
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Message = "Error: " + ex.Message;
                response.HttpStatusCode = HttpStatusCode.InternalServerError;
                response.Success = false;
            }

            return response;
        }


        [HttpPost("GetAuditSidePannelDetails")]
        public async Task<GetAuditDetailsResponse> GetAuditSidePannelDetails([FromBody] GetAuditAdministrationRequest request)
        {
            var response = new GetAuditDetailsResponse();

            try
            {


                var Audit = await _audit
                    .Find(x => x.Id == request.AuditId)
                    .FirstOrDefaultAsync();

                if (Audit == null)
                {
                    response.Message = "No updates found for this AuditId.";
                    response.HttpStatusCode = HttpStatusCode.NotFound;
                    response.Success = false;
                    return response;
                }
                var findCustomerRecord = await this._customer.Find(x => x.Id == Audit.ApplicationId).FirstOrDefaultAsync();
                if (findCustomerRecord != null)
                {
                    // 🔹 Fetch assigned audit team (TeamDetails array)
                    var assignedAudit = await _assignaudit
                        .Find(x => x.ApplicationId == Audit.ApplicationId)
                        .FirstOrDefaultAsync();

                    var teamMembers = assignedAudit?.TeamDetails?
                            .Select(t => new TeamMember
                            {
                                Name = t.Name,
                                Role = t.Role
                            })
                            .ToList() ?? new List<TeamMember>();
                    var Data = new List<SidePannaleData>
                    {
                        new SidePannaleData
                        {
                            CompanyName = findCustomerRecord.Orgnization_Name??"",
                            FileNumber = Audit.FileNumber,
                            CustomerApplicationId = findCustomerRecord.Id,
                            AuditId = Audit.Id,
                            Address = this._customersite.Find(x=>x.Fk_Customer_Application==findCustomerRecord.Id).FirstOrDefault()?.Address,
                            fkCertificate = Audit.Fk_CertificateId,
                            AuditStartDate =this._assignaudit.Find(x=>x.AuditId==Audit.Id).FirstOrDefault()?.StartDate,
                            AuditEndDate = this._assignaudit.Find(x=>x.AuditId==Audit.Id).FirstOrDefault()?.EndDate,
                            TeamDetails = teamMembers,
                            AddministrationUrl = "",
                            TechnicalReviewUrl = ""
                        }

                    };
                    // 🔹 Add admin data
                    response.Data = Data;
                    response.Message = "Recent updates retrieved successfully.";
                    response.HttpStatusCode = HttpStatusCode.OK;
                    response.Success = true;
                }
                

                
            }
            catch (Exception ex)
            {
                response.Message = "Error: " + ex.Message;
                response.HttpStatusCode = HttpStatusCode.InternalServerError;
                response.Success = false;
            }

            return response;
        }


    }
}
