using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.ExtensionMethods;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.SiteModels
{
  public class SiteModelMetadata : BaseRequestArgument, ISiteModelMetadata
  {
    private const byte VERSION_NUMBER = 1;

    public Guid ID { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime LastModifiedDate { get; set; }
    public BoundingWorldExtent3D SiteModelExtent { get; set; }
    public int MachineCount { get; set; }
    public int DesignCount { get; set; }
    public int SurveyedSurfaceCount { get; set; }


    public override void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteByte(VERSION_NUMBER);

      writer.WriteGuid(ID);
      writer.WriteString(Name);
      writer.WriteString(Description);
      writer.WriteLong(LastModifiedDate.ToBinary());

      writer.WriteBoolean(SiteModelExtent != null);
      SiteModelExtent?.ToBinary(writer);

      writer.WriteInt(MachineCount);
      writer.WriteInt(DesignCount);
      writer.WriteInt(SurveyedSurfaceCount);
    }

    public override void FromBinary(IBinaryRawReader reader)
    {
      byte readVersionNumber = reader.ReadByte();

      if (readVersionNumber != VERSION_NUMBER)
        throw new TRexSerializationVersionException(VERSION_NUMBER, readVersionNumber);

      ID = reader.ReadGuid() ?? Guid.Empty;
      Name = reader.ReadString();
      Description = reader.ReadString();
      LastModifiedDate = DateTime.FromBinary(reader.ReadLong());

      if (reader.ReadBoolean())
      {
        SiteModelExtent = new BoundingWorldExtent3D();
        SiteModelExtent.FromBinary(reader);
      }

      MachineCount = reader.ReadInt();
      DesignCount = reader.ReadInt();
      SurveyedSurfaceCount = reader.ReadInt();
    }
  }
}
