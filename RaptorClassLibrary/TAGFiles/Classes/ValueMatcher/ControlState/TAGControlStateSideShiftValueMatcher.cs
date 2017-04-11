using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.ControlState
{
    public class TAGControlStateSideShiftValueMatcher : TAGValueMatcher
    {
        public TAGControlStateSideShiftValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        public override string[] MatchedValueTypes()
        {
            return new string[] { TAGValueNames.kTagFileControlStateSideShift };
        }

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            valueSink.ControlStateSideShift = (int)value;

            return true;
        }
    }
}
