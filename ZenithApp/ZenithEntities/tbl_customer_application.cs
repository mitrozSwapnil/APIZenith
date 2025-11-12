using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using ZenithApp.ZenithMessage;
using static ZenithApp.ZenithRepository.AdminRepository;

namespace ZenithApp.ZenithEntities
{
    //public class tbl_customer_application
    //{

    //    [BsonId]
    //    [BsonRepresentation(BsonType.ObjectId)]
    //    public string Id { get; set; }

    //    public string ApplicationId { get; set; }

    //    public string? Orgnization_Name { get; set; }

    //    public string? Constituation_of_Orgnization { get; set; }

    //    public string? EmailId { get; set; }
    //    public string? status { get; set; }
    //    public string? Expected_Audit_Date { get; set; }

    //    public List<string>? Holiday { get; set; }
    //    public int? Fk_ApplicationStatus { get; set; }

    //    public string? Audit_Language { get; set; }

    //    [BsonElement("ConsultantName")]
    //    public string? ConsultantName { get; set; }

    //    public string? FileName { get; set; }

    //    public string? Name { get; set; }

    //    public string? Contact_details { get; set; }

    //    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    //    public DateTime? Datetime { get; set; }

    //    public string? Designation { get; set; }
    //    public int? ActiveState { get; set; } = 1;


    //    public List<AnnexureList>? Fk_Annaxture { get; set; }
    //    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    //    public DateTime? CreatedAt { get; set; }

    //    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    //    public DateTime? UpdatedAt { get; set; }

    //    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    //    public DateTime? SubmitDate { get; set; }

    //    public string? CreatedBy { get; set; }
    //    public string? UpdatedBy { get; set; }
    //    public bool? IsDelete { get; set; }
    //    public bool? IsFinalSubmit { get; set; }






    //   public List<OutsourceProcessItem> OutsourceProcess { get; set; }

    //}
    //public class OutsourceProcessItem
    //{
    //    public string? Process { get; set; }
    //    public string? Sub_contractorName { get; set; }
    //    public string? Location { get; set; }
    //    public string? controll_establish_level { get; set; }
    //}

    public class tbl_customer_application
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string ApplicationName { get; set; }
        public string ApplicationId { get; set; }

        public string Orgnization_Name { get; set; }
        public string Constituation_of_Orgnization { get; set; }
        public string EmailId { get; set; }

        public string Expected_Audit_Date { get; set; }
        public List<string>? Holiday { get; set; }
        public string Audit_Language { get; set; }
        public string ConsultantName { get; set; }
        public string FileName { get; set; }
        public string Name { get; set; }
        public string Contact_details { get; set; }
        public DateTime? Datetime { get; set; }
        public string Designation { get; set; }

      //  public List<AnnexureList>? Fk_Annaxture { get; set; }
        public List<ApplicationCertificate>? Fk_ApplicationCertificates { get; set; }
        public List<string>? Fk_Product_Certificates { get; set; }

        public List<KeyPersonnel>? Fk_Key_Personnels { get; set; }
        public List<CustomerSite>? Fk_Customer_Sites { get; set; }
        public List<CustomerEntity>? Fk_Customer_Entity { get; set; }

        public List<OutsourceProcessItem>? OutsourceProcess { get; set; }

        public int ActiveState { get; set; }
        public bool? IsFinalSubmit { get; set; }
        public DateTime? SubmitDate { get; set; }

        public bool IsDelete { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public string status { get; set; }
    }

    public class AnnexureList
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        public string CertificateId { get; set; }
        public string SubCertificateId { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
    }

    public class ApplicationCertificate
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        public string? Certificate_Name { get; set; }
        public bool Is_Delete { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Status { get; set; }
        public string? CertificationType { get; set; }
        public List<AssignedUser>? AssignTo { get; set; }   // [{ userId, role }]
        public DateTime? TargetDate { get; set; }
        public List<SubTypeItem>? SubType { get; set; }
    }

    public class SubTypeItem
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        public string Scheme { get; set; }
        public string Annexure { get; set; }
        public string Url { get; set; }
    }

    public class KeyPersonnel
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        public string Name { get; set; }
        public string Designation { get; set; }
        public string EmailId { get; set; }
        public string Contact_No { get; set; }
        public string Type { get; set; }
    }

    public class CustomerSite
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        public string Address { get; set; }
        public string Telephone { get; set; }
        public string Web { get; set; }
        public string Email { get; set; }
        public string Activity_Department { get; set; }
        public string Manpower { get; set; }
        public string Shift_Name { get; set; }
    }

    public class CustomerEntity
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        public string Name_of_Entity { get; set; }
        public string Identification_Number { get; set; }
        public string File { get; set; }
    }

    public class OutsourceProcessItem
    {
        public string Process { get; set; }
        public string Sub_contractorName { get; set; }
        public string Location { get; set; }
        public string controll_establish_level { get; set; }
    }

}

