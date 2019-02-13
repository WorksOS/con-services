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
    public DateTime CreationDate { get; set; }
    public DateTime LastModifiedDate { get; set; }
    public BoundingWorldExtent3D SiteModelExtent { get; set; }
    public int MachineCount { get; set; }
    public int DesignCount { get; set; }
    public int SurveyedSurfaceCount { get; set; }
    public int AlignmentCount { get; set; }


    public override void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteByte(VERSION_NUMBER);

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

    public override void FromBinary(IBinaryRawReader reader)
    {
      byte readVersionNumber = reader.ReadByte();

      if (readVersionNumber != VERSION_NUMBER)
        throw new TRexSerializationVersionException(VERSION_NUMBER, readVersionNumber);

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
  }
}
