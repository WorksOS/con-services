using System.ComponentModel;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Enums
{
  public enum TagFileErrorsEnum
  {
    [Description("Unknown Project")]
    UnknownProject = -2,   // UnableToDetermineProjectID
    [Description("Unknown Cell")]
    UnknownCell = -1,      // NoValidCellPassesInTagfile
    // invalid None = 0, // used in raptor for indicating not set, but never send to logger as not valid enum
    [Description("Project: No matching date/time")]
    ProjectID_NoMatchingDateTime = 1,  // UnableToDetermineProjectID
    [Description("Project: No matching area")]
    ProjectID_NoMatchingArea = 2,    // UnableToDetermineProjectID
    [Description("Project: Multiple projects")]
    ProjectID_MultipleProjects = 3,   // UnableToDetermineProjectID
    [Description("Project: Invalid LLH/NEE position")]
    ProjectID_InvalidLLHNEPosition = 4,   // UnableToDetermineProjectID
    [Description("No valid cells: OnGround flag not set")]
    NoValidCells_OnGroundFlagNotSet = 5, // NoValidCellPassesInTagfile
    [Description("No valid cells: Invalid position")]
    NoValidCells_InValidPosition = 6, // NoValidCellPassesInTagfile
    [Description("Coordinate conversion failure")]
    CoordConversion_Failure = 7 // NoValidCellPassesInTagfile
  }
}