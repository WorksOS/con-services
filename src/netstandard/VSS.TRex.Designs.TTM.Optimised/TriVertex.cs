using System.IO;
using VSS.TRex.Geometry;

namespace VSS.TRex.Designs.TTM.Optimised
{
  /// <summary>
  /// Implements a vertex at the corner of triangles in the TTM mesh
  /// </summary>
  public struct TriVertex
  {
    /// <summary>
    /// Gets the X, Y, Z location of the vertex as a XYZ instance
    /// </summary>
    /// <returns></returns>
    private XYZ GetXYZ()
    {
      return new XYZ(X, Y, Z);
    }

    /// <summary>
    /// Sets the location of the vertex from a XYZ instance
    /// </summary>
    /// <param name="Value"></param>
    private void SetXYZ(XYZ Value)
    {
      X = Value.X;
      Y = Value.Y;
      Z = Value.Z;
    }

    /// <summary>
    /// The X ordinate location of the vertex
    /// </summary>
    public double X;

    /// <summary>
    /// The Y ordinate location of the vertex
    /// </summary>
    public double Y;

    /// <summary>
    /// The Z ordinate location of the vertex
    /// </summary>    
    public double Z;

    /// <summary>
    /// Property representing the X, Y, Z location of this vertex as a XYZ instance
    /// </summary>
    public XYZ XYZ
    {
      get { return GetXYZ(); }
      set { SetXYZ(value); }
    }

    /// <summary>
    /// Overridden ToString()
    /// </summary>
    /// <returns></returns>
    public override string ToString() => $"X={X:F3}, Y={Y:F3}, Z={Z:F3}";

    public void Read(BinaryReader reader, TTMHeader header)
    {
      if (header.VertexCoordinateSize == sizeof(float))
      {
        Y = reader.ReadSingle() + header.NorthingOffsetValue;
        X = reader.ReadSingle() + header.EastingOffsetValue;
      }
      else
      {
        Y = reader.ReadDouble() + header.NorthingOffsetValue;
        X = reader.ReadDouble() + header.EastingOffsetValue;
      }

      Z = header.VertexValueSize == sizeof(float) ? reader.ReadSingle() : reader.ReadDouble();
    }
  }
}
