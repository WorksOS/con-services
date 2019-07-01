using System.IO;

namespace VSS.Hydrology.WebApi.TTM
{
  public class TTMVertices : TriVertices
  {
    protected override TriVertex CreateVertex(double X, double Y, double Z)
    {
      return new TTMVertex(X, Y, Z);
    }

    public void Write(BinaryWriter writer, TTMHeader header)
    {
      foreach (TTMVertex vertex in this)
        vertex.Write(writer, header);
    }

    public void Read(BinaryReader reader, TTMHeader header)
    {
      Capacity = header.NumberOfVertices;

      for (int i = 0; i < header.NumberOfVertices; i++)
      {
        long RecPos = reader.BaseStream.Position;
        TTMVertex Vertex = new TTMVertex(0, 0, 0);
        Add(Vertex);
        Vertex.Read(reader, header);
        reader.BaseStream.Position = RecPos + header.VertexRecordSize;
      }

      NumberVertices();
    }
  }
}
