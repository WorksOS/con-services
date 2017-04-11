using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Machine.Telematics
{
    public class TAGRadioTypeValueMatcher : TAGValueMatcher
    {
        public TAGRadioTypeValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        public override string[] MatchedValueTypes()
        {
            return new string[] { TAGValueNames.kTagRadioType };
        }

        public override bool ProcessANSIStringValue(TAGDictionaryItem valueType, byte[] value)
        {
            valueSink.RadioType = Encoding.ASCII.GetString(value);
            state.HaveSeenARadioSerial = true;

            return true;
        }
    }
}
