using System;

namespace VSS.TRex.SiteModels.Interfaces.Events
{
  public interface ISiteModelAttributesChangedEvent
  {
    Guid SourceNodeUid { get; }

    Guid SiteModelID { get; }

    bool ExistenceMapModified { get; }
    bool DesignsModified { get; }

    bool SurveyedSurfacesModified { get; }

    bool CsibModified { get; }
    bool MachinesModified { get; }

    bool MachineTargetValuesModified { get;}

    bool MachineDesignsModified { get; }
    
    bool ProofingRunsModified { get; }

    bool AlignmentsModified { get; }

    bool SiteModelMarkedForDeletion { get; set; }

    byte[] ExistenceMapChangeMask { get; }

    /// <summary>
    /// A unique ID for this event communicating site model changes within the grid
    /// </summary>
    Guid ChangeEventUid { get; set; }

    /// <summary>
    /// Date/time the change event was sent
    /// </summary>
    DateTime TimeSentUtc { get; set; }
  }
}
