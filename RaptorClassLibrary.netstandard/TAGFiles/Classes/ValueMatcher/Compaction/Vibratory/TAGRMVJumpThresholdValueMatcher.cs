using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Compaction.Vibratory
{
    /// <summary>
    /// Handles the vibratory drum Resonance Meter Value jump threshold value configured on the machine
    /// </summary>
    public class TAGRMVJumpThresholdValueMatcher : TAGValueMatcher
    {
        public TAGRMVJumpThresholdValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        public override string[] MatchedValueTypes()
        {
            return new string[] { TAGValueNames.kTagFileRMVJumpThreshold };
        }

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            valueSink.ICRMVJumpthreshold = (short)value;

            return true;
        }
    }
}
