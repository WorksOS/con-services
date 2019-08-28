using VSS.MasterData.Models.Models;
using VSS.TRex.Common.Types;
using VSS.TRex.Events.Models;
using VSS.TRex.Types;

namespace VSS.TRex.Events
{
  public static class ProductionEventStateEqualityComparer
  {
    public static bool Equals(byte x, byte y) => x == y;
    public static bool Equals(short x, short y) => x == y;
    public static bool Equals(ushort x, ushort y) => x == y;
    public static bool Equals(int x, int y) => x == y;
    public static bool Equals(float x, float y) => x == y;
    public static bool Equals(double x, double y) => x == y;
    public static bool Equals(GPSMode x, GPSMode y) => x == y;
    public static bool Equals(VibrationState x, VibrationState y) => x == y;
    public static bool Equals(AutoVibrationState x, AutoVibrationState y) => x == y;
    public static bool Equals(MachineGear x, MachineGear y) => x == y;
    public static bool Equals(AutomaticsType x, AutomaticsType y) => x == y;
    public static bool Equals(GPSAccuracyAndTolerance x, GPSAccuracyAndTolerance y) => x.GPSAccuracy == y.GPSAccuracy && x.GPSTolerance == y.GPSTolerance;
    public static bool Equals(ElevationMappingMode x, ElevationMappingMode y) => x == y;
    public static bool Equals(PositioningTech x, PositioningTech y) => x == y;
    public static bool Equals(ProductionEventType x, ProductionEventType y) => x == y;
    public static bool Equals_ProductionEventType(ProductionEventType x, ProductionEventType y) => x == y;
    public static bool Equals(OverrideEvent<int> x, OverrideEvent<int> y) => x.EndDate == y.EndDate && x.Value == y.Value;
    public static bool Equals(OverrideEvent<ushort> x, OverrideEvent<ushort> y) => x.EndDate == y.EndDate && x.Value == y.Value;
  }
}
