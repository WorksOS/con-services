namespace VSS.TagFileAuth.Service.WebApi.Enums
{
  public enum TagFileErrorsEnum
  {
    UnknownProject = -2,
    UnknownCell = -1,
    None = 0, // used in raptor for indicating not set, but never send to logger as not valid enum
    ProjectID_NoMatchingDateTime = 1,
    ProjectID_NoMatchingArea = 2,
    ProjectID_MultipleProjects = 3,
    ProjectID_InvalidLLHNEPosition = 4,
    NoValidCells_OnGroundFlagNotSet = 5,
    NoValidCells_InValidPosition = 6,
    CoordConversion_Failure = 7
  }
}