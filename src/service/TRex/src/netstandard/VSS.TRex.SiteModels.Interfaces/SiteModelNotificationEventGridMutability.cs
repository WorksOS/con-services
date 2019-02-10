using System;

namespace VSS.TRex.SiteModels.Interfaces
{
  /// <summary>
  /// Defines a set of grid mutabilities that may be requested for certain operations such as site model event notification
  /// </summary>
  [Flags]
  public enum SiteModelNotificationEventGridMutability : byte
  {
    NotifyMutable = 0x1,
    NotifyImmutable = 0x2
  }
}
