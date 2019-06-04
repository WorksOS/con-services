using System;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;
using VSS.TRex.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Machine
{
    /// <summary>
    /// Handles the blade of ground flag as reported fom the machine
    /// </summary>
    public class TAGBladeOnGroundValueMatcher : TAGValueMatcher
    {
        public TAGBladeOnGroundValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileBladeOnGroundTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, uint value)
        {
            bool result = false;

            if (valueType.Type == TAGDataType.t4bitUInt &&
                Enum.IsDefined(typeof(OnGroundState), (byte)value))
            {
                valueSink.SetOnGround((OnGroundState)value);
                result = true;
            }
            
            return result;
        }
    }
}
