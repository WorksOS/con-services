using System;
using System.IO;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.Common.Utilities.ExtensionMethods;

namespace VSS.TRex.Designs.Models
{
  public class DesignDescriptor : VersionCheckedBinarizableSerializationBase, IEquatable<DesignDescriptor>, IBinaryReaderWriter
  {
    private const byte VERSION_NUMBER = 1;

    public Guid DesignID { get; private set; }
    public string Folder { get; private set; } = string.Empty;
    public string FileName { get; private set; } = string.Empty;

    public DesignDescriptor()
    {
    }

    public DesignDescriptor(Guid designID,
                            string folder,
                            string fileName) : this()
    {
      DesignID = designID;
      Folder = folder;
      FileName = fileName;
    }

    public void Init(Guid designID,
                     string folder,
                     string fileName)
    {
      DesignID = designID;
      Folder = folder;
      FileName = fileName;
    }

    public string FullPath => Folder != null && FileName != null ? Path.Combine(Folder, FileName) : Folder ?? "" + FileName ?? "";

    public bool IsNull => string.IsNullOrEmpty(FileName);

    /// <summary>
    /// Overloaded ToString detailing the state of the Design Descriptor
    /// </summary>
    /// <returns></returns>
    public override string ToString() => $"[{DesignID}:'{Folder}', '{FileName}']";

    public bool Equals(DesignDescriptor other)
    {
      return DesignID == other.DesignID &&
             Folder == other.Folder &&
             FileName == other.FileName;
    }

    public void Clear() => Init(Guid.Empty, "", "");

    public static DesignDescriptor Null() => new DesignDescriptor();

    public void Write(BinaryWriter writer)
    {
      writer.Write(DesignID.ToByteArray());
      writer.Write(Folder);
      writer.Write(FileName);
    }

    public void Read(BinaryReader reader)
    {
      DesignID = reader.ReadGuid();
      Folder = reader.ReadString();
      FileName = reader.ReadString();
    }

    /// <summary>
    /// Serializes content of the cell to the writer
    /// </summary>
    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteGuid(DesignID);
      writer.WriteString(Folder);
      writer.WriteString(FileName);
    }

    /// <summary>
    /// Deserializes content of the cell from the writer
    /// </summary>
    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        DesignID = reader.ReadGuid() ?? Guid.Empty;
        Folder = reader.ReadString();
        FileName = reader.ReadString();
      }
    }
  }
}

