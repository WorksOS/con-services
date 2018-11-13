using System;
using System.Diagnostics;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.ExtensionMethods;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.SiteModels
{
  public class SiteModelMetadata : BaseRequestBinarizableArgument, ISiteModelMetadata, IEquatable<BaseRequestBinarizableArgument>
  {
    private const byte versionNumber = 1;

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
      writer.WriteByte(versionNumber);

      writer.WriteGuid(ID);
      writer.WriteString(Name);
      writer.WriteString(Description);
      writer.WriteLong(LastModifiedDate.Ticks);

      writer.WriteBoolean(SiteModelExtent != null);
      SiteModelExtent?.ToBinary(writer);

      writer.WriteInt(MachineCount);
      writer.WriteInt(DesignCount);
      writer.WriteInt(SurveyedSurfaceCount);
    }

    public override void FromBinary(IBinaryRawReader reader)
    {
      byte readVersionNumber = reader.ReadByte();

      Debug.Assert(readVersionNumber == versionNumber, $"Invalid version number: {readVersionNumber}, expecting {versionNumber}");

      ID = reader.ReadGuid() ?? Guid.Empty;
      Name = reader.ReadString();
      Description = reader.ReadString();
      LastModifiedDate = new DateTime(reader.ReadLong());

      if (reader.ReadBoolean())
      { 
        SiteModelExtent = new BoundingWorldExtent3D();
        SiteModelExtent.FromBinary(reader);
      }

      MachineCount = reader.ReadInt();
      DesignCount = reader.ReadInt();
      SurveyedSurfaceCount = reader.ReadInt();
    }

    protected bool Equals(SiteModelMetadata other)
    {
      return ID.Equals(other.ID) && 
             string.Equals(Name, other.Name) && 
             string.Equals(Description, other.Description) && 
             LastModifiedDate.Equals(other.LastModifiedDate) && 
             Equals(SiteModelExtent, other.SiteModelExtent) && 
             MachineCount == other.MachineCount && 
             DesignCount == other.DesignCount && 
             SurveyedSurfaceCount == other.SurveyedSurfaceCount;
    }

    public bool Equals(BaseRequestBinarizableArgument other)
    {
      return Equals(other as SiteModelMetadata);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((SiteModelMetadata) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = ID.GetHashCode();
        hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ (Description != null ? Description.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ LastModifiedDate.GetHashCode();
        hashCode = (hashCode * 397) ^ (SiteModelExtent != null ? SiteModelExtent.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ MachineCount;
        hashCode = (hashCode * 397) ^ DesignCount;
        hashCode = (hashCode * 397) ^ SurveyedSurfaceCount;
        return hashCode;
      }
    }
  }
}
