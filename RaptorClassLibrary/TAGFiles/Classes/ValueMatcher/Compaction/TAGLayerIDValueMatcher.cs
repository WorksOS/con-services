using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.TAGFiles.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Compaction
{
    /// <summary>
    /// Handles the layer ID set on the machine by the operator
    /// </summary>
    public class TAGLayerIDValueMatcher : TAGValueMatcher
    {
        public TAGLayerIDValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        public override string[] MatchedValueTypes()
        {
            return new string[] { TAGValueNames.kTagFileLayerIDTag };
        }

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            if (valueType.Type == TAGDataType.t12bitUInt)
            {
                valueSink.ICLayerIDValue = (ushort)value;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
