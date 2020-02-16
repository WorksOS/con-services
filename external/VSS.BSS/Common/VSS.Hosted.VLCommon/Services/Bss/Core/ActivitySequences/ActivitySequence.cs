using System.Collections.Generic;

namespace VSS.Hosted.VLCommon.Bss
{
  public class ActivitySequence : IActivitySequence
  {
    private readonly IActivity[] _activities;

    public ActivitySequence(IActivity[] activities)
    {
      _activities = activities;
    }

    public IEnumerable<IActivity> Activities
    {
      get
      {
        foreach (var activity in _activities)
        {
          yield return activity;
        }
      }
    }
  }
}
