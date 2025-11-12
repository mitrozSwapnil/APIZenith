namespace ZenithApp.ZenithMessage
{
    public class GetAuditTechnicalReviewResponse:BaseResponse
    {
        public tbl_audit_administration_technical Data { get; set; }
        public List<AuditList> auditList { get; set; }
    }

}
