using VSS.MasterData.Models.ResultHandling.Abstractions;
using Draw = System.Drawing;

namespace VSS.TRex.Gateway.Common.ResultHandling
{
  public class TileResult : ContractExecutionResult
  {
    public Draw.Bitmap TileData { get; private set; }

    /// <summary>
    /// Create instance of TileResult
    /// </summary>
    public static TileResult CreateTileResult(Draw.Bitmap data)
    {
      return new TileResult
      {
        TileData = data,
      };
    }

    /// <summary>
    /// Creates and returns an empty <see cref="TileResult"/> object, created with the input width and height values.
    /// </summary>
    /// <returns>Returns an empty <see cref="TileResult"/> object.</returns>
    public static TileResult EmptyTile(int width, int height)
    {
      using (Draw.Bitmap bitmap = new Draw.Bitmap(width, height))
      {
        return CreateTileResult(bitmap);
      }
    }
  }
}
