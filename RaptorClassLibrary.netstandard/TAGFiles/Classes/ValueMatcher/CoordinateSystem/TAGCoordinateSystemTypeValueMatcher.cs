using System;
using VSS.VisionLink.Raptor.TAGFiles.Types;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.CoordinateSystem
{
    /// <summary>
    /// Handles the type of grid coordinate system being used by the machine control system
    /// </summary>
    public class TAGCoordinateSystemTypeValueMatcher : TAGValueMatcher
    {
        public TAGCoordinateSystemTypeValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagCoordSysType };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            if (valueType.Type != TAGDataType.t4bitUInt)
            {
                return false;
            }

            if (!Enum.IsDefined(typeof(CoordinateSystemType), (int)value))
            {
                return false;
            }

            valueSink.CSType = (CoordinateSystemType)value;

            if (valueSink.CSType == CoordinateSystemType.ACS)
            {
                valueSink.IsCSIBCoordSystemTypeOnly = false;
            }

            return true;
        }
    }
}
