using VSS.VisionLink.Raptor.TAGFiles.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Machine.Events
{
    /// <summary>
    /// Handles machine map reset events
    /// </summary>
    public class TAGMapResetValueMatcher : TAGValueMatcher
    {
        public TAGMapResetValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileMapReset };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessEmptyValue(TAGDictionaryItem valueType)
        {
            return valueSink.DoEpochStateEvent(EpochStateEvent.MachineMapReset);
        }
    }
}
