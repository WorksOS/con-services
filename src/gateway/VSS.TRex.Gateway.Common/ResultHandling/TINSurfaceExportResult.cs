using System.IO;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.TRex.Gateway.Common.ResultHandling
{
  public class TINSurfaceExportResult : ContractExecutionResult
  {
    public byte[] TINData { get; private set; }

    /// <summary>
    /// Create instance of TileResult
    /// </summary>
    public static TINSurfaceExportResult CreateTINResult(byte[] data)
    {
      return new TINSurfaceExportResult
      { 
        TINData = data
      };
    }
  }
}

