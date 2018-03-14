using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.TAGFiles.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.CoordinateSystem
{
    /// <summary>
    /// Handles the Universal Tranverse Mercator project zone being used for grid coordinates written into the TAG file data
    /// </summary>
    public class TAGUTMZoneValueMatcher : TAGValueMatcher
    {
        public TAGUTMZoneValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        public override string[] MatchedValueTypes()
        {
            return new string[] { TAGValueNames.kTagUTMZone };
        }

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            if (valueType.Type == TAGDataType.t8bitUInt)
            {
                valueSink.UTMZone = (byte)value;

                if (!state.HaveSeenAUTMZone)
                {
                    valueSink.UTMZoneAtFirstPosition = (byte)value;

                    state.HaveSeenAUTMZone = true;
                }

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
