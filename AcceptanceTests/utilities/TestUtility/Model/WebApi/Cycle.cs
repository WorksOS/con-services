using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestUtility.Model.WebApi
{
    public class Cycle
    {
        public DateTime? startCycleDeviceTime { get; set; }
        //This field is only applicable for L-D configuration telling when a dumop event has happened. In case of Load config and Dump config it's null
        public DateTime? dumpDeviceTime { get; set; }
        public DateTime? endCycleDeviceTime { get; set; }
        public string cycleReportedDeviceTime { get; set; }
        public double? cycleLengthMinutes { get; set; }
        public double? distanceTravelled { get; set; }
        public double? volumePerCycleCubicMeter { get; set; }
        //In addition we have to store a meter value for a cycle start, end and dump so when aggregating we can respect values if there are cycles with no distance
        //Potenitally, logic for callouts as well
        public double? odometerStartCycleValue { get; set; }
        public double? odometerEndCycleValue { get; set; }
        public double? odometerDumpCycleValue { get; set; }

        public override bool Equals(object obj)
        {
            var actual = obj as Cycle;

            if (distanceTravelled != null)
            { 
                if (Math.Round((double)distanceTravelled, 2) != Math.Round((double)actual.distanceTravelled, 2))
                    return false;
            }

            if (volumePerCycleCubicMeter != null)
            { 
                if (Math.Round((double)volumePerCycleCubicMeter, 2) != Math.Round((double)actual.volumePerCycleCubicMeter, 2))
                    return false;
            }

            if (odometerStartCycleValue != null)
            { 
                if (Math.Round((double)odometerStartCycleValue, 2) != Math.Round((double)actual.odometerStartCycleValue, 2))
                    return false;
            }

            if (odometerEndCycleValue != null)
            { 
                if (Math.Round((double)odometerEndCycleValue, 2) != Math.Round((double)actual.odometerEndCycleValue, 2))
                    return false;
            }

            if (odometerDumpCycleValue != null)
            { 
                if (Math.Round((double)odometerDumpCycleValue, 2) != Math.Round((double)actual.odometerDumpCycleValue, 2))
                    return false;
            }

            if (cycleLengthMinutes != null)
            { 
                if (Math.Round((double)cycleLengthMinutes, 2) != Math.Round((double)actual.cycleLengthMinutes, 2)) 
                    return false;
            }

            if (startCycleDeviceTime != null)
            {
                if (startCycleDeviceTime != actual.startCycleDeviceTime)
                {
                    return false;
                }
            }

            if (endCycleDeviceTime != null)
            {
                if (endCycleDeviceTime != actual.endCycleDeviceTime)
                {
                    return false;
                }
            }

            if (dumpDeviceTime != null)
            {
                if (dumpDeviceTime != actual.dumpDeviceTime)
                {
                    return false;
                }
            }

            if (cycleReportedDeviceTime != null)
            {
                               
                if (DateTimeOffset.Parse(cycleReportedDeviceTime).DateTime  != DateTimeOffset.Parse(actual.cycleReportedDeviceTime).DateTime)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
