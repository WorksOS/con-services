using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Models.Models;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Types;
using VSS.TRex.DI;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.Events.Models;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Events
{
  /// <summary>
  /// A wrapper for all the event information related to a particular machine's activities within a particular
  /// site model.co
  /// </summary>
  public class ProductionEventLists : IProductionEventLists
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    /// <summary>
    /// The array of enumeration values represented by ProductionEventType
    /// </summary>
    public static readonly ProductionEventType[] ProductionEventTypeValues = Enum.GetValues(typeof(ProductionEventType)).Cast<ProductionEventType>().ToArray();
 
    private static readonly IProductionEventsFactory _ProductionEventsFactory = DIContext.Obtain<IProductionEventsFactory>();

    /// <summary>
    /// The SiteModel these events relate to
    /// </summary>
    private ISiteModel SiteModel;

    /// <summary>
    /// The collection of concrete event lists for a machine. Each element in the array is indexed by the
    /// ProductionEventType for that collection of events;
    /// </summary>
    private readonly IProductionEvents[] allEventsForMachine;

    private short _internalSiteModelMachineIndex;
    /// <summary>
    /// The ID of the machine these events were recorded by. The ID is the (short) internal machine ID
    /// used within the data model, not the GUID descriptor for the machine
    /// </summary>
    public short InternalSiteModelMachineIndex { get => _internalSiteModelMachineIndex; set => _internalSiteModelMachineIndex = value; }

    public IStartEndProductionEvents MachineStartupShutdownEvents
    {
      get => (IStartEndProductionEvents) GetEventList(ProductionEventType.MachineStartupShutdown);
    }

    /// <summary>
    /// Events recording the Start and Stop events for recording production data on a machine
    /// </summary>
    public IStartEndProductionEvents StartEndRecordedDataEvents
    {
      get => (IStartEndProductionEvents) GetEventList(ProductionEventType.StartEndRecordedData);
    }

    /// <summary>
    /// Events recording vibration state changes for vibratory drum compactor operation
    /// </summary>
    public IProductionEvents<VibrationState> VibrationStateEvents
    {
      get => (IProductionEvents<VibrationState>) GetEventList(ProductionEventType.VibrationStateChange);
    }

    /// <summary>
    /// Events recording automatics vibration state changes for vibratory drum compactor operation
    /// </summary>
    public IProductionEvents<AutoVibrationState> AutoVibrationStateEvents
    {
      get => (IProductionEvents<AutoVibrationState>) GetEventList(ProductionEventType.AutoVibrationStateChange);
    }

    /// <summary>
    /// Events recording changes to the prevailing GPSMode (eg: RTK Fixed, RTK Float, Differential etc) at the time 
    /// production measurements were being made
    /// </summary>
    public IProductionEvents<GPSMode> GPSModeStateEvents
    {
      get => (IProductionEvents<GPSMode>) GetEventList(ProductionEventType.GPSModeChange);
    }

    /// <summary>
    /// Records the positioning technology (eg: GPS or UTS) being used at the time production measurements were being made
    /// </summary>
    public IProductionEvents<PositioningTech> PositioningTechStateEvents
    {
      get => (IProductionEvents<PositioningTech>) GetEventList(ProductionEventType.PositioningTech);
    }

    /// <summary>
    /// Records the state of the automatic machine control on the machine at the time measurements were being made.
    /// </summary>
    public IProductionEvents<AutomaticsType> MachineAutomaticsStateEvents
    {
      get => (IProductionEvents<AutomaticsType>) GetEventList(ProductionEventType.MachineAutomaticsChange);
    }

    /// <summary>
    /// Records the state of the selected machine gear at the time measurements were being made
    /// </summary>
    public IProductionEvents<MachineGear> MachineGearStateEvents
    {
      get => (IProductionEvents<MachineGear>) GetEventList(ProductionEventType.MachineGearChange);
    }

    /// <summary>
    /// Records the state of minimum elevation mapping on the machine at the time measurements were being made
    /// </summary>
    public IProductionEvents<ElevationMappingMode> ElevationMappingModeStateEvents
    {
      get => (IProductionEvents<ElevationMappingMode>) GetEventList(ProductionEventType.ElevationMappingModeStateChange);
    }

    /// <summary>
    /// Records the state of GPSAccuracy and accompanying GPSTolerance on the machine at the time measurements were being made
    /// </summary>
    public IProductionEvents<GPSAccuracyAndTolerance> GPSAccuracyAndToleranceStateEvents
    {
      get => (IProductionEvents<GPSAccuracyAndTolerance>) GetEventList(ProductionEventType.GPSAccuracyChange);
    }

    /// <summary>
    /// Records the selected Layer ID on the machine at the time measurements were being made
    /// </summary>
    public IProductionEvents<ushort> LayerIDStateEvents
    {
      get
      {
        var layerEvents = (IProductionEvents<ushort>)GetEventList(ProductionEventType.LayerID);
        return MergeEventLists(layerEvents, LayerOverrideEvents, ProductionEventType.LayerID);
      }
    }

    /// <summary>
    /// Records the selected design on the machine at the time the measurements were made
    /// </summary>
    public IProductionEvents<int> MachineDesignNameIDStateEvents
    {
      get
      {
        var designEvents = (IProductionEvents<int>) GetEventList(ProductionEventType.DesignChange);
        return MergeEventLists(designEvents, DesignOverrideEvents, ProductionEventType.DesignChange);
      }
    }

    /// <summary>
    /// ICFlags control flags change events
    /// </summary>
    public IProductionEvents<byte> ICFlagsStateEvents
    {
      get => (IProductionEvents<byte>) GetEventList(ProductionEventType.ICFlagsChange);
    }

    /// <summary>
    /// Records the target CCV value configured on the machine control system
    /// </summary>
    public IProductionEvents<short> TargetCCVStateEvents
    {
      get => (IProductionEvents<short>) GetEventList(ProductionEventType.TargetCCV);
    }

    /// <summary>
    /// Records the target CCA value configured on the machine control system
    /// </summary>
    public IProductionEvents<byte> TargetCCAStateEvents
    {
      get => (IProductionEvents<byte>) GetEventList(ProductionEventType.TargetCCA);
    }

    /// <summary>
    /// Records the target MDP value configured on the machine control system
    /// </summary>
    public IProductionEvents<short> TargetMDPStateEvents
    {
      get => (IProductionEvents<short>) GetEventList(ProductionEventType.TargetMDP);
    }

    /// <summary>
    /// Records the target MDP value configured on the machine control system
    /// </summary>
    public IProductionEvents<ushort> TargetPassCountStateEvents
    {
      get => (IProductionEvents<ushort>) GetEventList(ProductionEventType.TargetPassCount);
    }

    /// <summary>
    /// Records the target minimum temperature value configured on the machine control system
    /// </summary>
    public IProductionEvents<ushort> TargetMinMaterialTemperature
    {
      get => (IProductionEvents<ushort>) GetEventList(ProductionEventType.TempWarningLevelMinChange);
    }

    /// <summary>
    /// Records the target maximum temperature value configured on the machine control system
    /// </summary>
    public IProductionEvents<ushort> TargetMaxMaterialTemperature
    {
      get => (IProductionEvents<ushort>) GetEventList(ProductionEventType.TempWarningLevelMaxChange);
    }

    /// <summary>
    /// Records the target lift thickness value configured on the machine control system
    /// </summary>
    public IProductionEvents<float> TargetLiftThicknessStateEvents
    {
      get => (IProductionEvents<float>) GetEventList(ProductionEventType.TargetLiftThickness);
    }

    /// <summary>
    /// Records the Resonance Meter Value jump threshold configured on the machine control system
    /// </summary>
    public IProductionEvents<short> RMVJumpThresholdEvents
    {
      get => (IProductionEvents<short>) GetEventList(ProductionEventType.MachineRMVJumpValueChange);
    }

    /// <summary>
    /// Records the selected Layer ID overriding the id on the machine at the time measurements were being made
    /// </summary>
    public IProductionEvents<OverrideEvent<ushort>> LayerOverrideEvents
    {
      get => (IProductionEvents<OverrideEvent<ushort>>)GetEventList(ProductionEventType.LayerOverride);
    }


    /// <summary>
    /// Records the selected Design overriding the design on the machine at the time measurements were being made
    /// </summary>
    public IProductionEvents<OverrideEvent<int>> DesignOverrideEvents
    {
      get => (IProductionEvents<OverrideEvent<int>>)GetEventList(ProductionEventType.DesignOverride);
    }

    /// <summary>
    /// Retrieves the requested event list for this machine in this site model
    /// Event lists are lazy loaded at the point they are requested.
    /// </summary>
    /// <param name="eventType"></param>
    /// <returns></returns>
    public IProductionEvents GetEventList(ProductionEventType eventType)
    {
      // Check if the request event list type has been instantiated and loaded
      if (allEventsForMachine[(int) eventType] == null)
      {
        // It is not... Instantiate and load the events
        lock (this)
        {
          if (allEventsForMachine[(int) eventType] == null) // This thread won the lock
          {
            var temp = _ProductionEventsFactory.NewEventList(_internalSiteModelMachineIndex, SiteModel.ID, eventType);

            if (temp != null) // The event is supported, load if the model is persistent (non-transient)
            {
              if (!SiteModel.IsTransient)
              {
                temp.LoadFromStore(SiteModel.PrimaryStorageProxy);
              }

              allEventsForMachine[(int) eventType] = temp;
            }
          }
        }
      }

      return allEventsForMachine[(int) eventType];
    }

    /// <summary>
    /// Returns an array containing all the event lists for a machine
    /// </summary>
    /// <returns></returns>
    public IProductionEvents[] GetEventLists() => allEventsForMachine;

    /// <summary>
    /// The count of the number of possible production event types
    /// </summary>
    private static readonly int NumEventListTypes = Enum.GetValues(typeof(ProductionEventType)).Cast<int>().Max() + 1;

    public ProductionEventLists(ISiteModel siteModel, short internalSiteModelMachineIndex)
    {
      SiteModel = siteModel;
      _internalSiteModelMachineIndex = internalSiteModelMachineIndex;

      // Create an array large enough to hold all the possible enumeration values for production events
      allEventsForMachine = new IProductionEvents[NumEventListTypes];
    }

    /// <summary>
    /// Saves the event lists for this machine to the persistent store
    /// </summary>
    public void SaveMachineEventsToPersistentStore(IStorageProxy storageProxy)
    {
      if (SiteModel.IsTransient)
        throw new TRexPersistencyException($"Site model {SiteModel.ID} is a transient site model. Transient site models may not save events to the persistent store.");

      foreach (var list in allEventsForMachine)
      {
        if (list?.EventsChanged == true)
        {
          Log.LogDebug($"Saving {list.EventListType} with {list.Count()} events for machine {InternalSiteModelMachineIndex} in project {SiteModel.ID}");

          list.SaveToStore(storageProxy);
        }
      }
    }

    /// <summary>
    /// Forces all event lists to be loaded for a machine. This is an in-efficient approach and should only be called
    /// if all lists are required, and doing so is more desirable than using lazy loading for event lists
    /// </summary>
    /// <param name="storageProxy"></param>
    /// <returns></returns>
    public bool LoadEventsForMachine(IStorageProxy storageProxy)
    {
      foreach (ProductionEventType evt in ProductionEventTypeValues)
      {
        Log.LogDebug($"Loading {evt} events for machine {_internalSiteModelMachineIndex} in project {SiteModel.ID}");

        GetEventList(evt)?.LoadFromStore(storageProxy);
      }

      return true;
    }

    /// <summary>
    /// Provides a reference to the TAG file processing start/end events list
    /// </summary>
    /// <returns></returns>
    public IProductionEventPairs GetStartEndRecordedDataEvents() => StartEndRecordedDataEvents;

    /// <summary>
    /// Merges machine and override events into a single event list
    /// </summary>
    private IProductionEvents<T> MergeEventLists<T>(IProductionEvents<T> machineEvents, IProductionEvents<OverrideEvent<T>> overrideEvents, ProductionEventType eventType)
    {
      //TODO: Ask Raymond - do we want to store this merged list anywhere?

      if (overrideEvents.Count() == 0)
        return machineEvents;

      var tempList = (IProductionEvents<T>)_ProductionEventsFactory.NewEventList(_internalSiteModelMachineIndex, SiteModel.ID, eventType);
      tempList.CopyEventsFrom(machineEvents);
      for (var i = 0; i < overrideEvents.Count(); i++)
      {
        overrideEvents.GetStateAtIndex(i, out var overrideStartDate, out var overrideState);
        var overrideValue = overrideState.Value;
        var overrideEndDate = overrideState.EndDate;
        var j = tempList.IndexOfClosestEventPriorToDate(overrideEndDate.AddMilliseconds(-1));
        if (j > -1)
        {
          //Remember and clone this last event
          tempList.GetStateAtIndex(j, out _, out var machineValue);
          //Remove all unused events in override interval
          RemovePreviousEvent(tempList, overrideStartDate.AddMilliseconds(-1), overrideEndDate.AddMilliseconds(-1));
          //Add override events and return
          tempList.PutValueAtDate(overrideStartDate, overrideValue);
          tempList.PutValueAtDate(overrideEndDate, machineValue);
        }
        else
        {
          //Add override events and return
          tempList.PutValueAtDate(overrideStartDate,overrideValue);
          tempList.PutValueAtDate(overrideEndDate, overrideValue);
        }
      }

      return tempList;
    }

    /// <summary>
    /// Recursive method to delete elements within the range
    /// </summary>
    private void RemovePreviousEvent<T>(IProductionEvents<T> list, DateTime limitEarliestTime, DateTime currentTime)
    {
      var index = list.IndexOfClosestEventPriorToDate(currentTime);
      if (index < 0)
        return;
      list.GetStateAtIndex(index, out var dateTime, out _);
      if (dateTime <= limitEarliestTime)
        return;
      list.RemoveValueAtDate(dateTime);
      RemovePreviousEvent<T>(list, limitEarliestTime, dateTime);
    }
  }
}
