using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.TAGFiles.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Ordinates
{
    /// <summary>
    /// Handles left/right (side) empty values for Track axle positioning
    /// </summary>
    public class TAGLeftRightTrackValueMatcher : TAGValueMatcher
    {
        public TAGLeftRightTrackValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        public override string[] MatchedValueTypes()
        {
            return new string[] { TAGValueNames.kTagFileLeftTrackTag, TAGValueNames.kTagFileRightTrackTag };
        }

        public override bool ProcessEmptyValue(TAGDictionaryItem valueType)
        {
            bool result = true;

            if (valueType.Name == TAGValueNames.kTagFileLeftTrackTag)
            {
                state.TrackSide = TAGValueSide.Left;
            }
            else if (valueType.Name == TAGValueNames.kTagFileRightTrackTag)
            {
                state.TrackSide = TAGValueSide.Right;
            }
            else
            {
                result = false;
            }

            return result;
        }

    }
}
