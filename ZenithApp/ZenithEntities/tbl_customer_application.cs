using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ZenithApp.ZenithEntities
{
    public class tbl_customer_application
    {
       
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string ApplicationId { get; set; }

        public string? Orgnization_Name { get; set; }

        public string? Constituation_of_Orgnization { get; set; }

        public string? EmailId { get; set; }
        public string? status { get; set; }

        //[BsonRepresentation(BsonType.ObjectId)]
        //public string? Fk_ApplicationCertificate { get; set; }

        //[BsonRepresentation(BsonType.ObjectId)]
        //public string? Fk_Product_certificate { get; set; }

        //[BsonRepresentation(BsonType.ObjectId)]
        //public string? Fk_key_personnel { get; set; }

        //[BsonRepresentation(BsonType.ObjectId)]
        //public string? Fk_customer_site { get; set; }

        //[BsonRepresentation(BsonType.ObjectId)]
        //public string? Fk_Entity { get; set; }

        //[BsonRepresentation(BsonType.ObjectId)]
        //public string? FK_Outsource_Process { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? Expected_Audit_Date { get; set; }

        public string? Holiday { get; set; }
        public int? Fk_ApplicationStatus { get; set; }

        public string? Audit_Language { get; set; }

        [BsonElement("ConsultantName")]
        public string? ConsultantName { get; set; }

        public string? FileName { get; set; }

        public string? Name { get; set; }

        public string? Contact_details { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? Datetime { get; set; }

        public string? Designation { get; set; }
        public int? ActiveState { get; set; } = 1;

        
        public string? Fk_Annaxture { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CreatedAt { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? UpdatedAt { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? SubmitDate { get; set; }

        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public bool? IsDelete { get; set; }
        public bool? IsFinalSubmit { get; set; }




       

       public List<OutsourceProcessItem> OutsourceProcess { get; set; }

    }
    public class OutsourceProcessItem
    {
        public string Process { get; set; }
        public string Sub_contractorName { get; set; }
        public string Location { get; set; }
        public string controll_establish_level { get; set; }
    }



}

