using VSS.VisionLink.Raptor.TAGFiles.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Machine.Events
{
    /// <summary>
    /// Handles machine startup (power on) events
    /// </summary>
    public class TAGMachineStartupValueMatcher : TAGValueMatcher
    {
        public TAGMachineStartupValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileStartupTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessEmptyValue(TAGDictionaryItem valueType)
        {
            return valueSink.DoEpochStateEvent(EpochStateEvent.MachineStartup);
        }
    }
}
