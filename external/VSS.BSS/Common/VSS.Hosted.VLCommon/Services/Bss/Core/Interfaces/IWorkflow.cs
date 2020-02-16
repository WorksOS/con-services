using System;
using System.Collections.Generic;

namespace VSS.Hosted.VLCommon.Bss
{
  public interface IWorkflow
  {
    Inputs Inputs { get; }
    IList<IActivitySequence> ActivitySequences { get; }

    DoSequence Do(params IActivity[] activities);
    IfThenElseSequence If(Func<bool> condition);
    WhileSequence While(Func<bool> condition);

    void TransactionStart();
    void TransactionCommit();
  }
}