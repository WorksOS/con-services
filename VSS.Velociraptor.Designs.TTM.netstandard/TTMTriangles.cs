using System;
using System.IO;

namespace VSS.TRex.Designs.TTM
{
    public class TTMTriangles : Triangles
    {
        public TTMTriangles() : base()
        {

        }

        public override Triangle CreateTriangle(TriVertex Vertex1, TriVertex Vertex2, TriVertex Vertex3)
        {
            return new TTMTriangle(Vertex1, Vertex2, Vertex3);
        }


        public void Write(BinaryWriter writer, TTMHeader header)
        {
            NumberTriangles();

            foreach (TTMTriangle triangle in this)
            {
                triangle.Write(writer, header);
            }
        }

        public void Read(BinaryReader reader, TTMHeader header, TriVertices vertices)
        {
            Capacity = header.NumberOfTriangles;

            // Create objects first as we need neighbour triangles to exist
            for (int i = 0; i < header.NumberOfTriangles; i++)
            {
                Add(new TTMTriangle(null, null, null));
            }

            NumberTriangles();

            for (int i = 0; i < Count; i++)
            {
                try
                {
                    long RecPos = reader.BaseStream.Position;
                    (this[i] as TTMTriangle).Read(reader, header, vertices, this, i + 1);
                    reader.BaseStream.Position = RecPos + header.TriangleRecordSize;
                }
                catch (Exception E)
                {
                    throw new Exception(string.Format("Failed to read triangle {0}\n{1}", i + 1, E));
                }
            }
        }
    }
}
