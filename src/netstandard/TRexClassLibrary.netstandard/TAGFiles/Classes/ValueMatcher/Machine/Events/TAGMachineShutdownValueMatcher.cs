using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Machine.Events
{
    /// <summary>
    /// Handles machine shutdown (power off) events
    /// </summary>
    public class TAGMachineShutdownValueMatcher : TAGValueMatcher
    {
        public TAGMachineShutdownValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileShutdownTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessEmptyValue(TAGDictionaryItem valueType)
        {
            return valueSink.DoEpochStateEvent(EpochStateEvent.MachineShutdown);
        }
    }
}
