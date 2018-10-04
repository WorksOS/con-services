using System;

namespace VSS.TRex.SiteModels.Interfaces
{
  [Flags]
  public enum SiteModelOriginConstructionFlags
  {
    PreserveExistenceMap = 0x1,
    PreserveGrid = 0x2,
    PreserveCsib = 0x4,
    PreserveDesigns = 0x8,
    PreserveSurveyedSurfaces = 0x10,
    PreserveMachines = 0x20,
    PreserveMachineTargetValues = 0x40,
    PreserveMachineDesigns = 0x80,
  }
}
