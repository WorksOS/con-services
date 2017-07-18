using System.ComponentModel;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Enums
{
  public enum TagFileErrorsEnum
  {
    [Description("Unknown Project")]
    UnknownProject = -2,
    [Description("Unknown Cell")]
    UnknownCell = -1,    
    // invalid None = 0, // used in raptor for indicating not set, but never send to logger as not valid enum
    [Description("Project: No matching date/time")]
    ProjectID_NoMatchingDateTime = 1,
    [Description("Project: No matching area")]
    ProjectID_NoMatchingArea = 2,
    [Description("Project: Multiple projects")]
    ProjectID_MultipleProjects = 3,
    [Description("Project: Invalid LLH/NEE position")]
    ProjectID_InvalidLLHNEPosition = 4,
    [Description("No valid cells: OnGround flag not set")]
    NoValidCells_OnGroundFlagNotSet = 5,
    [Description("No valid cells: Invalid position")]
    NoValidCells_InValidPosition = 6,
    [Description("Coordinate conversion failure")]
    CoordConversion_Failure = 7
  }
}