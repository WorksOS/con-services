using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Machine
{
    public class TAGWheelWidthValueMatcher : TAGValueMatcher
    {
        public TAGWheelWidthValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileWheelWidthTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessDoubleValue(TAGDictionaryItem valueType, double value)
        {
            // Value is absolute wheel width expressed in

            if (valueType.Type == TAGDataType.tIEEEDouble)
            {
                valueSink.MachineWheelWidth = value;
                return true;
            }

            return false;
        }
    }
}
