using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.TRex.Gateway.Common.ResultHandling
{
  public class CSVExportResult : ContractExecutionResult
  {
    public byte[] CSVData { get; private set; }

    /// <summary>
    /// Constructor with a parameter.
    /// </summary>
    public CSVExportResult(byte[] csvData)
    {
      CSVData = csvData;
    }
  }
}
