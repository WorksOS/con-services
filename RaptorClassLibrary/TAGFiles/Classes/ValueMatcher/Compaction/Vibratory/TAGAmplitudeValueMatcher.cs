using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.TAGFiles.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Compaction.Vibratory
{
    /// <summary>
    /// Handles vibratory drum amplitude frequency Values reported by the machine 
    /// </summary>
    public class TAGAmplitudeValueMatcher : TAGValueMatcher
    {
        public TAGAmplitudeValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        public override string[] MatchedValueTypes()
        {
            return new string[] { TAGValueNames.kTagFileICAmplitudeTag };
        }

        public override bool ProcessEmptyValue(TAGDictionaryItem valueType)
        {
            state.HaveSeenAnAbsoluteAmplitude = false;

            valueSink.SetICAmplitude(CellPass.NullAmplitude);

            return true;
        }

        public override bool ProcessIntegerValue(TAGDictionaryItem valueType, int value)
        {
            if (!state.HaveSeenAnAbsoluteAmplitude)
            {
                return false;
            }

            switch (valueType.Type)
            {
                case TAGDataType.t4bitInt:
                case TAGDataType.t8bitInt:
                    if (((ushort)(valueSink.ICAmplitudes.GetLatest()) + value) < 0)
                    {
                        return false;
                    }

                    valueSink.SetICAmplitude((ushort)((ushort)(valueSink.ICAmplitudes.GetLatest()) + value));
                    break;

                default:
                    return false;
            }

            return true;
        }

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            state.HaveSeenAnAbsoluteAmplitude = true;

            switch (valueType.Type)
            {
                case TAGDataType.t12bitUInt:
                    valueSink.SetICAmplitude((ushort)value);
                    break;

                default:
                    return false;
            }

            return true;
        }
    }
}
