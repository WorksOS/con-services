using System.Drawing;
using ASNodeDecls;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Extensions;

namespace VSS.Productivity3D.Common.ResultHandling
{
  public class TileResult : ContractExecutionResult
  {
    public byte[] TileData { get; private set; }
    public bool TileOutsideProjectExtents { get; private set; }

    /// <summary>
    /// Create instance of TileResult
    /// </summary>
    public static TileResult CreateTileResult(byte[] data, TASNodeErrorStatus raptorResult)
    {
      return new TileResult
      {
        TileData = data,
        TileOutsideProjectExtents = raptorResult != TASNodeErrorStatus.asneOK
      };
    }

    /// <summary>
    /// Creates and returns an empty <see cref="TileResult"/> object, created with the input width and height values.
    /// </summary>
    /// <returns>Returns an empty <see cref="TileResult"/> object.</returns>
    public static TileResult EmptyTile(int width, int height)
    {
      using (Bitmap bitmap = new Bitmap(width, height))
      {
        return CreateTileResult(bitmap.BitmapToByteArray(), TASNodeErrorStatus.asneOK);
      }
    }
  }
}