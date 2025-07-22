namespace ZenithApp.Models
{
    public class tbl_user
    {
        public string? FullName { get; set; }
        public string? UserName{ get; set; }
        public string? Password{ get; set; }
        public string? EmailId{ get; set; }
        public string? Department{ get; set; }
        public string? ContactNo{ get; set; }
        public string? Address{ get; set; }
        public string? Gender { get; set; }
        public int? Fk_RoleID { get; set; }
        public int? Fk_Otp{ get; set; }
        public int? IsDelete{ get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt{ get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy{ get; set; }
    }
}
