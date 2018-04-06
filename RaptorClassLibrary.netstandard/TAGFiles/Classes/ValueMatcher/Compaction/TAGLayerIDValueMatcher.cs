﻿using VSS.VisionLink.Raptor.TAGFiles.Types;

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

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileLayerIDTag };

        public override string[] MatchedValueTypes() => valueTypes;

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
