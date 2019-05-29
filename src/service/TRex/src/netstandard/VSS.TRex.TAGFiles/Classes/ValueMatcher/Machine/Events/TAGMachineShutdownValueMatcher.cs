using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Machine.Events
{
    /// <summary>
    /// Handles machine shutdown (power off) events
    /// </summary>
    public class TAGMachineShutdownValueMatcher : TAGValueMatcher
    {
        public TAGMachineShutdownValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileShutdownTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessEmptyValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType)
        {
            return valueSink.DoEpochStateEvent(EpochStateEvent.MachineShutdown);
        }
    }
}
