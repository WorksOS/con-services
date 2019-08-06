using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.ResultHandling
{
  public class QMTileResult : ContractExecutionResult
  {
    public byte[] TileData { get; }

    /// <summary>
    /// Constructor with parameters.
    /// </summary>
    public QMTileResult(byte[] data)
    {
      TileData = data;
    }
  }
}
