using System;
using VSS.VisionLink.Raptor.TAGFiles.Types;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher
{
    /// <summary>
    ///  Handles the machine direction TAG value
    /// </summary>
    public class TAGDirectionValueMatcher : TAGValueMatcher
    {
        public TAGDirectionValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileDirectionTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            if (valueType.Type != TAGDataType.t4bitUInt)
            {
                return false;
            }

            // Direction value in tag file is 1-based

            if (!Enum.IsDefined(typeof(MachineDirection), (int)(value - 1)))
            {
                return false;
            }

            valueSink.MachineDirection = (MachineDirection)(value - 1);
            return true;
        }
    }
}
