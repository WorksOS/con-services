namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class LayerIdsExecutionResult : ResponseBase
  {
    public LayerIdDetails[] LayerIdDetailsArray { get; set; }

    public LayerIdsExecutionResult()
        : base("success")
    { }
  }
}
