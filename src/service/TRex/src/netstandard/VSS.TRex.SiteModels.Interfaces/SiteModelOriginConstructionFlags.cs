using System;

namespace VSS.TRex.SiteModels.Interfaces
{
  [Flags]
  public enum SiteModelOriginConstructionFlags
  {
    PreserveNothing = 0x0,
    PreserveExistenceMap = 0x1,
    PreserveGrid = 0x2,
    PreserveCsib = 0x4,
    PreserveDesigns = 0x8,
    PreserveSurveyedSurfaces = 0x10,
    PreserveMachines = 0x20,
    PreserveMachineTargetValues = 0x40,
    PreserveMachineDesigns = 0x80,
    PreserveSiteModelDesigns = 0x100,
    PreserveProofingRuns = 0x200,
    PreserveAlignments = 0x400
  }
}
