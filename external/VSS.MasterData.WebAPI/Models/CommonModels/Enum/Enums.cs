using System;
using System.Collections.Generic;
using System.Text;

namespace CommonModel.Enum
{
    public enum AssetTargetType
    {
        IdletimeHours = 0,
        RuntimeHours = 1,
        OdometerinKmsPerWeek = 2,
        BucketVolumeinCuMeter = 3,
        PayloadinTonnes = 4,
        CycleCount = 5,
        VolumeinCuMeter = 6,
        IdlingBurnRateinLiPerHour = 7,
        WorkingBurnRateinLiPerHour = 8,
        PayloadPerCycleInTonnes = 9
    }

    public enum AssetSettingsOperationType
    {
        Insert,
        Update,
        Delete
    }
}
