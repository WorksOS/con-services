using VSS.TRex.Common.CellPasses;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Compaction.Vibratory
{
    /// <summary>
    /// Handles vibratory drum amplitude frequency Values reported by the machine 
    /// </summary>
    public class TAGAmplitudeValueMatcher : TAGValueMatcher
    {
        public TAGAmplitudeValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileICAmplitudeTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessEmptyValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType)
        {
            state.HaveSeenAnAbsoluteAmplitude = false;

            valueSink.SetICAmplitude(CellPassConsts.NullAmplitude);

            return true;
        }

        public override bool ProcessIntegerValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, int value)
        {
            bool result = false;

            if (state.HaveSeenAnAbsoluteAmplitude &&
               (valueType.Type == TAGDataType.t4bitInt || valueType.Type == TAGDataType.t8bitInt))
            { 
                if (((ushort)(valueSink.ICAmplitudes.GetLatest()) + value) >= 0)
                {
                    valueSink.SetICAmplitude((ushort)((ushort)(valueSink.ICAmplitudes.GetLatest()) + value));
                    result = true;
                }
            }

            return result;
        }

        public override bool ProcessUnsignedIntegerValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, uint value)
        {
            state.HaveSeenAnAbsoluteAmplitude = true;

            bool result = false;

            if (valueType.Type == TAGDataType.t12bitUInt)
            {  
                valueSink.SetICAmplitude((ushort)value);
                result = true;
            }

            return result;
        }
    }
}
