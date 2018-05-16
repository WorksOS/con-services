using System;
using VSS.TRex.TAGFiles.Types;
using VSS.TRex.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Machine
{
    /// <summary>
    /// Handles the blade of ground flag as reported fom the amchine
    /// </summary>
    public class TAGBladeOnGroundValueMatcher : TAGValueMatcher
    {
        public TAGBladeOnGroundValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileBladeOnGroundTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            if (valueType.Type != TAGDataType.t4bitUInt)
            {
                return false;
            }

            if (!Enum.IsDefined(typeof(OnGroundState), (int)value))
            {
                return false;
            }

            valueSink.SetOnGround((OnGroundState)value);
            return true;
        }
    }
}
