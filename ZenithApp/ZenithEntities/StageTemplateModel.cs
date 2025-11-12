namespace ZenithApp.ZenithEntities
{
    public class StageTemplateModel
    {
        public string AuditStage { get; set; }
        public List<StepTemplateModel> Steps { get; set; } = new List<StepTemplateModel>();
    }
}
