using System.Text;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Machine.Telematics
{
    public class TAGRadioSerialValueMatcher : TAGValueMatcher
    {
        public TAGRadioSerialValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagRadioSerial };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessANSIStringValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, byte[] value)
        {
            valueSink.RadioSerial = Encoding.ASCII.GetString(value);
            state.HaveSeenARadioSerial = true;

            return true;
        }
    }
}
