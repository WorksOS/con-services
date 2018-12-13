using System;

namespace VSS.TRex.Profiling.Models
{
  /// <summary>
  /// The layer status states that may be assigned to a material layer analyzed from production data in the site model
  /// </summary>
  [Flags]
  public enum LayerStatus
  {
    None = 0,
    Complete = 0x1,
    WorkInProgress = 0x2,
    Undercompacted = 0x4,
    Overcompacted = 0x8,
    TooThick = 0x10,
    Superseded = 0x20
  }
}
