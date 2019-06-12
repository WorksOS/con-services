using System.Text;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Machine
{
    public class TAGSerialValueMatcher : TAGValueMatcher
    {
        public TAGSerialValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileSerialTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessANSIStringValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, string value)
        {
            valueSink.HardwareID = value;

            return true;
        }
    }
}
