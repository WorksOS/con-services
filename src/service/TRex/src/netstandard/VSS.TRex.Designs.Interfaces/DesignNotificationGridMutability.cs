using System;

namespace VSS.TRex.Designs.Interfaces
{
  /// <summary>
  /// Defines a set of grid mutabilities that may be requested for changes to a design
  /// </summary>
  [Flags]
  public enum DesignNotificationGridMutability : byte
  {
    NotifyMutable = 0x1,
    NotifyImmutable = 0x2,
    NotifyAll = NotifyMutable | NotifyImmutable
  }

}
