using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Positioning
{
    public class TAGGPSBasePositionValueMatcher : TAGValueMatcher
    {
        public TAGGPSBasePositionValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        public override string[] MatchedValueTypes()
        {
            return new string[] { TAGValueNames.kTagGPSBasePosition };
        }

        public override bool ProcessEmptyValue(TAGDictionaryItem valueType)
        {
            state.GPSBasePositionReportingHaveStarted = true;

            return true;
        }
    }
}
