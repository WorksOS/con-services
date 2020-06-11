using System;

namespace VSS.Common.Abstractions.Clients.CWS.Enums
{
  //Note: WM send an int for this when validating projects.
  //If new values are added make sure they will work as flags.
  [Flags]
  public enum CwsProjectType
  {
    Standard = 0,
    AcceptsTagFiles = 1
    //OtherProjectFeatureInTheFuture = 1 << 2
  }
}
