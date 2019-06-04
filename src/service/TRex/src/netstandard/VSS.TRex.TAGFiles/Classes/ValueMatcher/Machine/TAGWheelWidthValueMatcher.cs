using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Machine
{
    public class TAGWheelWidthValueMatcher : TAGValueMatcher
    {
        public TAGWheelWidthValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileWheelWidthTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessDoubleValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, double value)
        {
            // Value is absolute wheel width expressed in

            bool result = false;

            if (valueType.Type == TAGDataType.tIEEEDouble)
            {
                valueSink.MachineWheelWidth = value;
                result = true;
            }

            return result;
        }
    }
}
