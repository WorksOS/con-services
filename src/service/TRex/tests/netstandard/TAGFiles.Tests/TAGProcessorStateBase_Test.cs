using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace TAGFiles.Tests
{
  public class TAGProcessorStateBase_Test : TAGProcessorStateBase
  {
    public EpochStateEvent TriggeredEpochStateEvent = EpochStateEvent.Unknown;

    public override bool DoEpochStateEvent(EpochStateEvent eventType)
    {
      TriggeredEpochStateEvent = eventType;

      return base.DoEpochStateEvent(eventType);
    }
  }
}
