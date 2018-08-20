using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Compaction.Vibratory
{
    /// <summary>
    /// Handles the vibratory drum Resonance Meter Value jump threshold value configured on the machine
    /// </summary>
    public class TAGRMVJumpThresholdValueMatcher : TAGValueMatcher
    {
        public TAGRMVJumpThresholdValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileRMVJumpThreshold };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            valueSink.ICRMVJumpthreshold = (short)value;

            return true;
        }
    }
}
