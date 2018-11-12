using System;

namespace VSS.TRex.SiteModels.Interfaces.Events
{
  public interface ISiteModelAttributesChangedEvent
  {
    Guid SiteModelID { get; }

    bool ExistenceMapModified { get; }
    bool DesignsModified { get; }

    bool SurveyedSurfacesModified { get; }

    bool CsibModified { get; }
    bool MachinesModified { get; }

    bool MachineTargetValuesModified { get;}
    bool MachineDesignsModified { get; }

    byte[] ExistenceMapChangeMask { get; }
  }
}
