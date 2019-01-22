using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.TRex.Gateway.Common.ResultHandling
{
  public class GriddedReportDataResult : ContractExecutionResult
  {
    public byte[] GriddedData { get; private set; }

    /// <summary>
    /// Constructor with a parameter.
    /// </summary>
    public GriddedReportDataResult(byte[] griddedData)
    {
      GriddedData = griddedData;
    }
  }
}
