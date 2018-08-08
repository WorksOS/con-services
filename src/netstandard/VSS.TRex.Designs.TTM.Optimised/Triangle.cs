using System.IO;

namespace VSS.TRex.Designs.TTM.Optimised
{
    /// <summary>
    /// Describes a triangle in the TIN mesh
    /// </summary>
    public struct Triangle
    {
      public int Vertex0, Vertex1, Vertex2;

      public void Read(BinaryReader reader, TTMHeader header)
      {
        if (header.VertexNumberSize == sizeof(short))
        {
          Vertex0 = reader.ReadInt16() - 1;
          Vertex1 = reader.ReadInt16() - 1;
          Vertex2 = reader.ReadInt16() - 1;
        }
        else
        {
          Vertex0 = reader.ReadInt32() - 1;
          Vertex1 = reader.ReadInt32() - 1;
          Vertex2 = reader.ReadInt32() - 1;
        }

        // This loop does not need to be executed since the reader repositions the reading location after each serialise in
        //for (int i = 0; i < 3; i++)
        //{
        //  int NeighbourIndex = Utilities.ReadInteger(reader, header.TriangleNumberSize);
        // SetNeighbour(i, (NeighbourIndex < 1 || NeighbourIndex > triangles.Items.Length) ? null : triangles.Items[NeighbourIndex - 1]);
        //}
      }
  }
}
