using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.TRex.Gateway.Common.ResultHandling
{
  public class ReportGridDataResult : ContractExecutionResult
  {
    public byte[] GriddedData { get; private set; }

    /// <summary>
    /// Constructor with a parameter.
    /// </summary>
    public ReportGridDataResult(byte[] griddedData)
    {
      GriddedData = griddedData;
    }
  }
}
