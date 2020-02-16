using System;
using System.Collections.Generic;

namespace VSS.Hosted.VLCommon.Bss
{
  public abstract class Workflow : IWorkflow
  {
    protected Workflow(Inputs inputs)
    {
      Inputs = inputs;
      ActivitySequences = new List<IActivitySequence>();
    }

    public Inputs Inputs { get; protected set; }
    public IList<IActivitySequence> ActivitySequences { get; set; }

    public DoSequence Do(params IActivity[] activities)
    {
      var doActivity = new DoSequence(activities);
      ActivitySequences.Add(doActivity);
      return doActivity;
    }

    public IfThenElseSequence If(Func<bool> condition)
    {
      var ifThenElseActivity = new IfThenElseSequence(condition);
      ActivitySequences.Add(ifThenElseActivity);
      return ifThenElseActivity;
    }

    public WhileSequence While(Func<bool> condition)
    {
      var whileActivity = new WhileSequence(condition);
      ActivitySequences.Add(whileActivity);
      return whileActivity;
    }

    public void TransactionStart()
    {
      ActivitySequences.Add(new TransactionStart());
    }

    public void TransactionCommit()
    {
      ActivitySequences.Add(new TransactionCommit());
    }

  }

  public class TransactionCommit : IActivitySequence
  {
    public IEnumerable<IActivity> Activities
    {
      get { yield return new NotificationActivity(CoreConstants.TRANSACTION_COMMITED); }
    }
  }

  public class TransactionStart : IActivitySequence
  {
    public IEnumerable<IActivity> Activities
    {
      get { yield return new NotificationActivity(CoreConstants.TRANSACTION_STARTED); }
    }
  }
}