using VSS.VisionLink.Raptor.TAGFiles.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Machine.Events
{
    public class TAGUTSModeValueMatcher : TAGValueMatcher
    {
        public TAGUTSModeValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagUTSMode };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessEmptyValue(TAGDictionaryItem valueType)
        {
            return valueSink.DoEpochStateEvent(EpochStateEvent.MachineInUTSMode);
        }
    }
}
