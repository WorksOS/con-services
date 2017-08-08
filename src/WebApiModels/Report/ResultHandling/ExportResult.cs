using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Contracts;

namespace VSS.Productivity3D.WebApiModels.Report.ResultHandling
{

  public class ExportResult : ContractExecutionResult
  {
    /// <summary>
    /// Private constructor
    /// </summary>
    private ExportResult()
    { }


    public byte[] ExportData { get; private set; }
    public short ResultCode { get; private set; }

    /// <summary>
    /// Create instance of TileResult
    /// </summary>
    public static ExportResult CreateExportDataResult(byte[] data, short resultCode)
    {
      return new ExportResult
             {
                 ExportData = data,
                 ResultCode = resultCode
             };
    }

    /// <summary>
    /// Create example instance of TileResult to display in Help documentation.
    /// </summary>
    public static ExportResult HelpSample
    {
      get
      {
        return new ExportResult
        {
               };
      }
    }
  }
}