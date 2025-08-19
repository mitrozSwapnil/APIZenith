namespace ZenithApp.ZenithMessage
{
    public class FieldCommentRequest : BaseRequest
    {
        public string? ApplicationId { get; set; }
        public string? FieldName { get; set; }
        public string? CommentText { get; set; }
    }
}
