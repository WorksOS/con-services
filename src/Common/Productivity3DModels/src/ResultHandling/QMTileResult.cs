using System.Drawing;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Extensions;

namespace VSS.Productivity3D.Models.ResultHandling
{
  public class QMTileResult : ContractExecutionResult
  {
    public byte[] TileData { get; private set; }

    /// <summary>
    /// Constructor with parameters.
    /// </summary>
    public QMTileResult(byte[] data)
    {
      TileData = data;
    }
    
  }
}
