using System;
using VSS.TRex.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Positioning
{
    /// <summary>
    /// Handles transmission gear selection reported by the machine
    /// </summary>
    public class TAGGPSModeValueMatcher : TAGValueMatcher
    {
        public TAGGPSModeValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileGPSModeTag };

        public override string[] MatchedValueTypes() => valueTypes;

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
