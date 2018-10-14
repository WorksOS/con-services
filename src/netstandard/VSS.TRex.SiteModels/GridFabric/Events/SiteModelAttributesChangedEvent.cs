using System;
using VSS.TRex.SiteModels.Interfaces.Events;

namespace VSS.TRex.SiteModels.GridFabric.Events
{
  /// <summary>
  /// Contains all relevant information detailing a mutating change event made to a site model that effects the metadata and
  /// other information either directly contained within a site model (eg: project extents, cell size etc) or referenced by it
  /// (eg: machines, target event lists, designs, sitemodels etc)
  /// </summary>
  public class SiteModelAttributesChangedEvent : ISiteModelAttributesChangedEvent
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
    /// A serialised bit mask subgrid tree representing the set of subgrids that have been changed in a
    /// mutating event on the sitemodel such as TAG file processing
    /// </summary>
    public byte[] ExistenceMapChangeMask { get; set;  }
  }
}
