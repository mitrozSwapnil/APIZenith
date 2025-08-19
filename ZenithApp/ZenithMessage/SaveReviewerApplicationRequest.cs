namespace ZenithApp.ZenithMessage
{
    public class SaveReviewerApplicationRequest
    {
        public string CertificationName { get; set; }                  // "ISO" | ...
        public string ApplicationId { get; set; }                      // review target (base table Id)
        public Dictionary<string, object> Fields { get; set; }         // changed fields payload
        public Dictionary<string, string> ChangeReasons { get; set; }  // reason per field (when overwriting other reviewer)
        public bool? IsFinalSubmit { get; set; }                       // optional
        public int? ExpectedVersion { get; set; }                      // optional optimistic concurrency
    }
}
