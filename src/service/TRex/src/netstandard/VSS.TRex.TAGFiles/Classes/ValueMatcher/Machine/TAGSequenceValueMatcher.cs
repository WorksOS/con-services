using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Machine
{
    /// <summary>
    /// Handles compactor and hardware pair ID values
    /// </summary>
    public class TAGSequenceValueMatcher : TAGValueMatcher
    {
        public TAGSequenceValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileSequenceTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            bool result = valueType.Type == TAGDataType.t32bitUInt;

            if (result)
            {
                valueSink.Sequence = value;
            }

            return result;
        }
    }
}
