﻿ 
 
 
 
 
 
 
 
 
 
 
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
namespace VSS.VisionLink.Landfill.Common.Models  
{
    public enum BookmarkTypeEnum
    {
      None=0,
      WorkDefinitionOffset=1,
      ProcessorTrigger=2,
      RuntimeHoursOffset=3,
      MovingOffset=4,
      EngineStatusOffset=5,
      SwitchStateOffset=6,
      OdometerMeterOffset=7,
      CreateAssetEventOffset=8,
      UpdateAssetEventOffset=9,
      DeleteAssetEventOffset=10,
      ServiceMeterAdjustmentOffset=11,
      Test=255
    }

    public enum CalloutTypeEnum
    {
      None=0,
      MissingMeterValue=1,
      MultipleDayDelta=2,
      Spike=3,
      NotApplicable=4,
      NegativeValue=5,
      NoData=6,
      MissingTotalFuelData=7
    }

    public enum EventTypeEnum
    {
      None=0,
      EngineStart=1,
      EngineStop=2,
      StartMoving=3,
      StopMoving=4,
      SwitchActive=5,
      SwitchInActive=6
    }

    public enum SegmentTypeEnum
    {
      None=0,
      Working=1,
      Idle=2
    }

    public enum TargetHoursTemplateTypeEnum
    {
      Projected=0,
      OneOff=1
    }

    public enum WorkDefinitionTypeEnum
    {
      Unknown=0,
      MovementEvents=1,
      SwitchEvents=2,
      MovementAndSwitchEvents=3,
      MeterDelta=4
    }

}
