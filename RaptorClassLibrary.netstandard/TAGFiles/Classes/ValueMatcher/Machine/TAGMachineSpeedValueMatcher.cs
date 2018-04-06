namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Machine
{
    /// <summary>
    /// Handles the machine speed value reported from the machine ECM
    /// </summary>
    public class TAGMachineSpeedValueMatcher : TAGValueMatcher
    {
        public TAGMachineSpeedValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagMachineSpeed };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessDoubleValue(TAGDictionaryItem valueType, double value)
        {
            state.HaveSeenMachineSpeed = true;
            valueSink.SetICMachineSpeedValue(value);

            return true;
        }
    }
}
