using VSS.TRex.Geometry;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Ordinates
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
            bool result = false;

            if (state.HaveSeenAnAbsoluteRearPosition)
            {
              if (valueType.Name == TAGValueNames.kTagFileEastingRearTag)
              {
                if (state.RearSide == TAGValueSide.Left)
                {
                  valueSink.DataRearLeft.X += (double) value / 1000;
                }
                else
                {
                  valueSink.DataRearRight.X += (double) value / 1000;
                }

                result = true;
              }
              else if (valueType.Name == TAGValueNames.kTagFileNorthingRearTag)
              {
                if (state.RearSide == TAGValueSide.Left)
                {
                  valueSink.DataRearLeft.Y += (double) value / 1000;
                }
                else
                {
                  valueSink.DataRearRight.Y += (double) value / 1000;
                }

                result = true;
              }
              else if (valueType.Name == TAGValueNames.kTagFileElevationRearTag)
              {
                if (state.RearSide == TAGValueSide.Left)
                {
                  valueSink.DataRearLeft.Z += (double) value / 1000;
                }
                else
                {
                  valueSink.DataRearRight.Z += (double) value / 1000;
                }

                result = true;
              }
            }

            return result;
        }

        public override bool ProcessDoubleValue(TAGDictionaryItem valueType, double value)
        {
            state.HaveSeenAnAbsoluteRearPosition = true;
            bool result = false;

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

                result = true;
            }
            else if (valueType.Name == TAGValueNames.kTagFileNorthingRearTag)
            {
                if (state.RearSide == TAGValueSide.Left)
                {
                    valueSink.DataRearLeft.Y = value;
                }
                else
                {
                    valueSink.DataRearRight.Y = value;
                }

                result = true;
            }
            else if (valueType.Name == TAGValueNames.kTagFileElevationRearTag)
            {
                if (state.RearSide == TAGValueSide.Left)
                {
                    valueSink.DataRearLeft.Z = value;
                }
                else
                {
                    valueSink.DataRearRight.Z = value;
                }

                result = true;
            }

            return result;
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

