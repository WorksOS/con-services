using System;
using System.Collections.Generic;

namespace VSS.Hosted.VLCommon.Bss
{
  public class WhileSequence : IActivitySequence
  {
    private IActivity[] _activities;
    private readonly Func<bool> _condition = () => false;

    public WhileSequence(Func<bool> condition)
    {
      _condition = condition;
    }

    public WhileSequence Do(params IActivity[] activities)
    {
      _activities = activities;
      return this;
    }

    public IEnumerable<IActivity> Activities
    {
      get
      {
        while (_condition())
        {
          foreach (var activity in _activities)
          {
            yield return activity;
          }
        }
      }
    }
  }
}