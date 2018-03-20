using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.WebApi.Models.Report.ResultHandling
{
  public class ExportResult : ContractExecutionResult
  {
    public byte[] ExportData { get; private set; }
    public short ResultCode { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private ExportResult()
    { }

    /// <summary>
    /// Create instance of TileResult
    /// </summary>
    public static ExportResult Create(byte[] data, short resultCode)
    {
      return new ExportResult
      {
        ExportData = data,
        ResultCode = resultCode
      };
    }
  }
}