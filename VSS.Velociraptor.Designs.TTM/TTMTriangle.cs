using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.Velociraptor.Designs.TTM
{
    public class TTMTriangle : Triangle
    {
        public TTMTriangle(TriVertex Vertex1, TriVertex Vertex2, TriVertex Vertex3) : base()
        {

        }

        //    {$ifdef LoadTTMIndices}
        //    VertexIndices,
        //      NeighbourIndices: array[TTriangleSide] of Integer;
        //    {$endif}

        public void Write(BinaryWriter writer, TTMHeader header)
        {
            for (int i = 0; i < 3; i++)
            {
                Utilities.WriteInteger(writer, Vertices[i].Tag, header.VertexNumberSize);
            }

            for (int i = 0; i < 3; i++)
            {
                int NeighbourIndex = Neighbours[i] == null ? Consts.NoNeighbour : Neighbours[i].Tag;

                Utilities.WriteInteger(writer, NeighbourIndex, header.TriangleNumberSize);
            }
        }

        public void Read(BinaryReader reader, TTMHeader header, 
        TriVertices vertices, TTMTriangles triangles, int TriNumber )
        {
            for (int i = 0; i < 3; i++)
            {
                int VertIndex = Utilities.ReadInteger(reader, header.VertexNumberSize);

                this.Vertices[i] = (VertIndex < 1 || VertIndex > vertices.Count()) ? null : vertices[VertIndex];

                //{$ifdef LoadTTMIndices}
                //VertexIndices[i] := VertexIndex;
                //{$endif}
            }

            for (int i = 0; i < 3; i++)
            {
                int NeighbourIndex = Utilities.ReadInteger(reader, header.TriangleNumberSize);

                this.Neighbours[i] = (NeighbourIndex < 1 || NeighbourIndex > triangles.Count()) ? null : triangles[NeighbourIndex];

                //{$ifdef LoadTTMIndices}
                //NeighbourIndices[i] := NeighbourIndex;
                //{$endif}
            }
        }
    }
}
