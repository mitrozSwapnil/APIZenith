namespace ZenithApp.ZenithMessage
{
    public class gethistoryResponse : BaseResponse
    {
        public List<ReviewerHistoryDto> ReviewerHistory { get; set; } // <-- new
    }
    public class ReviewerHistoryDto
    {
        public string? ApplicationId { get; set; }
        public string? AssignPersonName { get; set; }
        public string? AssignPersonRole { get; set; }
        public string? AssignPersonId { get; set; }
        public DateTime? LatestUpdatedDate { get; set; }
    }
}
