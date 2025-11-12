using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ZenithApp.ZenithMessage
{
    public class tbl_competencyProfile
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string ApplicantId { get; set; }

        // === Personal Info / Part A ===
        public PersonalInfo PersonalInfo { get; set; }
        public List<string> RolesApplied { get; set; }
        public List<string> SchemesApplied { get; set; }
        public List<LanguageProficiency> Languages { get; set; }
        public List<string> DigitalCompetence { get; set; }
        public List<AcademicQualification> AcademicQualifications { get; set; }
        public TrainingRecord Training { get; set; }
        public List<ProductExpertise> ProductExpertise { get; set; }
        public List<AuditorRegistration> AuditorRegistrations { get; set; }
        public List<ExperienceDetails> AuditingExperience { get; set; }
        public List<ExperienceDetails> TechnicalFileReview { get; set; }
        public List<ExperienceDetails> ConsultancyExperience { get; set; }
        public string Achievements { get; set; }
        public List<bool> Enclosures { get; set; }
        public string Signature { get; set; }
        public DateTime? Date { get; set; }

        // === Part B ===
        public string ApplicantName { get; set; }
        public string ApplyingRole { get; set; }
        public string ApplicationType { get; set; }
        public Dictionary<string, Expertise> Expertise { get; set; }
        public List<CompetenceArea> CompetenceAreas { get; set; }
        public string OverallAssessment { get; set; }
        public string Recommendations { get; set; }
        public string AssessorName { get; set; }
        public string AssessorSignature { get; set; }
        public DateTime? PartBDate { get; set; }

        // === Common ===
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; }
        public bool IsDelete { get; set; }
    }

    // === Sub-classes ===
    public class Expertise
    {
        public string Competence { get; set; }
        public string Level { get; set; }
    }

    public class CompetenceArea
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Score { get; set; }
    }
    public class PersonalInfo
    {
        public string Name { get; set; }
        public string Nationality { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string AddressCurrent { get; set; }
        public string AddressPermanent { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string EmploymentType { get; set; }
    }

    public class LanguageProficiency
    {
        public string Name { get; set; }
        public string Understanding { get; set; }
        public string Speaking { get; set; }
        public string Writing { get; set; }
    }

    public class AcademicQualification
    {
        public string Qualification { get; set; }
        public string YearOfCompletion { get; set; }
        public string Duration { get; set; }
        public string Subjects { get; set; }
        public string College { get; set; }
    }

    public class TrainingRecord
    {
        public string Period { get; set; }
        public string Company { get; set; }
        public string BusinessActivity { get; set; }
        public string Designation { get; set; }
        public string Department { get; set; }
        public string Duties { get; set; }
    }

    public class ProductExpertise
    {
        public string Category { get; set; }
        public string Manufacturing { get; set; }
        public string Tests { get; set; }
        public string Standards { get; set; }
        public string NaceRev { get; set; }
        public string TechnicalArea { get; set; }
        public string NdtMvt { get; set; }
    }

    public class AuditorRegistration
    {
        public string Status { get; set; }
        public string Standard { get; set; }
        public string RegistrationBody { get; set; }
        public string IndustrySector { get; set; }
        public bool IsApproved { get; set; }
    }

    public class ExperienceDetails
    {
        public string Description { get; set; }
        public string ReferenceDocument { get; set; }
    }

}
