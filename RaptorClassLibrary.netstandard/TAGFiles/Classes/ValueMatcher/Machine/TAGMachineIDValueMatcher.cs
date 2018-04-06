using System.Text;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Machine
{
    /// <summary>
    /// Matches machine ID values
    /// </summary>
    public class TAGMachineIDValueMatcher : TAGValueMatcher
    {
        public TAGMachineIDValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileMachineIDTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessANSIStringValue(TAGDictionaryItem valueType, byte[] value)
        {
            valueSink.MachineID = Encoding.ASCII.GetString(value);
            state.HaveSeenAMachineID = true;

            return true;
        }
    }
}
