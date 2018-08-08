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
    /// A 'tag' used for various purposes in TTM processing
    /// </summary>
    public int Tag;

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
    /// Converts the location of the vertex to a string
    /// </summary>
    /// <returns></returns>
    public string AsText()
    {
      return $"Tag:{Tag}, X={X:F3}, Y={Y:F3}, Z={Z:F3}";
    }

    /// <summary>
    /// Overridden ToString()
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return AsText();
    }

    public void Read(BinaryReader reader, TTMHeader header)
    {
      Y = Utilities.ReadFloat(reader, header.VertexCoordinateSize) + header.NorthingOffsetValue;
      X = Utilities.ReadFloat(reader, header.VertexCoordinateSize) + header.EastingOffsetValue;
      Z = Utilities.ReadFloat(reader, header.VertexValueSize);
    }
  }
}
