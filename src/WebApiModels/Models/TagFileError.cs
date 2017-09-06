// todo move to VSS.VisionLink.Interfaces

namespace VSS.VisionLink.Interfaces.Events.TagFile
{
  public enum TagFileError
  {
    Unknown,
    NoMatchingProjectDate,
    NoMatchingProjectArea,
    MultipleProjects,
    InvalidSeedPosition,
    InvalidOnGroundFlag,
    InvalidPosition,
    UnknownCell,
    UnknownProject
  }
}