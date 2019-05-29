using System.Text;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Machine.Telematics
{
    public class TAGRadioTypeValueMatcher : TAGValueMatcher
    {
        public TAGRadioTypeValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagRadioType };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessANSIStringValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, byte[] value)
        {
            valueSink.RadioType = Encoding.ASCII.GetString(value);
            state.HaveSeenARadioSerial = true;

            return true;
        }
    }
}
