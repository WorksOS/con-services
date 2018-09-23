using System;
using VSS.TRex.SiteModels.Interfaces.Events;

namespace VSS.TRex.SiteModels.GridFabric.Events
{
  public class SiteModelAttributesChangedEvent : ISiteModelAttributesChangedEvent
  {
    public Guid SiteModelID { get; set; } = Guid.Empty;
    public bool ExistenceMapModified { get; set; }
    public bool DesignsModified { get; set; }
    public bool SurveyedSurfacesModified { get; set; }
    public bool CsibModified { get; set; }
    public bool MachinesModified { get; set; }
    public bool MachineTargetValuesModified { get; set; }
  }
}
