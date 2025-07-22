namespace ZenithApp.ZenithMessage
{
    public class CustomerDashboardData
    {
        public string? Id { get; set; }
        public string? ApplicationId { get; set; }
        public string? CompanyName { get; set; }
        public string? Certification_Name { get; set; }
        public string? Certification_Id{ get; set; }
        public string? Status { get; set; }
        public DateTime? ReceiveDate { get; set; }
        public string? AssignedUserName { get; set; }
        //public List<UserDropdownDto>? UserList { get; set; } // for dropdown when not assigned




    }
}
