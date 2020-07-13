using System;

namespace VSS.Productivity3D.Project.Abstractions.Models.Cws
{
  [Flags]
  public enum NotificationType
  {
    Unknown = 0x00,
    MetaData = 0x01,
    CoordinateSystem = 0x02
  }
}
