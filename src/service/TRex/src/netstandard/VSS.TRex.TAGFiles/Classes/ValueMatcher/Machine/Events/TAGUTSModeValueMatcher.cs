using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Machine.Events
{
    public class TAGUTSModeValueMatcher : TAGValueMatcher
    {
        public TAGUTSModeValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagUTSMode };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessEmptyValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType)
        {
            return valueSink.DoEpochStateEvent(EpochStateEvent.MachineInUTSMode);
        }
    }
}
