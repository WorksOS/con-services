using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.Velociraptor.Designs.TTM
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

        public void SnapToOutputResolution( TTMHeader header)
        {
            float s;

            if (header.VertexCoordinateSize == sizeof(float))
            {
                s = (float)(X - header.EastingOffsetValue);
                X = s + header.EastingOffsetValue;
                s = (float)(Y - header.NorthingOffsetValue);
                Y = s + header.NorthingOffsetValue;
            }
            if (header.VertexValueSize == sizeof(float))
            {
                s = (float)Z;
                Z = s;
            }
        }
    }
}
