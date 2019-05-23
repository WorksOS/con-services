using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.ExtensionMethods;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.SiteModels
{
  public class SiteModelMetadata : IBinarizable, ISiteModelMetadata
  {
    private const byte VERSION_NUMBER = 1;

    public Guid ID { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime LastModifiedDate { get; set; }
    public BoundingWorldExtent3D SiteModelExtent { get; set; }
    public int MachineCount { get; set; }
    public int DesignCount { get; set; }
    public int SurveyedSurfaceCount { get; set; }
    public int AlignmentCount { get; set; }


    public void ToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteGuid(ID);
      writer.WriteLong(CreationDate.ToBinary());
      writer.WriteLong(LastModifiedDate.ToBinary());

      writer.WriteBoolean(SiteModelExtent != null);
      SiteModelExtent?.ToBinary(writer);

      writer.WriteInt(MachineCount);
      writer.WriteInt(DesignCount);
      writer.WriteInt(SurveyedSurfaceCount);
      writer.WriteInt(AlignmentCount);
    }

    public void FromBinary(IBinaryRawReader reader)
    {
      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      ID = reader.ReadGuid() ?? Guid.Empty;
      CreationDate = DateTime.FromBinary(reader.ReadLong());
      LastModifiedDate = DateTime.FromBinary(reader.ReadLong());

      if (reader.ReadBoolean())
      {
        SiteModelExtent = new BoundingWorldExtent3D();
        SiteModelExtent.FromBinary(reader);
      }

      MachineCount = reader.ReadInt();
      DesignCount = reader.ReadInt();
      SurveyedSurfaceCount = reader.ReadInt();
      AlignmentCount = reader.ReadInt();
    }

    public void WriteBinary(IBinaryWriter writer) => ToBinary(writer.GetRawWriter());

    public void ReadBinary(IBinaryReader reader) => FromBinary(reader.GetRawReader());
  }
}
