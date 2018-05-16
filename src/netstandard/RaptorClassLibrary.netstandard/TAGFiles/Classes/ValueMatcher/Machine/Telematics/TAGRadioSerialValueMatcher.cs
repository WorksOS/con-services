using System.Text;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Machine.Telematics
{
    public class TAGRadioSerialValueMatcher : TAGValueMatcher
    {
        public TAGRadioSerialValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagRadioSerial };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessANSIStringValue(TAGDictionaryItem valueType, byte[] value)
        {
            valueSink.RadioSerial = Encoding.ASCII.GetString(value);
            state.HaveSeenARadioSerial = true;

            return true;
        }
    }
}
