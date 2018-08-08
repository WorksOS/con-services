using System.IO;

namespace VSS.TRex.Designs.TTM.Optimised
{
    /// <summary>
    /// Describes a triangle in the TIN mesh
    /// </summary>
    public struct Triangle
    {
      /// <summary>
      /// A 'tag' used for various purposes in TTM processing
      /// </summary>
      public int Tag;

      public int Vertex0, Vertex1, Vertex2;

//        public double Area() => XYZ.GetTriArea(Vertex0.XYZ, Vertex1.XYZ, Vertex2.XYZ);
//        public double GetHeight(double X, double Y) => XYZ.GetTriangleHeight(Vertex0.XYZ, Vertex1.XYZ, Vertex2.XYZ, X, Y );

      public void Read(BinaryReader reader, TTMHeader header)
      {
        Vertex0 = Utilities.ReadInteger(reader, header.VertexNumberSize);
        Vertex1 = Utilities.ReadInteger(reader, header.VertexNumberSize);
        Vertex2 = Utilities.ReadInteger(reader, header.VertexNumberSize);

        // This loop does not need to be executed since the reader repositions the reading location after each serialise in
        //for (int i = 0; i < 3; i++)
        //{
        //  int NeighbourIndex = Utilities.ReadInteger(reader, header.TriangleNumberSize);
        // SetNeighbour(i, (NeighbourIndex < 1 || NeighbourIndex > triangles.Items.Length) ? null : triangles.Items[NeighbourIndex - 1]);
        //}
      }
  }
}
