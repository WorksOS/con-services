using System;
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
  public class SiteModelAttributesChangedEvent : BaseRequestResponse, ISiteModelAttributesChangedEvent
  {
    public Guid SiteModelID { get; set; } = Guid.Empty;
    public bool ExistenceMapModified { get; set; }
    public bool DesignsModified { get; set; }
    public bool SurveyedSurfacesModified { get; set; }
    public bool CsibModified { get; set; }
    public bool MachinesModified { get; set; }
    public bool MachineTargetValuesModified { get; set; }
    public bool MachineDesignsModified { get; set; }
    public bool ProofingRunsModified { get; set; }


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
      writer.WriteBoolean(ProofingRunsModified);
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
      ProofingRunsModified = reader.ReadBoolean();
      ExistenceMapChangeMask = reader.ReadByteArray();
    }
  }
}
