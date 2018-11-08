using System.Drawing;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Extensions;

namespace VSS.Productivity3D.Models.ResultHandling
{
  public class TileResult : ContractExecutionResult
  {
    public byte[] TileData { get; private set; }
    public bool TileOutsideProjectExtents { get; private set; }

    /// <summary>
    /// Constructor with parameters.
    /// </summary>
    public TileResult(byte[] data, bool tileOutsideProjectExtents = false)
    {
      TileData = data;
      TileOutsideProjectExtents = tileOutsideProjectExtents;
    }

    /// <summary>
    /// Creates and returns an empty <see cref="TileResult"/> object, created with the input width and height values.
    /// </summary>
    /// <returns>Returns an empty <see cref="TileResult"/> object.</returns>
    public static TileResult EmptyTile(int width, int height)
    {
      using (Bitmap bitmap = new Bitmap(width, height))
      {
        return new TileResult(bitmap.BitmapToByteArray());
      }
    }
  }
}