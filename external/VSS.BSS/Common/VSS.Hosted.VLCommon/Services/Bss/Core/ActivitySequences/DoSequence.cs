using System;
using System.Collections.Generic;

namespace VSS.Hosted.VLCommon.Bss
{
  public class DoSequence : IActivitySequence
  {
    private readonly IActivity[] _activities;
    private Func<bool> _condition = () => false;

    public DoSequence(params IActivity[] activities)
    {
      _activities = activities;
    }

    public void While(Func<bool> condition)
    {
      _condition = condition;
    }

    public IEnumerable<IActivity> Activities
    {
      get
      {
        do
        {
          foreach (var activity in _activities)
          {
            yield return activity;
          } 
        } while (_condition());
      }
    }
  }
}