using System;
using System.Collections.Generic;
using System.IO;

namespace VSS.TRex.Designs.TTM
{
    public class TTMEdges : List<TTMTriangle>
    {
        //    {$ifdef LoadTTMIndices}
        //    TriangleIndices: array of Integer;
        //    {$endif

        public void Write(BinaryWriter writer, TTMHeader header)
        {
            // Assume triangles have been numbered
            for (int i = 0; i < Count; i++)
            {
                Utilities.WriteInteger(writer, this[i].Tag, header.TriangleNumberSize);
            }
        }

        public void Read(BinaryReader reader, TTMHeader header, Triangles triangles)
        {
            //{$ifdef LoadTTMIndices}
            //SetLength(TriangleIndices, Header.NumberOfEdgeRecords);
            //{$endif}

            Capacity = header.NumberOfEdgeRecords;

            for (int i = 0; i < header.NumberOfEdgeRecords; i++)
            {
                try
                {
                    long RecPos = reader.BaseStream.Position;
                    int TriangleIndex = Utilities.ReadInteger(reader, header.TriangleNumberSize);

                    Add(TriangleIndex < 1 || TriangleIndex > triangles.Count ? null : triangles[TriangleIndex - 1] as TTMTriangle);

                    //{$ifdef LoadTTMIndices}
                    //TriangleIndices[i] = TriangleIndex;
                    //{$endif}
                    reader.BaseStream.Position = RecPos + header.EdgeRecordSize;
                }
                catch (Exception E)
                {
                      throw new Exception(string.Format("Failed to read edge {0}\n{1}", i + 1, E));
                }
            }       
        }

        public int AddTriangle(TTMTriangle Triangle)
        {
            Add(Triangle);
            return Count - 1;
        }
    }
}
