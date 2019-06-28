using System.IO;
using VSS.Hydrology.WebApi.Common.TTM;

namespace VSS.Hydrology.WebApi.TTM
{
  public class TTMVertex : TriVertex
  {
    public TTMVertex(double aX, double aY, double aZ) : base(aX, aY, aZ)
    {
    }

    public void Write(BinaryWriter writer, TTMHeader header)
    {
      Utilities.WriteFloat(writer, Y - header.NorthingOffsetValue, header.VertexCoordinateSize);
      Utilities.WriteFloat(writer, X - header.EastingOffsetValue, header.VertexCoordinateSize);
      Utilities.WriteFloat(writer, Z, header.VertexValueSize);
    }

    public void Read(BinaryReader reader, TTMHeader header)
    {
      Y = Utilities.ReadFloat(reader, header.VertexCoordinateSize) + header.NorthingOffsetValue;
      X = Utilities.ReadFloat(reader, header.VertexCoordinateSize) + header.EastingOffsetValue;
      Z = Utilities.ReadFloat(reader, header.VertexValueSize);
    }
  }
}
