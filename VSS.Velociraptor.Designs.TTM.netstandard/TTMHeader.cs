using System.IO;
using System.Runtime.InteropServices;

namespace VSS.Velociraptor.Designs.TTM
{

    public static class HeaderConsts
    {
        public const int kDTMInternalModelNameSize = 32;
        public const int kDTMFileSignatureSize = 20;

//    DTMInternalModelName = array[1..kDTMInternalModelNameSize] of Byte;
//    DTMFileSignature = array[1..kDTMFileSignatureSize] of Byte;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TTMHeader
    {
        public byte FileMajorVersion; // Must be 1
        public byte FileMinorVersion; // Must be 0

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = HeaderConsts.kDTMFileSignatureSize)]
        public byte[] FileSignature; // = new byte[HeaderConsts.kDTMFileSignatureSize]; // Must be "TNL TIN DTM FILE" \0\0\0\0

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = HeaderConsts.kDTMInternalModelNameSize)]
        public byte[] DTMModelInternalName; // = new byte[HeaderConsts.kDTMInternalModelNameSize]; // E.g. "Christchurch area TIN DTM" \0

        public byte CoordinateUnits; // Must be 1: Metres
        public byte VertexValueUnits; // Must be 1: Metres
        public byte InterpolationMethod;// Must be 1: Linear
        public double NorthingOffsetValue; // e.g. 700500. Typically the centre of the area covered by the TIN
        public double EastingOffsetValue; // E.g. 300500. Typically the centre of the area covered by the TIN
        public double MinimumNorthing; // E.g. 700000
        public double MinimumEasting; // E.g. 300000
        public double MaximumNorthing; // E.g. 701000
        public double MaximumEasting; // E.g. 301000
        public byte VertexCoordinateSize; // 4: Single precision, 8: Double precision
        public byte VertexValueSize; // 4: Single precision, 8: Double precision
        public byte VertexNumberSize; // 2: Short integer, 4: Long integer
        public byte TriangleNumberSize; // 2: Short integer, 4: Long integer
        public int StartOffsetOfVertices;
        public int NumberOfVertices;
        public short VertexRecordSize; // Typically 2 x (size of vertex coordinate) + (size of vertex value) but could be larger.
        public int StartOffsetOfTriangles;
        public int NumberOfTriangles;
        public short TriangleRecordSize; // Typically 3 x (vertex number size) + 3 x (neighbour field size) but could be larger.
        public int StartOffsetOfEdgeList;
        public int NumberOfEdgeRecords;
        public short EdgeRecordSize; // Typically 1 x (size of triangle field size) but could be larger.
        public int StartOffsetOfStartPoints;
        public int NumberOfStartPoints; // 50 or less
        public short StartPointRecordSize; // Typically 2 x (vertex coordinate size) + (size of triangle field) but could be larger.

        public static TTMHeader NewHeader()
        {
            return new TTMHeader()
            {
                FileSignature = new byte[HeaderConsts.kDTMFileSignatureSize],
                DTMModelInternalName = new byte[HeaderConsts.kDTMInternalModelNameSize]
            };
        }

        public void Read(BinaryReader reader)
        {
            FileMajorVersion = reader.ReadByte(); // Must be 1
            FileMinorVersion = reader.ReadByte(); // Must be 0
            reader.Read(FileSignature, 0, HeaderConsts.kDTMFileSignatureSize); // Must be "TNL TIN DTM FILE" \0\0\0\0
            reader.Read(DTMModelInternalName, 0, HeaderConsts.kDTMInternalModelNameSize); // Must be "TNL TIN DTM FILE" \0\0\0\0
            CoordinateUnits = reader.ReadByte(); // Must be 1: Metres
            VertexValueUnits = reader.ReadByte(); // Must be 1: Metres
            InterpolationMethod = reader.ReadByte();// Must be 1: Linear
            NorthingOffsetValue = reader.ReadDouble(); // e.g. 700500. Typically the centre of the area covered by the TIN
            EastingOffsetValue = reader.ReadDouble(); // E.g. 300500. Typically the centre of the area covered by the TIN
            MinimumNorthing = reader.ReadDouble(); // E.g. 700000
            MinimumEasting = reader.ReadDouble(); // E.g. 300000
            MaximumNorthing = reader.ReadDouble(); // E.g. 701000
            MaximumEasting = reader.ReadDouble(); // E.g. 301000
            VertexCoordinateSize = reader.ReadByte(); // 4: Single precision, 8: Double precision
            VertexValueSize = reader.ReadByte(); // 4: Single precision, 8: Double precision
            VertexNumberSize = reader.ReadByte(); // 2: Short integer, 4: Long integer
            TriangleNumberSize = reader.ReadByte(); // 2: Short integer, 4: Long integer
            StartOffsetOfVertices = reader.ReadInt32();
            NumberOfVertices = reader.ReadInt32();
            VertexRecordSize = reader.ReadInt16(); // Typically 2 x (size of vertex coordinate) + (size of vertex value) but could be larger.
            StartOffsetOfTriangles = reader.ReadInt32();
            NumberOfTriangles = reader.ReadInt32();
            TriangleRecordSize = reader.ReadInt16(); // Typically 3 x (vertex number size) + 3 x (neighbour field size) but could be larger.
            StartOffsetOfEdgeList = reader.ReadInt32();
            NumberOfEdgeRecords = reader.ReadInt32();
            EdgeRecordSize = reader.ReadInt16(); // Typically 1 x (size of triangle field size) but could be larger.
            StartOffsetOfStartPoints = reader.ReadInt32();
            NumberOfStartPoints = reader.ReadInt32(); // 50 or less
            StartPointRecordSize = reader.ReadInt16(); // Typically 2 x (vertex coordinate size) + (size of triangle field) but could be larger.
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(FileMajorVersion); // Must be 1
            writer.Write(FileMinorVersion); // Must be 0
            writer.Write(FileSignature, 0, HeaderConsts.kDTMFileSignatureSize); // Must be "TNL TIN DTM FILE" \0\0\0\0
            writer.Write(DTMModelInternalName, 0, HeaderConsts.kDTMInternalModelNameSize); // E.g. "Christchurch area TIN DTM" \0
            writer.Write(CoordinateUnits); // Must be 1: Metres
            writer.Write(VertexValueUnits); // Must be 1: Metres
            writer.Write(InterpolationMethod);// Must be 1: Linear
            writer.Write(NorthingOffsetValue); // e.g. 700500. Typically the centre of the area covered by the TIN
            writer.Write(EastingOffsetValue); // E.g. 300500. Typically the centre of the area covered by the TIN
            writer.Write(MinimumNorthing); // E.g. 700000
            writer.Write(MinimumEasting); // E.g. 300000
            writer.Write(MaximumNorthing); // E.g. 701000
            writer.Write(MaximumEasting); // E.g. 301000
            writer.Write(VertexCoordinateSize); // 4: Single precision, 8: Double precision
            writer.Write(VertexValueSize); // 4: Single precision, 8: Double precision
            writer.Write(VertexNumberSize); // 2: Short integer, 4: Long integer
            writer.Write(TriangleNumberSize); // 2: Short integer, 4: Long integer
            writer.Write(StartOffsetOfVertices);
            writer.Write(NumberOfVertices);
            writer.Write(VertexRecordSize); // Typically 2 x (size of vertex coordinate) + (size of vertex value) but could be larger.
            writer.Write(StartOffsetOfTriangles);
            writer.Write(NumberOfTriangles);
            writer.Write(TriangleRecordSize); // Typically 3 x (vertex number size) + 3 x (neighbour field size) but could be larger.
            writer.Write(StartOffsetOfEdgeList);
            writer.Write(NumberOfEdgeRecords);
            writer.Write(EdgeRecordSize); // Typically 1 x (size of triangle field size) but could be larger.
            writer.Write(StartOffsetOfStartPoints);
            writer.Write(NumberOfStartPoints); // 50 or less
            writer.Write(StartPointRecordSize); // Typically 2 x (vertex coordinate size) + (size of triangle field) but could be larger.
        }
}
}
