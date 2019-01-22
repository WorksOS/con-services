using Apache.Ignite.Core.Binary;
using VSS.TRex.Common.Interfaces;

namespace VSS.TRex.Common.Types
{
  /// <summary>
  /// AreaControlSet contains a collection of tuning parameters that relate to how to relate production data
  /// cells to pixels when rendering tiles.
  /// </summary>
  public class AreaControlSet : IFromToBinary
  {
    public bool UseIntegerAlgorithm;
    public double PixelXWorldSize;
    public double PixelYWorldSize;
    public double UserOriginX;
    public double UserOriginY;
    public double Rotation;

    public AreaControlSet(
      bool useIntegerAlgorithm,
      double pixelXWorldSize,
      double pixelYWorldSize,
      double userOriginX,
      double userOriginY,
      double rotation
      )
    {
      UseIntegerAlgorithm = useIntegerAlgorithm;
      PixelXWorldSize = pixelXWorldSize;
      PixelYWorldSize = pixelYWorldSize;
      UserOriginX = userOriginX;
      UserOriginY = userOriginY;
      Rotation = rotation;
    }

    public AreaControlSet()
    {
    }

    public static AreaControlSet CreateAreaControlSet()
    {
      return new AreaControlSet(true, 0, 0, 0, 0, 0);
    }

    public void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteBoolean(UseIntegerAlgorithm);
      writer.WriteDouble(PixelXWorldSize);
      writer.WriteDouble(PixelYWorldSize);
      writer.WriteDouble(UserOriginX);
      writer.WriteDouble(UserOriginY);
      writer.WriteDouble(Rotation);
    }

    public void FromBinary(IBinaryRawReader reader)
    {
      UseIntegerAlgorithm = reader.ReadBoolean();
      PixelXWorldSize = reader.ReadDouble();
      PixelYWorldSize = reader.ReadDouble();
      UserOriginX = reader.ReadDouble();
      UserOriginY = reader.ReadDouble();
      Rotation = reader.ReadDouble();
    }
  }
}
