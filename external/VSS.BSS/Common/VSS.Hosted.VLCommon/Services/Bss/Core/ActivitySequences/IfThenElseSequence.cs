using System;
using System.Collections.Generic;

namespace VSS.Hosted.VLCommon.Bss
{
  public class IfThenElseSequence : IActivitySequence
  {
    private readonly Func<bool> _condition;
    private IActivity[] _trueActivities = new IActivity[]{};
    private IActivity[] _falseActivities = new IActivity[]{};

    public IfThenElseSequence(Func<bool> condition)
    {
      _condition = condition;
    }
    
    public IfThenElseSequence ThenDo(params IActivity[] activities)
    {
      _trueActivities = activities;
      return this;
    }

    public IfThenElseSequence ElseDo(params IActivity[] activities)
    {
      _falseActivities = activities;
      return this;
    }

    public IEnumerable<IActivity> Activities
    {
      get
      {
        if(_condition())
        {
          foreach (var activity in _trueActivities)
          {
            yield return activity;
          }
        }
        else
        {
          foreach (var activity in _falseActivities)
          {
            yield return activity;
          }
        }
      }
    }
  }
}