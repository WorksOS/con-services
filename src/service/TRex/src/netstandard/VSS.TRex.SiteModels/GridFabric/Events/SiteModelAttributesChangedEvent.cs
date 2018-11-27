using System;
using System.Linq;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.SiteModels.Interfaces.Events;

namespace VSS.TRex.SiteModels.GridFabric.Events
{
  /// <summary>
  /// Contains all relevant information detailing a mutating change event made to a site model that effects the metadata and
  /// other information either directly contained within a site model (eg: project extents, cell size etc) or referenced by it
  /// (eg: machines, target event lists, designs, sitemodels etc)
  /// </summary>
  public class SiteModelAttributesChangedEvent : BaseRequestResponse, ISiteModelAttributesChangedEvent, IEquatable<SiteModelAttributesChangedEvent>
  {
    public Guid SiteModelID { get; set; } = Guid.Empty;
    public bool ExistenceMapModified { get; set; }
    public bool DesignsModified { get; set; }
    public bool SurveyedSurfacesModified { get; set; }
    public bool CsibModified { get; set; }
    public bool MachinesModified { get; set; }
    public bool MachineTargetValuesModified { get; set; }
    public bool MachineDesignsModified { get; set; }

    /// <summary>
    /// A serialized bit mask subgrid tree representing the set of subgrids that have been changed in a
    /// mutating event on the sitemodel such as TAG file processing
    /// </summary>
    public byte[] ExistenceMapChangeMask { get; set;  }

    public override void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteGuid(SiteModelID);
      writer.WriteBoolean(ExistenceMapModified);
      writer.WriteBoolean(DesignsModified);
      writer.WriteBoolean(SurveyedSurfacesModified);
      writer.WriteBoolean(CsibModified);
      writer.WriteBoolean(MachinesModified);
      writer.WriteBoolean(MachineTargetValuesModified);
      writer.WriteBoolean(MachineDesignsModified);
      writer.WriteByteArray(ExistenceMapChangeMask);
    }

    public override void FromBinary(IBinaryRawReader reader)
    {
      SiteModelID = reader.ReadGuid() ?? Guid.Empty;
      ExistenceMapModified = reader.ReadBoolean();
      DesignsModified = reader.ReadBoolean();
      SurveyedSurfacesModified = reader.ReadBoolean();
      CsibModified = reader.ReadBoolean();
      MachinesModified = reader.ReadBoolean();
      MachineTargetValuesModified = reader.ReadBoolean();
      MachineDesignsModified = reader.ReadBoolean();
      ExistenceMapChangeMask = reader.ReadByteArray();
    }

    public bool Equals(SiteModelAttributesChangedEvent other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;

      return SiteModelID.Equals(other.SiteModelID) && 
             ExistenceMapModified == other.ExistenceMapModified && 
             DesignsModified == other.DesignsModified && 
             SurveyedSurfacesModified == other.SurveyedSurfacesModified && 
             CsibModified == other.CsibModified && 
             MachinesModified == other.MachinesModified && 
             MachineTargetValuesModified == other.MachineTargetValuesModified && 
             MachineDesignsModified == other.MachineDesignsModified &&
             (Equals(ExistenceMapChangeMask, other.ExistenceMapChangeMask) ||
              ExistenceMapChangeMask != null && other.ExistenceMapChangeMask != null &&
              ExistenceMapChangeMask.Length == other.ExistenceMapChangeMask.Length &&
              ExistenceMapChangeMask.SequenceEqual(other.ExistenceMapChangeMask));
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((SiteModelAttributesChangedEvent) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = SiteModelID.GetHashCode();
        hashCode = (hashCode * 397) ^ ExistenceMapModified.GetHashCode();
        hashCode = (hashCode * 397) ^ DesignsModified.GetHashCode();
        hashCode = (hashCode * 397) ^ SurveyedSurfacesModified.GetHashCode();
        hashCode = (hashCode * 397) ^ CsibModified.GetHashCode();
        hashCode = (hashCode * 397) ^ MachinesModified.GetHashCode();
        hashCode = (hashCode * 397) ^ MachineTargetValuesModified.GetHashCode();
        hashCode = (hashCode * 397) ^ MachineDesignsModified.GetHashCode();
        hashCode = (hashCode * 397) ^ (ExistenceMapChangeMask != null ? ExistenceMapChangeMask.GetHashCode() : 0);
        return hashCode;
      }
    }
  }
}
