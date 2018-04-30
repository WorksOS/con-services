using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.MasterData.Models.ResultHandling
{
  /// <summary>
  /// Generic container for Export binary data.
  /// </summary>
  /// <seealso cref="ContractExecutionResult" />
  public class ExportResult : ContractExecutionResult
  {
    /// <summary>
    /// Gets or sets the export data.
    /// </summary>
    /// <value>
    /// The export data.
    /// </value>
    public byte[] ExportData { get; set; }

    /// <summary>
    /// Gets or sets the result code.
    /// </summary>
    /// <value>
    /// The result code.
    /// </value>
    public short ResultCode { get; set; }
  }
}