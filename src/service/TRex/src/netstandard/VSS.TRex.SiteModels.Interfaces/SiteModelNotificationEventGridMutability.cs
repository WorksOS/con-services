using System;

namespace VSS.TRex.SiteModels.Interfaces
{
  /// <summary>
  /// Defines a set of grid mutabilities that may be requested for certain operations such as sitemodel event notification
  /// </summary>
  [Flags]
  public enum SiteModelNotificationEventGridMutability
  {
    NotifyMutable = 0x1,
    NotifyImmutable = 0x2
  }
}
