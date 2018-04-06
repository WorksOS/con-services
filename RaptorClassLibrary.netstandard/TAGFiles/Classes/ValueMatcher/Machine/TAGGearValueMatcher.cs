using System;
using VSS.VisionLink.Raptor.TAGFiles.Types;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Machine
{
    public class TAGGearValueMatcher : TAGValueMatcher
    {
        public TAGGearValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileICGearTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            if (valueType.Type != TAGDataType.t4bitUInt)
            {
                return false;
            }

            if (!Enum.IsDefined(typeof(MachineGear), (int)value))
            {
                return false;
            }

            valueSink.ICGear = (MachineGear)value;
            return true;
        }
    }
}
