using VSS.TRex.Common.CellPasses;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Compaction.MDP
{
    public class TAGMDPValueMatcher : TAGValueMatcher
    {
        public TAGMDPValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileICMDPTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessEmptyValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType)
        {
            state.HaveSeenAnAbsoluteMDP = false;

            valueSink.SetICMDPValue(CellPassConsts.NullMDP);

            return true;
        }

        public override bool ProcessIntegerValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, int value)
        {
            bool result = false;

            if (state.HaveSeenAnAbsoluteMDP &&
                (valueType.Type == TAGDataType.t4bitInt || valueType.Type == TAGDataType.t8bitInt))
            { 
                if (((short)(valueSink.ICMDPValues.GetLatest()) + value) >= 0)
                    valueSink.SetICMDPValue((short)((short)(valueSink.ICMDPValues.GetLatest()) + value));
                {
                     result = true;
                }
            }

            return result;
        }

        public override bool ProcessUnsignedIntegerValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, uint value)
        {       
            bool result = false;
         
            if (valueType.Type == TAGDataType.t12bitUInt)
            {
                state.HaveSeenAnAbsoluteMDP = true;

                valueSink.SetICMDPValue((short) value);
                result = true;
            }
         
            return result;
        }
    }
}
