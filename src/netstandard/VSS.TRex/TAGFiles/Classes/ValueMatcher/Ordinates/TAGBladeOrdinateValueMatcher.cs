using VSS.TRex.Geometry;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Ordinates
{
    /// <summary>
    /// Handles ordinates in 3 space (ie: easting, northing and elevation). Both absolute and delta values are handled. Both
    /// left and right hand drum/blade side values are handled
    /// </summary>
    public class TAGBladeOrdinateValueMatcher : TAGValueMatcher
    {
        public TAGBladeOrdinateValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileEastingTag, TAGValueNames.kTagFileNorthingTag, TAGValueNames.kTagFileElevationTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessIntegerValue(TAGDictionaryItem valueType, int value)
        {
            // Position value is integer number of millimeters offset from the current position

            if (!state.HaveSeenAnAbsolutePosition)
            {
                return false;
            }

            if (valueType.Name == TAGValueNames.kTagFileEastingTag)
            {
                if (state.Side == TAGValueSide.Left)
                {
                    valueSink.DataLeft.X += (double)value / 1000;
                }
                else
                {
                    valueSink.DataRight.X += (double)value / 1000;
                }

                return true;
            }

            if (valueType.Name == TAGValueNames.kTagFileNorthingTag)
            {
                if (state.Side == TAGValueSide.Left)
                {
                    valueSink.DataLeft.Y += (double)value / 1000;
                }
                else
                {
                    valueSink.DataRight.Y += (double)value / 1000;
                }

                return true;
            }

            if (valueType.Name == TAGValueNames.kTagFileElevationTag)
            {
                if (state.Side == TAGValueSide.Left)
                {
                    valueSink.DataLeft.Z += (double)value / 1000;
                }
                else
                {
                    valueSink.DataRight.Z += (double)value / 1000;
                }

                return true;
            }

            return false;
        }

        public override bool ProcessDoubleValue(TAGDictionaryItem valueType, double value)
        {
            state.HaveSeenAnAbsolutePosition = true;

            if (valueType.Name == TAGValueNames.kTagFileEastingTag)
            {
                if (state.Side == TAGValueSide.Left)
                {
                    valueSink.DataLeft.X = value;
                }
                else
                {
                    valueSink.DataRight.X = value;
                }

                return true;
            }

            if (valueType.Name == TAGValueNames.kTagFileNorthingTag)
            {
                if (state.Side == TAGValueSide.Left)
                {
                    valueSink.DataLeft.Y = value;
                }
                else
                {
                    valueSink.DataRight.Y = value;
                }

                return true;
            }

            if (valueType.Name == TAGValueNames.kTagFileElevationTag)
            {
                if (state.Side == TAGValueSide.Left)
                {
                    valueSink.DataLeft.Z = value;
                }
                else
                {
                    valueSink.DataRight.Z = value;
                }

                return true;
            }

            return false;
        }

        public override bool ProcessEmptyValue(TAGDictionaryItem valueType)
        {
            state.HaveSeenAnAbsolutePosition = false;

            if (state.Side == TAGValueSide.Left)
            {
                valueSink.DataLeft = XYZ.Null;
            }                       
            else                    
            {
                valueSink.DataRight = XYZ.Null;
            }

            return true;
        }
    }
}
