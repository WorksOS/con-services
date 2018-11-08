using System.Collections.Generic;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class GetMachineDesignResult : ResponseBase
  {
    public List<DesignName> designs { get; set; }

    public GetMachineDesignResult()
        : base("success")
    { }
  }
}
