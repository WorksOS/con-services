using System;
using System.IO;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.Common.Utilities.ExtensionMethods;
using VSS.TRex.Common.Utilities.Interfaces;

namespace VSS.TRex.Designs.Models
{
  public class DesignOffset : IEquatable<DesignOffset>, IBinaryReaderWriter, IBinarizable, IFromToBinary
  {
    private const byte VERSION_NUMBER = 1;

    public Guid DesignID { get; set; } = Guid.Empty;
    public double Offset { get; set; } = 0;

    public DesignOffset()
    {
    }

    public DesignOffset(Guid designID,
      double offset) : this()
    {
      DesignID = designID;
      Offset = offset;
    }

    public override string ToString() => $"[{DesignID}:'{Offset}']";

    public bool Equals(DesignOffset other)
    {
      return DesignID == other.DesignID &&
             Math.Round(Offset, 3) == Math.Round(other.Offset, 3);//equal to nearest mm
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(DesignID.ToByteArray());
      writer.Write(Offset);
    }

    public void Read(BinaryReader reader)
    {
      DesignID = reader.ReadGuid();
      Offset = reader.ReadDouble();
    }


    public void ToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteGuid(DesignID);
      writer.WriteDouble(Offset);
    }

    public void FromBinary(IBinaryRawReader reader)
    {
      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      DesignID = reader.ReadGuid() ?? Guid.Empty;
      Offset = reader.ReadDouble();
    }

    public void WriteBinary(IBinaryWriter writer) => ToBinary(writer.GetRawWriter());

    public void ReadBinary(IBinaryReader reader) => FromBinary(reader.GetRawReader());
  }
}
