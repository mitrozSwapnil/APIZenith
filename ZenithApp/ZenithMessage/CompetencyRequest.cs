namespace ZenithApp.ZenithMessage
{
    public class CompetencyRequest:BaseRequest
    {
        // Step 1
        public string Id { get; set; }
        public string ApplicantId { get; set; }
        public string Name { get; set; }
        public string Nationality { get; set; }
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string AddressCurrent { get; set; }
        public string AddressPermanent { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string EmploymentType { get; set; }
        public List<string> Roles { get; set; }

        // Step 2
        public List<string> Schemes { get; set; }
        public List<LanguageProficiency> Languages { get; set; }
        public List<string> DigitalCompetence { get; set; }

        // Step 3
        public List<AcademicQualification> Qualifications { get; set; }
        public TrainingRecord Training { get; set; }
        public List<ProductExpertise> Products { get; set; }

        // Step 4
        public List<AuditorRegistration> AuditorRegistration { get; set; }
        public List<ExperienceDetails> AuditingExperience { get; set; }
        public List<ExperienceDetails> TechnicalFileReview { get; set; }
        public List<ExperienceDetails> ConsultancyExperience { get; set; }
        public string Achievements { get; set; }

        // Step 5
        public List<bool> Enclosures { get; set; }
        public string Signature { get; set; }
        public string Date { get; set; }

        public string CreatedBy { get; set; }
    }

}
