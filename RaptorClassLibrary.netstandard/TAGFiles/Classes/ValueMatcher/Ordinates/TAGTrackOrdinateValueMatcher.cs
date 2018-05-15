using VSS.TRex.Geometry;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Ordinates
{
    public class TAGTrackOrdinateValueMatcher : TAGValueMatcher
    {
        public TAGTrackOrdinateValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileEastingTrackTag, TAGValueNames.kTagFileNorthingTrackTag, TAGValueNames.kTagFileElevationTrackTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessIntegerValue(TAGDictionaryItem valueType, int value)
        {
            // Position value is integer number of millimeters offset from the current position

            if (!state.HaveSeenAnAbsoluteTrackPosition)
            {
                return false;
            }

            if (valueType.Name == TAGValueNames.kTagFileEastingTrackTag)
            {
                if (state.TrackSide == TAGValueSide.Left)
                {
                    valueSink.DataTrackLeft.X += (double)value / 1000;
                }
                else
                {
                    valueSink.DataTrackRight.X += (double)value / 1000;
                }

                return true;
            }

            if (valueType.Name == TAGValueNames.kTagFileNorthingTrackTag)
            {
                if (state.TrackSide == TAGValueSide.Left)
                {
                    valueSink.DataTrackLeft.Y += (double)value / 1000;
                }
                else
                {
                    valueSink.DataTrackRight.Y += (double)value / 1000;
                }

                return true;
            }

            if (valueType.Name == TAGValueNames.kTagFileElevationTrackTag)
            {
                if (state.TrackSide == TAGValueSide.Left)
                {
                    valueSink.DataTrackLeft.Z  += (double)value / 1000;
                }
                else
                {
                    valueSink.DataTrackRight.Z += (double)value / 1000;
                }

                return true;
            }

            return false;
        }

        public override bool ProcessDoubleValue(TAGDictionaryItem valueType, double value)
        {
            state.HaveSeenAnAbsoluteTrackPosition = true;

            if (valueType.Name == TAGValueNames.kTagFileEastingTrackTag)
            {
                if (state.TrackSide == TAGValueSide.Left)
                {
                    valueSink.DataTrackLeft.X = value;
                }
                else
                {
                    valueSink.DataTrackRight.X = value;
                }

                return true;
            }

            if (valueType.Name == TAGValueNames.kTagFileNorthingTrackTag)
            {
                if (state.TrackSide == TAGValueSide.Left)
                {
                    valueSink.DataTrackLeft.Y = value;
                }
                else
                {
                    valueSink.DataTrackRight.Y = value;
                }

                return true;
            }

            if (valueType.Name == TAGValueNames.kTagFileElevationTrackTag)
            {
                if (state.TrackSide == TAGValueSide.Left)
                {
                    valueSink.DataTrackLeft.Z = value;
                }
                else
                {
                    valueSink.DataTrackRight.Z = value;
                }

                return true;
            }

            return false;
        }

        public override bool ProcessEmptyValue(TAGDictionaryItem valueType)
        {
            state.HaveSeenAnAbsoluteTrackPosition = false;

            if (state.TrackSide == TAGValueSide.Left)
            {
                valueSink.DataTrackLeft = XYZ.Null;
            }
            else
            {
                valueSink.DataTrackRight = XYZ.Null;
            }

            return true;
        }
    }
}
