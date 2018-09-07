using System;
using VSS.TRex.Events;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Events
{
  public class ProductionEventsFactoryTests
  {
    [Fact]
    public void Test_ProductionEventsFactory_FactoryCreation()
    {
      ProductionEventsFactory factory = new ProductionEventsFactory();

      Assert.True(factory != null);
    }

    [Fact]
    public void Test_ProductionEventsFactory_ExpectedNumberOfEventTypes()
    {
      const int count = 27;

      Assert.True(Enum.GetValues(typeof(ProductionEventType)).Length == count, 
        $"Number of production event types is {Enum.GetValues(typeof(ProductionEventType)).Length}, not {count} as exptected");
    }

    [Theory]
    [InlineData(ProductionEventType.TargetCCV, typeof(ProductionEvents<short>))]
    [InlineData(ProductionEventType.TargetPassCount, typeof(ProductionEvents<ushort>))]
    [InlineData(ProductionEventType.TargetLiftThickness, typeof(ProductionEvents<float>))]
    [InlineData(ProductionEventType.GPSModeChange, typeof(ProductionEvents<GPSMode>))]
    [InlineData(ProductionEventType.VibrationStateChange, typeof(ProductionEvents<VibrationState>))]
    [InlineData(ProductionEventType.AutoVibrationStateChange, typeof(ProductionEvents<AutoVibrationState>))]
    [InlineData(ProductionEventType.MachineGearChange, typeof(ProductionEvents<MachineGear>))]
    [InlineData(ProductionEventType.MachineAutomaticsChange, typeof(ProductionEvents<MachineAutomaticsMode>))]
    [InlineData(ProductionEventType.MachineRMVJumpValueChange, typeof(ProductionEvents<short>))]
    [InlineData(ProductionEventType.ICFlagsChange, typeof(ProductionEvents<byte>))]
    [InlineData(ProductionEventType.MinElevMappingStateChange, typeof(ProductionEvents<bool>))]
    [InlineData(ProductionEventType.GPSAccuracyChange, typeof(ProductionEvents<GPSAccuracyAndTolerance>))]
    [InlineData(ProductionEventType.PositioningTech, typeof(ProductionEvents<PositioningTech>))]
    [InlineData(ProductionEventType.TempWarningLevelMinChange, typeof(ProductionEvents<ushort>))]
    [InlineData(ProductionEventType.TempWarningLevelMaxChange, typeof(ProductionEvents<ushort>))]
    [InlineData(ProductionEventType.TargetMDP, typeof(ProductionEvents<short>))]
    [InlineData(ProductionEventType.LayerID, typeof(ProductionEvents<ushort>))]
    [InlineData(ProductionEventType.TargetCCA, typeof(ProductionEvents<byte>))]
    [InlineData(ProductionEventType.StartEndRecordedData, typeof(StartEndProductionEvents))]
    [InlineData(ProductionEventType.MachineStartupShutdown, typeof(StartEndProductionEvents))]
    [InlineData(ProductionEventType.DesignChange, typeof(ProductionEvents<int>))]
    public void Test_ProductionEventsFactory_EventListCreation(ProductionEventType eventType, Type type)
    {
      ProductionEventsFactory factory = new ProductionEventsFactory();

      var list = factory.NewEventList(-1, Guid.Empty, eventType);

      Assert.True(list.GetType() == type, $"Event list created by factory for {eventType} is of type {list.GetType()} not {type}");
    }
  }
}
