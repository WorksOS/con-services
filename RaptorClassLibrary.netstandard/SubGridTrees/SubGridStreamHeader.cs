using System;
using System.IO;
using System.Linq;

namespace VSS.VisionLink.Raptor.SubGridTrees
{
    /// <summary>
    /// Defines the header written at the start of a stream containing subgrid informatation, either subgrid directory or subgrid segment.
    /// </summary>
    public class SubGridStreamHeader
    {
        public const int kSubGridHeaderFlag_IsSubgridDirectoryFile = 0x1;
        public const int kSubGridHeaderFlag_IsSubgridSegmentFile = 0x2;

        public const int kSubGridMajorVersion = 2;
        public const int kSubGridMinorVersion_Latest = 0;

        public static byte[] kICServerSubgridLeafFileMoniker => new byte[] { 73, 67, 83, 71, 76, 69, 65, 70 }; // 'ICSGLEAF' 

        public static byte[] kICServerSubgridDirectoryFileMoniker => new byte[] { 73, 67, 83, 71, 68, 73, 82, 76 }; // 'ICSGDIRL';


        public byte[] Identifier = new byte[8];

        public byte MajorVersion;
        public byte MinorVersion;

        public int Flags;
        public DateTime StartTime;
        public DateTime EndTime;

        // FLastUpdateTimeUTC records the time at which this subgrid was last updated
        // in the persistent store
        public DateTime LastUpdateTimeUTC;

        public bool IsSubGridDirectoryFile => (Flags & kSubGridHeaderFlag_IsSubgridDirectoryFile) != 0;
        public bool IsSubGridSegmentFile => (Flags & kSubGridHeaderFlag_IsSubgridSegmentFile) != 0;

        public void Read(BinaryReader reader)
        {
            Identifier = reader.ReadBytes(8);
            MajorVersion = reader.ReadByte();
            MinorVersion = reader.ReadByte();

            Flags = reader.ReadInt32();

            StartTime = DateTime.FromBinary(reader.ReadInt64());
            EndTime = DateTime.FromBinary(reader.ReadInt64());
            LastUpdateTimeUTC = DateTime.FromBinary(reader.ReadInt64());
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Identifier);
            writer.Write(MajorVersion);
            writer.Write(MinorVersion);

            writer.Write(Flags);

            writer.Write(StartTime.ToBinary());
            writer.Write(EndTime.ToBinary());
            writer.Write(LastUpdateTimeUTC.ToBinary());
        }

        public SubGridStreamHeader()
        {
        }

        public SubGridStreamHeader(BinaryReader reader) : this()
        {
            Read(reader);
        }

        /// <summary>
        /// Determines if the header moniker in the stream matches the expected moniker for the context the stream is being read into
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public bool IdentifierMatches(byte[] identifier)
        {
            if (Identifier.Length != identifier.Length)
            {
                return false;
            }

            for (int I = 0; I < Identifier.Length; I++)
            {
                if (Identifier[I] != identifier[I])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
