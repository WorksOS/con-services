using VSS.VisionLink.Raptor.TAGFiles.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Machine
{
    /// <summary>
    /// Handles the flag indicating when the machine is recording minimum elevation mapping information
    /// </summary>
    public class TAGMinElevMappingValueMatcher : TAGValueMatcher
    {
        public TAGMinElevMappingValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileMinElevMappingFlag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            if (valueType.Type == TAGDataType.t4bitUInt && (value == 0 || value == 1))
            {
                valueSink.SetMinElevMappingState(value == 1);
                return true;
            }

            return false;
        }
    }
}
