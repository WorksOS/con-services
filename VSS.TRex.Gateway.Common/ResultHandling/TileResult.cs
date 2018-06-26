using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.TRex.Rendering.Implementations.Core2;

namespace VSS.TRex.Gateway.Common.ResultHandling
{
  public class TileResult : ContractExecutionResult
  {
    public Bitmap TileData { get; private set; }

    /// <summary>
    /// Create instance of TileResult
    /// </summary>
    public static TileResult CreateTileResult(Bitmap data)
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
      using (Bitmap bitmap = new Bitmap(width, height))
      {
        return CreateTileResult(bitmap);
      }
    }
  }
}
