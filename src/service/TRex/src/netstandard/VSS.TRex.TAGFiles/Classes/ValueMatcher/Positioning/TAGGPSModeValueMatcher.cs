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
          bool result = Enum.IsDefined(typeof(GPSMode), (byte) value);

          if (result)
            valueSink.SetGPSMode((GPSMode) value);

          return result;
        }
    }
}
