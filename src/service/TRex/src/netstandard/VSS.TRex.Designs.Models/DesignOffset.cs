using System;
using System.IO;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.Common.Utilities.ExtensionMethods;

namespace VSS.TRex.Designs.Models
{
  public class DesignOffset : VersionCheckedBinarizableSerializationBase, IEquatable<DesignOffset>, IBinaryReaderWriter
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


    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteGuid(DesignID);
      writer.WriteDouble(Offset);
    }

    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        DesignID = reader.ReadGuid() ?? Guid.Empty;
        Offset = reader.ReadDouble();
      }
    }
  }
}
