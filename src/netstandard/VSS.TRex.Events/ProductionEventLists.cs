using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.DI;
using VSS.TRex.Events.Interfaces;
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
    /// The SiteModel these events relate to
    /// </summary>
    private ISiteModel SiteModel { get; set; }

    /// <summary>
    /// The collection of concrete event lists for a machine. Each element in the array is indexed by the
    /// ProductionEventType for that collection of events;
    /// </summary>
    private readonly IProductionEvents[] allEventsForMachine;

    /// <summary>
    /// The ID of the machine these events were recorded by. The ID is the (short) internal machine ID
    /// used within the data model, not the GUID descriptor for the machine
    /// </summary>
    public short MachineID { get; set; }

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
    /// Records the IDs of the designs selected on a machine at the time production measurements were being made
    /// </summary>
    public IProductionEvents<int> DesignNameIDStateEvents
    {
      get => (IProductionEvents<int>) GetEventList(ProductionEventType.DesignChange);
    }

    /// <summary>
    /// Records the state of the automatic machine control on the machine at the time measurements were being made.
    /// </summary>
    public IProductionEvents<MachineAutomaticsMode> MachineAutomaticsStateEvents
    {
      get => (IProductionEvents<MachineAutomaticsMode>) GetEventList(ProductionEventType.MachineAutomaticsChange);
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
    public IProductionEvents<bool> MinElevMappingStateEvents
    {
      get => (IProductionEvents<bool>) GetEventList(ProductionEventType.MinElevMappingStateChange);
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
      get => (IProductionEvents<ushort>) GetEventList(ProductionEventType.LayerID);
    }

    /// <summary>
    /// Records the selected design on the machine at the time the measurements were made
    /// </summary>
    //public IProductionEvents<string> DesignNameStateEvents
    //{
    //  get => (IProductionEvents<string>)GetEventList(ProductionEventType.DesignChange);
    //}

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
    /// Retrieves the requested event list for this meachine in this sitemodel
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
            IProductionEvents temp = DIContext.Obtain<IProductionEventsFactory>().NewEventList(MachineID, SiteModel.ID, eventType);

            if (temp != null) // The event is supported, load if the model is persisent (non-transient)
            {
              if (!SiteModel.IsTransient)
              {
                temp.LoadFromStore(DIContext.Obtain<ISiteModels>().StorageProxy);
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
    private static int NumEventListTypes = Enum.GetValues(typeof(ProductionEventType)).Cast<int>().Max(x => x) + 1;

    public ProductionEventLists(ISiteModel siteModel, short machineID)
    {
      SiteModel = siteModel;
      MachineID = machineID;

      // Create an array large enough to hold all the possible enumeration values for production events
      allEventsForMachine = new IProductionEvents[NumEventListTypes];
    }

    /// <summary>
    /// Saves the event lists for this machine to the persistent store
    /// </summary>
    public void SaveMachineEventsToPersistentStore(IStorageProxy storageProxy)
    {
      if (SiteModel.IsTransient)
      {
        throw new TRexPersistencyException($"Sitemodel {SiteModel.ID} is a transient site model. Transient sitemodels may not save events to the persistent store.");
      }

      foreach (var list in allEventsForMachine)
      {
        if (list?.EventsChanged == true)
        {
          Log.LogDebug($"Saving {list.EventListType} with {list.Count()} events for machine {MachineID} in project {SiteModel.ID}");

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
      foreach (ProductionEventType evt in Enum.GetValues(typeof(ProductionEventType)))
      {
        Log.LogDebug($"Loading {evt} events for machine {MachineID} in project {SiteModel.ID}");

        GetEventList(evt)?.LoadFromStore(storageProxy);
      }

      return true;
    }

    /// <summary>
    /// Provides a refernece to the TAG file processing start/end events list
    /// </summary>
    /// <returns></returns>
    public IProductionEventPairs GetStartEndRecordedDataEvents() => StartEndRecordedDataEvents;
  }
}
