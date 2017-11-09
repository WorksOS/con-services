﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.Velociraptor.Designs.TTM
{
    public class TTMVertices : TriVertices
    {
        protected override TriVertex CreateVertex(double X, double Y, double Z)
        {
            return new TTMVertex(X, Y, Z);
        }

        // public

        public void Write(BinaryWriter writer, TTMHeader header)
        {
            foreach (TTMVertex vertex in this)
            {
                vertex.Write(writer, header);
            }
        }

        public void Read(BinaryReader reader, TTMHeader header)
        {
            Capacity = header.NumberOfVertices;
            for (int i = 0; i < header.NumberOfVertices + 1; i++)
                try
                {
                    long RecPos = reader.BaseStream.Position;
                    TTMVertex Vert = new TTMVertex(0, 0, 0);
                    Add(Vert);
                    Vert.Read(reader, header);
                    reader.BaseStream.Position = RecPos + header.VertexRecordSize;
                }
                catch (Exception E)
                {
                    throw new Exception(string.Format("Failed to read vertex {0}\n{1}", i + 1, E));
                }

            NumberVertices();
        }

        public void SnapToOutputResolution(TTMHeader header)
        {
            if (header.VertexCoordinateSize != sizeof(double) || header.VertexValueSize != sizeof(double))
            {
                foreach (TTMVertex vertex in this)
                    vertex.SnapToOutputResolution(header);
            }
        }
    }
}
