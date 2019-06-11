using System;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;
using VSS.TRex.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Positioning
{
    /// <summary>
    /// Handles transmission gear selection reported by the machine
    /// </summary>
    public class TAGGPSModeValueMatcher : TAGValueMatcher
    {
        public TAGGPSModeValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileGPSModeTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, uint value)
        {
          bool result = (value >= GPSModeConstants.GPS_MODE_MIN_VALUE && value <= GPSModeConstants.GPS_MODE_MAX_VALUE) || value == (int)GPSMode.NoGPS;

          if (result)
            valueSink.SetGPSMode((GPSMode) value);

          return result;
        }
    }
}
