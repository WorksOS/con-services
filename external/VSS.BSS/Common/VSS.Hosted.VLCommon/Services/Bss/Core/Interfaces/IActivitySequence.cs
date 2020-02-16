using System.Collections.Generic;

namespace VSS.Hosted.VLCommon.Bss
{
  public interface IActivitySequence
  {
    IEnumerable<IActivity> Activities { get; }
  }
}