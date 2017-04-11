using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Cells;
using VSS.VisionLink.Raptor.TAGFiles.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Compaction.Vibratory
{
    /// <summary>
    /// Handles vibratory drum vibration frequency Values reported by the machine 
    /// </summary>
    public class TAGFrequencyValueMatcher : TAGValueMatcher
    {
        public TAGFrequencyValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        public override string[] MatchedValueTypes()
        {
            return new string[] { TAGValueNames.kTagFileICFrequencyTag };
        }

        public override bool ProcessEmptyValue(TAGDictionaryItem valueType)
        {
            state.HaveSeenAnAbsoluteFrequency = false;

            valueSink.SetICFrequency(CellPass.NullFrequency);

            return true;
        }

        public override bool ProcessIntegerValue(TAGDictionaryItem valueType, int value)
        {
            if (!state.HaveSeenAnAbsoluteFrequency)
            {
                return false;
            }

            switch (valueType.Type)
            {
                case TAGDataType.t4bitInt:
                case TAGDataType.t8bitInt:
                    if (((ushort)(valueSink.ICFrequencys.GetLatest()) + value) < 0)
                    {
                        return false;
                    }

                    valueSink.SetICFrequency((ushort)((ushort)(valueSink.ICFrequencys.GetLatest()) + value));
                    break;

                default:
                    return false;
            }

            return true;
        }

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            state.HaveSeenAnAbsoluteFrequency = true;

            switch (valueType.Type)
            {
                case TAGDataType.t12bitUInt:
                    valueSink.SetICFrequency((ushort)value);
                    break;

                default:
                    return false;
            }

            return true;
        }
    }
}
