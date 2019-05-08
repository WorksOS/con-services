using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Interfaces.Events.AssetTarget.Context
{
    public enum AssetTargetType
    {
		IdletimeHours, // Weekly Idle Hours
		RuntimeHours, // weekly Runtime Hours
		OdometerinKmsPerWeek, // Odometer value in kms per week
		BucketVolumeinCuMeter, // Bucket Volume in Cu.Meter Per Cycle
		PayloadinTonnes, // Weekly Payload Data in Tonnes
		CycleCount, // Weekly Cycle Count Data
		VolumeinCuMeter, // Weekly Volume Data in Cu.Meter
        IdlingBurnRateinLiPerHour, // Fuel Burn Rate for an asset in Liters per Hour when idle
        WorkingBurnRateinLiPerHour, // Fuel Burn Rate for an asset in Liters per Hour when working
    }
}
