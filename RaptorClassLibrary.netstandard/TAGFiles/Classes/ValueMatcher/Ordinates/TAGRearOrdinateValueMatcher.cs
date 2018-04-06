using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.TAGFiles.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Ordinates
{
    public class TAGRearOrdinateValueMatcher : TAGValueMatcher
    {
        public TAGRearOrdinateValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileEastingRearTag, TAGValueNames.kTagFileNorthingRearTag, TAGValueNames.kTagFileElevationRearTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessIntegerValue(TAGDictionaryItem valueType, int value)
        {
            // Position value is integer number of millimeters offset from the current position

            if (!state.HaveSeenAnAbsoluteRearPosition)
            {
                return false;
            }

            if (valueType.Name == TAGValueNames.kTagFileEastingRearTag)
            {
                if (state.RearSide == TAGValueSide.Left)
                {
                    valueSink.DataRearLeft.X += (double)value / 1000;
                }
                else
                {
                    valueSink.DataRearRight.X += (double)value / 1000;
                }

                return true;
            }

            if (valueType.Name == TAGValueNames.kTagFileNorthingRearTag)
            {
                if (state.RearSide == TAGValueSide.Left)
                {
                    valueSink.DataRearLeft.Y += (double)value / 1000;
                }
                else
                {
                    valueSink.DataRearRight.Y += (double)value / 1000;
                }

                return true;
            }

            if (valueType.Name == TAGValueNames.kTagFileElevationRearTag)
            {
                if (state.RearSide == TAGValueSide.Left)
                {
                    valueSink.DataRearLeft.Z += (double)value / 1000;
                }
                else
                {
                    valueSink.DataRearRight.Z += (double)value / 1000;
                }

                return true;
            }

            return false;
        }

        public override bool ProcessDoubleValue(TAGDictionaryItem valueType, double value)
        {
            state.HaveSeenAnAbsoluteRearPosition = true;

            if (valueType.Name == TAGValueNames.kTagFileEastingRearTag)
            {
                if (state.RearSide == TAGValueSide.Left)
                {
                    valueSink.DataRearLeft.X = value;
                }
                else
                {
                    valueSink.DataRearRight.X = value;
                }

                return true;
            }

            if (valueType.Name == TAGValueNames.kTagFileNorthingRearTag)
            {
                if (state.RearSide == TAGValueSide.Left)
                {
                    valueSink.DataRearLeft.Y = value;
                }
                else
                {
                    valueSink.DataRearRight.Y = value;
                }

                return true;
            }

            if (valueType.Name == TAGValueNames.kTagFileElevationRearTag)
            {
                if (state.RearSide == TAGValueSide.Left)
                {
                    valueSink.DataRearLeft.Z = value;
                }
                else
                {
                    valueSink.DataRearRight.Z = value;
                }

                return true;
            }

            return false;
        }

        public override bool ProcessEmptyValue(TAGDictionaryItem valueType)
        {
            state.HaveSeenAnAbsoluteRearPosition = false;

            if (state.RearSide == TAGValueSide.Left)
            {
                valueSink.DataRearLeft = XYZ.Null;
            }
            else
            {
                valueSink.DataRearRight = XYZ.Null;
            }

            return true;
        }
    }
}

