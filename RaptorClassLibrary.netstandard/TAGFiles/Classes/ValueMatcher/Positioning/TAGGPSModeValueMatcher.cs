using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.TAGFiles.Types;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Positioning
{
    /// <summary>
    /// Handles transmission gear selection reported by the machine
    /// </summary>
    public class TAGGPSModeValueMatcher : TAGValueMatcher
    {
        public TAGGPSModeValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        public override string[] MatchedValueTypes()
        {
            return new string[] { TAGValueNames.kTagFileGPSModeTag };
        }

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            if (!Enum.IsDefined(typeof(GPSMode), (int)value))
            {
                return false;
            }

            valueSink.SetGPSMode((GPSMode)value);
            return true;
        }
    }
}
