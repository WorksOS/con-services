using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Machine.Telematics
{
    public class TAGRadioSerialValueMatcher : TAGValueMatcher
    {
        public TAGRadioSerialValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        public override string[] MatchedValueTypes()
        {
            return new string[] { TAGValueNames.kTagRadioSerial };
        }

        public override bool ProcessANSIStringValue(TAGDictionaryItem valueType, byte[] value)
        {
            valueSink.RadioSerial = Encoding.ASCII.GetString(value);
            state.HaveSeenARadioSerial = true;

            return true;
        }
    }
}
