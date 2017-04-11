using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.TAGFiles.Types;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Positioning
{
    /// <summary>
    /// Handles the GPS accuracy aggregate message from the mamchine
    /// </summary>
    public class TAGGPSAccuracyValueMatcher : TAGValueMatcher
    {
        public TAGGPSAccuracyValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        public override string[] MatchedValueTypes()
        {
            return new string[] { TAGValueNames.kTagGPSAccuracy };
        }

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            ushort WordToCheck; //16-bit UINT
            GPSAccuracy Accuracy;
            short ErrorLimit;

            if (valueType.Type != TAGDataType.t16bitUInt)
            {
                return false;
            }

            // Shift bits right so that we can check the top 2 bits
            WordToCheck = (ushort)(value >> 14);

            switch (WordToCheck)
            {
                case 0:
                    Accuracy = GPSAccuracy.Fine;
                    break;
                case 1:
                    Accuracy = GPSAccuracy.Medium;
                    break;
                case 2:
                    Accuracy = GPSAccuracy.Coarse;
                    break;
                default:
                    Accuracy = GPSAccuracy.Unknown;
                    break;
            }

            // Lose the top 2 bits; what remains is the error limit in mm
            ErrorLimit = (short)(value & 0x3fff);

            valueSink.SetGPSAccuracyState(Accuracy, ErrorLimit);

            return true;
        }
    }
}
