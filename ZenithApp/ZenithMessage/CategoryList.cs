namespace ZenithApp.ZenithMessage
{
    public class CategoryList
    {
        public string? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? CategoryCode { get; set; }
        public string? CategoryDescription { get; set; }
        public bool? IsActive { get; set; } = true;
        public int? ActiveState { get; set; } = 1;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
