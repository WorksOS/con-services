using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Machine.Events
{
    /// <summary>
    /// Handles machine map reset events
    /// </summary>
    public class TAGMapResetValueMatcher : TAGValueMatcher
    {
        public TAGMapResetValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileMapReset };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessEmptyValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType)
        {
            return valueSink.DoEpochStateEvent(EpochStateEvent.MachineMapReset);
        }
    }
}
