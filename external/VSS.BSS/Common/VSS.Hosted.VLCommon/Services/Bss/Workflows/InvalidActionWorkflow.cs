using System;
using System.Collections.Generic;

namespace VSS.Hosted.VLCommon.Bss
{
  public class InvalidActionWorkflow : IWorkflow
  {
    private readonly object _action;
    private readonly Type _messageType;

    public InvalidActionWorkflow(object action, Type messageType)
    {
      _action = action;
      _messageType = messageType;
    }

    public Inputs Inputs { get { return new Inputs(); }}

    public IList<IActivitySequence> ActivitySequences
    {
      get
      {
        IActivity activity = new InvalidAction(_action, _messageType);
        IActivitySequence activitySequence = new ActivitySequence(new []{activity});

        return new List<IActivitySequence> { activitySequence };
      }
    }

    #region NOT IMPLEMENTED
    public DoSequence Do(params IActivity[] activities)
    {
      throw new NotImplementedException();
    }

    public IfThenElseSequence If(Func<bool> condition)
    {
      throw new NotImplementedException();
    }

    public WhileSequence While(Func<bool> condition)
    {
      throw new NotImplementedException();
    }

    public void TransactionStart()
    {
      throw new NotImplementedException();
    }

    public void TransactionCommit()
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}