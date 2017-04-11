using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.ControlState
{
    public class TAGControlStateRightLiftValueMatcher : TAGValueMatcher
    {
        public TAGControlStateRightLiftValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        public override string[] MatchedValueTypes()
        {
            return new string[] { TAGValueNames.kTagFileControlStateRightLift };
        }

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            valueSink.ControlStateRightLift = (int)value;

            return true;
        }
    }
}
