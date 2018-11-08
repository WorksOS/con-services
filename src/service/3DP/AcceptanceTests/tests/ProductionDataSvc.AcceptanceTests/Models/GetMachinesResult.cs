namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class GetMachinesResult : ResponseBase
  {
    public MachineStatus[] MachineStatuses { get; set; }

    public GetMachinesResult()
      : base("success")
    { }

    public GetMachinesResult(MachineStatus[] statuses, int code = 0, string message = "success")
      : base(code, message)
    {
      MachineStatuses = statuses;
    }
  }
}
