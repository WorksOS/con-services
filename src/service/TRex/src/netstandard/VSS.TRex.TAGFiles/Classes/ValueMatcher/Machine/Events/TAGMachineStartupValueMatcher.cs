using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Machine.Events
{
    /// <summary>
    /// Handles machine startup (power on) events
    /// </summary>
    public class TAGMachineStartupValueMatcher : TAGValueMatcher
    {
        public TAGMachineStartupValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileStartupTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessEmptyValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType)
        {
            return valueSink.DoEpochStateEvent(EpochStateEvent.MachineStartup);
        }
    }
}
