using System;
using System.Collections.Generic;
using VSS.MasterData.Models.Models;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Types;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Geometry;
using VSS.TRex.TAGFiles.Classes.ValueMatcher;
using VSS.TRex.TAGFiles.Types;
using VSS.TRex.Types;

namespace VSS.TRex.TAGFiles.Classes.States
{
  /// <summary>
  /// TAGProcessorStateBase implements a basic TAG value reader sink with all the
  /// state information read from tag files representing as-built and compaction
  /// recorded information.
  /// </summary>
  public class TAGProcessorStateBase
  {
    private bool _HaveSeenFirstDataTime; 
    private DateTime _DataTimePrevious = Consts.MIN_DATETIME_AS_UTC;

    private XYZ _LeftPoint = XYZ.Null;
    private XYZ _RightPoint = XYZ.Null;
    private XYZ _LeftTrackPoint = XYZ.Null;
    private XYZ _RightTrackPoint = XYZ.Null;
    private XYZ _LeftWheelPoint = XYZ.Null;
    private XYZ _RightWheelPoint = XYZ.Null;
    private XYZ _LeftRearPoint = XYZ.Null;
    private XYZ _RightRearPoint = XYZ.Null;

    // Declarations that hold values from read records & declarations for processing state information

    // CalculateMachineSpeed calculates the speed of the machine in meters per second
    private double CalculateMachineSpeed()
    {
      XYZ CentrePointFrom;
      XYZ CentrePointTo;

      if (!_LeftPoint.IsNull && !_RightPoint.IsNull && !DataLeft.IsNull && !DataRight.IsNull)
      {
        CentrePointFrom = (_LeftPoint + _RightPoint) * 0.5;
        CentrePointTo = (DataLeft + DataRight) * 0.5;
      }
      else
      if (!_LeftTrackPoint.IsNull && !_RightTrackPoint.IsNull && !DataTrackLeft.IsNull && !DataTrackRight.IsNull)
      {
        CentrePointFrom = (_LeftTrackPoint + _RightTrackPoint) * 0.5;
        CentrePointTo = (DataTrackLeft + DataTrackRight) * 0.5;
      }
      else
      if (!_LeftWheelPoint.IsNull && !_RightWheelPoint.IsNull && !DataWheelLeft.IsNull && !DataWheelRight.IsNull)
      {
        CentrePointFrom = (_LeftWheelPoint + _RightWheelPoint) * 0.5;
        CentrePointTo = (DataWheelLeft + DataWheelRight) * 0.5;
      }
      else
      {
        return Consts.NullDouble;
      }

      double DistanceTraveled = XYZ.Get3DLength(CentrePointFrom, CentrePointTo); // meters converted to kilometers...
      double TravelTime = (_DataTime - _DataTimePrevious).TotalMilliseconds / 1000;   // milliseconds converted to seconds...

      return TravelTime > 0 ? DistanceTraveled / TravelTime : 0.0;
    }

    private bool GetLLHReceived() => _LLHLat != Consts.NullDouble && _LLHLon != Consts.NullDouble && _LLHHeight != Consts.NullDouble;

    private bool GetGPSBaseLLHReceived() => (GPSBaseLat != Consts.NullDouble) && (GPSBaseLon != Consts.NullDouble) && (GPSBaseHeight != Consts.NullDouble);

    protected bool _HaveFirstEpoch;
    protected bool _HaveFirstRearEpoch;
    protected bool _HaveFirstTrackEpoch;
    protected bool _HaveFirstWheelEpoch;

    protected void InitialiseAttributeAccumulators()
    {
      _ICMachineSpeedValues.Add(Consts.MIN_DATETIME_AS_UTC, Consts.NullDouble);
      _ICCCVValues.Add(Consts.MIN_DATETIME_AS_UTC, CellPassConsts.NullCCV);
      _ICRMVValues.Add(Consts.MIN_DATETIME_AS_UTC, CellPassConsts.NullRMV);
      _ICFrequencys.Add(Consts.MIN_DATETIME_AS_UTC, CellPassConsts.NullFrequency);
      _ICAmplitudes.Add(Consts.MIN_DATETIME_AS_UTC, CellPassConsts.NullAmplitude);
      _GPSModes.Add(Consts.MIN_DATETIME_AS_UTC, CellPassConsts.NullGPSMode);

      // We will assume that the absence of an OnGround flag in the tag file shall
      // default to true wrt to the processing of the file.
      _OnGrounds.Add(Consts.MIN_DATETIME_AS_UTC, OnGroundState.YesLegacy);

      _AgeOfCorrections.Add(Consts.MIN_DATETIME_AS_UTC, 0);

      _VolkelMeasureRanges.Add(Consts.MIN_DATETIME_AS_UTC, CellPassConsts.NullVolkelMeasRange);
      _VolkelMeasureUtilRanges.Add(Consts.MIN_DATETIME_AS_UTC, CellPassConsts.NullVolkelMeasUtilRange);
      _ICMDPValues.Add(Consts.MIN_DATETIME_AS_UTC, CellPassConsts.NullMDP);
      _ICTemperatureValues.Add(Consts.MIN_DATETIME_AS_UTC, CellPassConsts.NullMaterialTemperatureValue);
      _ICCCAValues.Add(Consts.MIN_DATETIME_AS_UTC, CellPassConsts.NullCCA);
      _ICCCALeftFrontValues.Add(Consts.MIN_DATETIME_AS_UTC, CellPassConsts.NullCCA);
      _ICCCARightFrontValues.Add(Consts.MIN_DATETIME_AS_UTC, CellPassConsts.NullCCA);
      _ICCCALeftRearValues.Add(Consts.MIN_DATETIME_AS_UTC, CellPassConsts.NullCCA);
      _ICCCALeftRearValues.Add(Consts.MIN_DATETIME_AS_UTC, CellPassConsts.NullCCA);
    }

    protected virtual void DiscardAllButLatestAttributeAccumulatorValues()
    {
      _ICMachineSpeedValues.DiscardAllButLatest();
      _ICCCVValues.DiscardAllButLatest();
      _ICRMVValues.DiscardAllButLatest();
      _ICFrequencys.DiscardAllButLatest();
      _ICAmplitudes.DiscardAllButLatest();
      _GPSModes.DiscardAllButLatest();
      _OnGrounds.DiscardAllButLatest();
      _AgeOfCorrections.DiscardAllButLatest();
      _VolkelMeasureRanges.DiscardAllButLatest();
      _VolkelMeasureUtilRanges.DiscardAllButLatest();
      _ICMDPValues.DiscardAllButLatest();
      _ICCCAValues.DiscardAllButLatest();
      _ICCCALeftFrontValues.DiscardAllButLatest();
      _ICCCARightFrontValues.DiscardAllButLatest();
      _ICCCALeftRearValues.DiscardAllButLatest();
      _ICCCARightRearValues.DiscardAllButLatest();
      _ICTemperatureValues.DiscardAllButLatest();
    }

    protected virtual void SetDataTime(DateTime value)
    {
      _DataTime = value;

      if (!_HaveSeenFirstDataTime)
      {
        _HaveSeenFirstDataTime = true;
        _FirstDataTime = _DataTime;
      }
    }
    protected virtual void SetDesign(string value) => _Design = value;
    protected virtual void SetICMode(byte value) => _ICMode = value;

    protected virtual void SetICGear(MachineGear value)
    {
      _ICGear = value;

      if (value != MachineGear.SensorFailedDeprecated)
      {
        GearValueReceived = true;
      }
    }

    protected virtual void SetICSonic3D(byte value) => _ICSonic3D = value;
    protected virtual void SetICCCVTargetValue(short value) => _ICCCVTargetValue = value;
    protected virtual void SetICMDPTargetValue(short value) => _ICMDPTargetValue = value;
    protected virtual void SetICPassTargetValue(ushort value) => _ICPassTargetValue = value;
    protected virtual void SetICLayerIDValue(ushort value) => _ICLayerIDValue = value;
    protected virtual void SetStartProofingDataTime(DateTime value) => _StartProofingDataTime = value;
    protected virtual void SetStartProofing(string value) => _StartProofing = value;
    protected virtual void SetICTargetLiftThickness(float value) => _ICTargetLiftThickness = value;
    protected virtual void SetApplicationVersion(string value) => _ApplicationVersion = value;
    protected virtual void SetAutomaticsMode(AutomaticsType value) => _AutomaticsMode = value;
    protected virtual void SetRMVJumpThresholdValue(short value) => _RMVJumpThreshold = value;
    protected virtual void SetICSensorType(CompactionSensorType value) => _ICSensorType = value;
    protected virtual void SetICTempWarningLevelMinValue(ushort value) => _ICTempWarningLevelMinValue = value;
    protected virtual void SetICTempWarningLevelMaxValue(ushort value) => _ICTempWarningLevelMaxValue = value;
    protected virtual void SetICCCATargetValue(byte value) => _ICCCATargetValue = value;
    protected virtual void SetUTMZone(byte value) => _UTMZone = value;
    protected virtual void SetCSType(CoordinateSystemType value) => _CSType = value;

    protected void UpdateAutomaticsMode()
    {
      const int kMachineIsInAutomaticsModeFlags = MachineControlStateFlags.GCSControlStateAuto |
                                                  MachineControlStateFlags.GCSControlStateInActiveAuto |
                                                  MachineControlStateFlags.GCSControlStateAutoValueNotDriving;

      AutomaticsType NewAutomaticsModeState;

      AutomaticsType OldAutomaticsModeState = _AutomaticsMode;

      //   implement unknown automatics state for initialisation purposes

      if (((_ControlStateLeftLift & kMachineIsInAutomaticsModeFlags) != 0) ||
         ((_ControlStateRightLift & kMachineIsInAutomaticsModeFlags) != 0) ||
         ((_ControlStateLift & kMachineIsInAutomaticsModeFlags) != 0) ||
         ((_ControlStateTilt & kMachineIsInAutomaticsModeFlags) != 0) ||
         ((_ControlStateSideShift & kMachineIsInAutomaticsModeFlags) != 0))
      {
        NewAutomaticsModeState = AutomaticsType.Automatics;
      }
      else
      {
        NewAutomaticsModeState = AutomaticsType.Manual;
      }

      if (OldAutomaticsModeState != NewAutomaticsModeState)
      {
        _AutomaticsMode = NewAutomaticsModeState;
      }
    }

    public void SetControlStateLeftLift(int value)
    {
      _ControlStateLeftLift = value;
      UpdateAutomaticsMode();
    }

    public void SetControlStateLift(int value)
    {
      _ControlStateLift = value;
      UpdateAutomaticsMode();
    }

    public void SetControlStateRightLift(int value)
    {
      _ControlStateRightLift = value;
      UpdateAutomaticsMode();
    }

    public void SetControlStateSideShift(int value)
    {
      _ControlStateSideShift = value;
      UpdateAutomaticsMode();
    }

    public void SetControlStateTilt(int value)
    {
      _ControlStateTilt = value;
      UpdateAutomaticsMode();
    }

    public virtual void SetMachineDirection(MachineDirection value)
    {
      if (!GearValueReceived)
      {
        if (value == MachineDirection.Forward)
        {
          _ICGear = MachineGear.Forward;
        }
        else
        if (value == MachineDirection.Reverse)
        {
          _ICGear = MachineGear.Reverse;
        }
      }
    }

    public MachineDirection GetMachineDirection()
    {
      if (_ICGear == MachineGear.Forward ||
          _ICGear == MachineGear.Forward2 ||
          _ICGear == MachineGear.Forward3 ||
          _ICGear == MachineGear.Forward4 ||
          _ICGear == MachineGear.Forward5)
      {
        return MachineDirection.Forward;
      }

      if (_ICGear == MachineGear.Reverse ||
          _ICGear == MachineGear.Reverse2 ||
          _ICGear == MachineGear.Reverse3 ||
          _ICGear == MachineGear.Reverse4 ||
          _ICGear == MachineGear.Reverse5)
      {
        return MachineDirection.Reverse;
      }

      return MachineDirection.Unknown;
    }

    public MachineControlPlatformType GetPlatformType()
    {
      return MachineSerialUtilities.MapSerialToModel(HardwareID);
    }

    public virtual void SetResearchData(bool value) => _ResearchData = value;
    public virtual void SetUsingCCA(bool value) => _UsingCCA = value;

    /// <summary>
    /// Determine if valid machine implement tip or front axle positions have been received
    /// </summary>
    /// <returns></returns>
    public bool HaveReceivedValidTipPositions => !(DataLeft.IsNull || DataRight.IsNull);

    /// <summary>
    /// Determine if valid machine track positions have been received
    /// </summary>
    /// <returns></returns>
    public bool HaveReceivedValidTrackPositions => !(DataTrackLeft.IsNull || DataTrackRight.IsNull);

    /// <summary>
    /// Determine if valid machine wheel positions have been received
    /// </summary>
    /// <returns></returns>
    public bool HaveReceivedValidWheelPositions => !(DataWheelLeft.IsNull || DataWheelRight.IsNull);

    /// <summary>
    /// Determine if valid rear axle positions have been received
    /// </summary>
    /// <returns></returns>
    public bool HaveReceivedValidRearPositions => !(DataRearLeft.IsNull || DataRearRight.IsNull);

    //////////////////// Public properties
    public List<UTMCoordPointPair> ConvertedBladePositions = new List<UTMCoordPointPair>();
    public List<UTMCoordPointPair> ConvertedRearAxlePositions = new List<UTMCoordPointPair>();
    public List<UTMCoordPointPair> ConvertedTrackPositions = new List<UTMCoordPointPair>();
    public List<UTMCoordPointPair> ConvertedWheelPositions = new List<UTMCoordPointPair>();

    // FFirstLeftPoint and FFirstRightPoint record the grid positions of the
    // first epoch in the TAG file. One use of this if for comparison against
    // a grid coordinate project boundary to see if the initial position lies
    // the project.
    public XYZ FirstAccurateLeftPoint = XYZ.Null;
    public XYZ FirstAccurateRightPoint = XYZ.Null;

    public bool HaveFirstAccurateGridEpochEndPoints;

    public int ProcessedEpochCount;
    public int ProcessedCellPassesCount;
    public int VisitedEpochCount;

    public short GPSWeekNumber;
    public uint GPSWeekTime;

    protected DateTime _DataTime = Consts.MIN_DATETIME_AS_UTC;
    public DateTime DataTime { get { return _DataTime; } set { SetDataTime(value); } }

    public DateTime _FirstDataTime;
    public DateTime FirstDataTime { get { return _FirstDataTime; } }

    public XYZ DataLeft = XYZ.Null;
    public XYZ DataRight = XYZ.Null;

    public XYZ DataTrackLeft = XYZ.Null;
    public XYZ DataTrackRight = XYZ.Null;


    public XYZ DataWheelLeft = XYZ.Null;
    public XYZ DataWheelRight = XYZ.Null;

    public XYZ DataRearLeft = XYZ.Null;
    public XYZ DataRearRight = XYZ.Null;

    private AccumulatedAttributes<GPSMode> _GPSModes  = new AccumulatedAttributes<GPSMode>();
    public AccumulatedAttributes<GPSMode> GPSModes { get => _GPSModes; }

    private AccumulatedAttributes<OnGroundState> _OnGrounds = new AccumulatedAttributes<OnGroundState>();
    public AccumulatedAttributes<OnGroundState> OnGrounds { get => _OnGrounds; }

    private AccumulatedAttributes<byte> _AgeOfCorrections = new AccumulatedAttributes<byte>();

    public AccumulatedAttributes<byte> AgeOfCorrections { get => _AgeOfCorrections; }

    //  ValidPosition is only used in terms of the most recent epoch and do not need to have the history of these
    // values maintained in a TAccumulatedAttributeList
    public byte ValidPosition;

    public ElevationMappingMode ElevationMappingMode;
    public byte InAvoidZone;

    public GPSAccuracy GPSAccuracy = GPSAccuracy.Unknown;
    public ushort GPSAccuracyErrorLimit = CellPassConsts.NullGPSTolerance;

    public MachineDirection MachineDirection { get { return GetMachineDirection(); } set { SetMachineDirection(value); } }

    public MachineType MachineType = CellPassConsts.MachineTypeNull;

    protected string _Design = string.Empty;
    public string Design { get { return _Design; } set { SetDesign(value); } }

    public string MachineID  = string.Empty;
    public string HardwareID = string.Empty;
    public uint Sequence;

    private AccumulatedAttributes<short> _ICCCVValues = new AccumulatedAttributes<short>();
    public AccumulatedAttributes<short> ICCCVValues { get => _ICCCVValues; }

    private AccumulatedAttributes<double> _ICMachineSpeedValues = new AccumulatedAttributes<double>();
    public AccumulatedAttributes<double> ICMachineSpeedValues { get => _ICMachineSpeedValues; }

    protected short _ICCCVTargetValue = CellPassConsts.NullCCV;
    public short ICCCVTargetValue { get { return _ICCCVTargetValue; } set { SetICCCVTargetValue(value); } }

    protected ushort _ICPassTargetValue;
    public ushort ICPassTargetValue { get { return _ICPassTargetValue; } set { SetICPassTargetValue(value); } }

    protected ushort _ICLayerIDValue = CellPassConsts.NullLayerID;
    public ushort ICLayerIDValue { get { return _ICLayerIDValue; } set { SetICLayerIDValue(value); } }

    protected AccumulatedAttributes<short> _ICRMVValues = new AccumulatedAttributes<short>();
    public AccumulatedAttributes<short> ICRMVValues { get => _ICRMVValues; }

    protected short _RMVJumpThreshold = CellPassConsts.NullRMV;
    public short ICRMVJumpthreshold { get { return _RMVJumpThreshold; } set { SetRMVJumpThresholdValue(value); } }

    private AccumulatedAttributes<ushort> _ICFrequencys = new AccumulatedAttributes<ushort>();
    public AccumulatedAttributes<ushort> ICFrequencys { get => _ICFrequencys; } 

    private AccumulatedAttributes<ushort> _ICAmplitudes { get; } = new AccumulatedAttributes<ushort>();
    public AccumulatedAttributes<ushort> ICAmplitudes { get => _ICAmplitudes; }

    protected MachineGear _ICGear = CellPassConsts.NullMachineGear;
    public MachineGear ICGear { get { return _ICGear; } set { SetICGear(value); } }

    protected byte _ICSonic3D = CellPassConsts.Null3DSonic;
    public byte ICSonic3D { get { return _ICSonic3D; } set { SetICSonic3D(value); } }

    protected byte _ICMode = ICModeFlags.IC_UNKNOWN_INVALID_MC0243_SENSOR_FLAG;
    public byte ICMode { get { return _ICMode; } set { SetICMode(value); } }

    protected CompactionSensorType _ICSensorType = CompactionSensorType.NoSensor;
    public CompactionSensorType ICSensorType { get { return _ICSensorType; } set { SetICSensorType(value); } }

    private AccumulatedAttributes<short> _ICMDPValues = new AccumulatedAttributes<short>();
    public AccumulatedAttributes<short> ICMDPValues { get => _ICMDPValues; }

    protected short _ICMDPTargetValue = CellPassConsts.NullMDP;
    public short ICMDPTargetValue { get { return _ICMDPTargetValue; } set { SetICMDPTargetValue(value); } }

    protected AccumulatedAttributes<byte> _ICCCAValues = new AccumulatedAttributes<byte>();
    public AccumulatedAttributes<byte> ICCCAValues { get => _ICCCAValues; }

    protected AccumulatedAttributes<byte> _ICCCALeftFrontValues = new AccumulatedAttributes<byte>();
    public AccumulatedAttributes<byte> ICCCALeftFrontValues { get => _ICCCALeftFrontValues; }

    protected AccumulatedAttributes<byte> _ICCCARightFrontValues = new AccumulatedAttributes<byte>();
    public AccumulatedAttributes<byte> ICCCARightFrontValues { get => _ICCCARightFrontValues; }

    protected AccumulatedAttributes<byte> _ICCCALeftRearValues = new AccumulatedAttributes<byte>();
    public AccumulatedAttributes<byte> ICCCALeftRearValues { get => _ICCCALeftRearValues; }

    protected AccumulatedAttributes<byte> _ICCCARightRearValues = new AccumulatedAttributes<byte>();
    public AccumulatedAttributes<byte> ICCCARightRearValues { get => _ICCCARightRearValues; }

    protected byte _ICCCATargetValue = CellPassConsts.NullCCA;
    public byte ICCCATargetValue { get { return _ICCCATargetValue; } set { SetICCCATargetValue(value); } }

    protected AccumulatedAttributes<ushort> _ICTemperatureValues = new AccumulatedAttributes<ushort>();
    public AccumulatedAttributes<ushort> ICTemperatureValues { get => _ICTemperatureValues; }

    protected ushort _ICTempWarningLevelMinValue = CellPassConsts.NullMaterialTemperatureValue;
    public ushort ICTempWarningLevelMinValue { get { return _ICTempWarningLevelMinValue; } set { SetICTempWarningLevelMinValue(value); } }

    protected ushort _ICTempWarningLevelMaxValue = CellPassConsts.NullMaterialTemperatureValue;
    public ushort ICTempWarningLevelMaxValue { get { return _ICTempWarningLevelMaxValue; } set { SetICTempWarningLevelMaxValue(value); } }

    protected AccumulatedAttributes<byte> _VolkelMeasureRanges = new AccumulatedAttributes<byte>();
    public AccumulatedAttributes<byte> VolkelMeasureRanges { get => _VolkelMeasureRanges; }

    protected AccumulatedAttributes<int> _VolkelMeasureUtilRanges = new AccumulatedAttributes<int>();
    public AccumulatedAttributes<int> VolkelMeasureUtilRanges { get => _VolkelMeasureUtilRanges; }

    // FApplicationVersion is the version of the application reported in the TAG file returned by the machine.
    protected string _ApplicationVersion = string.Empty;
    public string ApplicationVersion { get { return _ApplicationVersion; } set { SetApplicationVersion(value); } }

    protected double _CalculatedMachineSpeed = Consts.NullDouble;
    public double CalculatedMachineSpeed { get => _CalculatedMachineSpeed; }

    protected string _StartProofing = string.Empty;    // Proofing run name...
    public string StartProofing { get => _StartProofing; set =>  SetStartProofing(value); }

    // Proofing runs declarations...
    
    /// <summary>
    /// Proofing time is GPS time in milliseconds
    /// </summary>
    protected uint _StartProofingTime;
    public uint StartProofingTime { get => _StartProofingTime; set => _StartProofingTime = value; }

    protected short _StartProofingWeek;
    public short StartProofingWeek { get => _StartProofingWeek; set => _StartProofingWeek = value; }

    protected string _EndProofingName = string.Empty;
    public string EndProofingName { get => _EndProofingName; set => _EndProofingName = value; }

    protected DateTime _StartProofingDataTime = Consts.MIN_DATETIME_AS_UTC;
    public DateTime StartProofingDataTime { get { return _StartProofingDataTime; } set { SetStartProofingDataTime(value); } }

    protected float _ICTargetLiftThickness = CellPassConsts.NullOverridingTargetLiftThicknessValue;
    public float ICTargetLiftThickness { get { return _ICTargetLiftThickness; } set { SetICTargetLiftThickness(value); } }

    public BoundingWorldExtent3D ProofingRunExtent = BoundingWorldExtent3D.Inverted();
    public BoundingWorldExtent3D DesignExtent = BoundingWorldExtent3D.Inverted();

    // The control state members contain the control state flags set for five different
    // automatics controls supported by the GCS900 machine control system
    protected int _ControlStateLeftLift = MachineControlStateFlags.NullGCSControlState;
    public int ControlStateLeftLift { get { return _ControlStateLeftLift; } set { SetControlStateLeftLift(value); } }

    protected int _ControlStateRightLift = MachineControlStateFlags.NullGCSControlState;
    public int ControlStateRightLift { get { return _ControlStateRightLift; } set { SetControlStateRightLift(value); } }

    protected int _ControlStateLift = MachineControlStateFlags.NullGCSControlState;
    public int ControlStateLift { get { return _ControlStateLift; } set { SetControlStateLift(value); } }

    protected int _ControlStateTilt = MachineControlStateFlags.NullGCSControlState;
    public int ControlStateTilt { get { return _ControlStateTilt; } set { SetControlStateTilt(value); } }

    protected int _ControlStateSideShift = MachineControlStateFlags.NullGCSControlState;
    public int ControlStateSideShift { get { return _ControlStateSideShift; } set { SetControlStateSideShift(value); } }

    // _AutomaticsMode records the machine automatic control state as defined by
    // the 5 GCS900 control state flag sets. It is currently defined as a simple
    // on/off switch. The UpdateAutomaticsMode method examines the individual
    // control states and sets the value of this accordingly.
    AutomaticsType _AutomaticsMode = AutomaticsType.Unknown;
    public AutomaticsType AutomaticsMode { get { return _AutomaticsMode; } set { SetAutomaticsMode(value); } }

    // FMachineWheelWidth records the width of wheels on wheeled machines.
    // Units are meters
    public double MachineWheelWidth;

    // Indicates that we've received a machine Gear value from the tag file.
    // If not, then if we encounter a Direction value, use that to populate the machine Gear.
    public bool GearValueReceived;
    public PositioningTech PositioningTech = PositioningTech.Unknown;

    // Serial of the IP radio, expected to be unique for a given Radio Type
    public string RadioSerial = string.Empty;
    // Type of IP radio, e.g. torch
    public string RadioType = string.Empty;

    protected double _LLHLat = Consts.NullDouble;
    public double LLHLat { get => _LLHLat; set => SetLLHLat(value); } 
    private void SetLLHLat(double value)
    {
      _LLHLat = value;
      LLHLatRecordedTime = DataTime;
    }

    protected double _LLHLon = Consts.NullDouble;
    public double LLHLon { get => _LLHLon; set => SetLLHLon(value); }

    private void SetLLHLon(double value)
    {
      _LLHLon = value;
      LLHLonRecordedTime = DataTime;
    }

    protected double _LLHHeight = Consts.NullDouble;
    public double LLHHeight { get => _LLHHeight; set => SetLLHHeight(value); }

    private void SetLLHHeight(double value)
    {
      _LLHHeight = value;
      LLHHeightRecordedTime = DataTime;
    }

    public DateTime? LLHLatRecordedTime;
    public DateTime? LLHLonRecordedTime;
    public DateTime? LLHHeightRecordedTime;


    public bool LLHReceived => GetLLHReceived();

    protected byte _UTMZone = CellPassConsts.NullUTMZone;
    public byte UTMZone { get { return _UTMZone; } set { SetUTMZone(value); } }

    protected CoordinateSystemType _CSType = CoordinateSystemType.NoCoordSystem;
    public CoordinateSystemType CSType { get { return _CSType; } set { SetCSType(value); } }

    public double GPSBaseLat = Consts.NullDouble;
    public double GPSBaseLon = Consts.NullDouble;
    public double GPSBaseHeight = Consts.NullDouble;

    public bool IsCSIBCoordSystemTypeOnly = true;
    public byte UTMZoneAtFirstPosition;

    public bool GPSBaseLLHReceived => GetGPSBaseLLHReceived();

    public bool OnGroundFlagSet;


    protected bool _ResearchData;
    public bool ResearchData { get { return _ResearchData; } set { SetResearchData(value); } }

    protected bool _UsingCCA;
    public bool UsingCCA { get { return _UsingCCA; } set { SetUsingCCA(value); } }

    //////////////////////// Public procedures

    public virtual void SetGPSMode(GPSMode value) => _GPSModes.Add(_DataTime, value);

    public virtual void SetOnGround(OnGroundState value)
    {
      if (value != OnGroundState.No)
      {
        OnGroundFlagSet = true;
      }
      OnGrounds.Add(DataTime, value);
    }

    public virtual void SetICCCVValue(short value) => _ICCCVValues.Add(_DataTime, value);
    public virtual void SetICMachineSpeedValue(double value) => _ICMachineSpeedValues.Add(_DataTime, value);
    public virtual void SetICFrequency(ushort value) => _ICFrequencys.Add(_DataTime, value);
    public virtual void SetICAmplitude(ushort value) => _ICAmplitudes.Add(_DataTime, value);
    public virtual void SetICRMVValue(short value) => _ICRMVValues.Add(_DataTime, value);
    public virtual void SetAgeOfCorrection(byte value) => _AgeOfCorrections.Add(_DataTime, value);
    public virtual void SetVolkelMeasRange(byte value) => _VolkelMeasureRanges.Add(_DataTime, value);
    public virtual void SetVolkelMeasUtilRange(int value) => _VolkelMeasureUtilRanges.Add(_DataTime, value);
    public virtual void SetElevationMappingModeState(ElevationMappingMode value) => ElevationMappingMode = value;
    public virtual void SetInAvoidZoneState(byte value) => InAvoidZone = value;
    public virtual void SetPositioningTechState(PositioningTech value) => PositioningTech = value;
    public virtual void SetGPSAccuracyState(GPSAccuracy AccValue, ushort LimValue)
    {
      GPSAccuracy = AccValue;
      GPSAccuracyErrorLimit = LimValue;
    }
    public virtual void SetICMDPValue(short value) => _ICMDPValues.Add(_DataTime, value);
    public virtual void SetICCCAValue(byte value) => _ICCCAValues.Add(_DataTime, value);
    public virtual void SetICCCARightFrontValue(byte value) => _ICCCARightFrontValues.Add(_DataTime, value);
    public virtual void SetICCCALeftFrontValue(byte value) => _ICCCALeftFrontValues.Add(_DataTime, value);
    public virtual void SetICCCARightRearValue(byte value) => _ICCCARightRearValues.Add(_DataTime, value);
    public virtual void SetICCCALeftRearValue(byte value) => _ICCCALeftRearValues.Add(_DataTime, value);


    public virtual void SetICTemperatureValue(ushort value) => _ICTemperatureValues.Add(_DataTime, value);

    /// <summary>
    /// TAG Processor state base constructor. 
    /// Initializes the attribute consumers. All other state is initialized inline.
    /// </summary>
    public TAGProcessorStateBase()
    {
      InitialiseAttributeAccumulators();
    }

    /// <summary>
    /// ProcessContext performs processing across a
    /// context consisting of a pair of data epochs read in from the snail trail file.
    ///This function returns false if there was an error in processing the context.
    /// </summary>
    /// <returns></returns>
    public virtual bool ProcessEpochContext()
    {
      _CalculatedMachineSpeed = CalculateMachineSpeed();

      _LeftPoint = DataLeft;
      _RightPoint = DataRight;

      _LeftTrackPoint = DataTrackLeft;
      _RightTrackPoint = DataTrackRight;

      _LeftWheelPoint = DataWheelLeft;
      _RightWheelPoint = DataWheelRight;

      _LeftRearPoint = DataRearLeft;
      _RightRearPoint = DataRearRight;

      GPSMode gPSMode = _GPSModes.GetValueAtDateTime(_DataTime, CellPassConsts.NullGPSMode);

      // Check to see if the current blade epoch position is 'accurate' (essentially, not 'Autonomous' if GPS; we assume UTS is accurate enough regardless)
      // and if so save it for future use
      if (!HaveFirstAccurateGridEpochEndPoints &&
          (PositioningTech == PositioningTech.UTS) ||
            ((PositioningTech == PositioningTech.GPS) &&
             (gPSMode == GPSMode.Float || gPSMode == GPSMode.Fixed || gPSMode == GPSMode.DGPS || gPSMode == GPSMode.SBAS || gPSMode == GPSMode.LocationRTK)))
      {
        if (!_LeftPoint.IsNullInPlan && !_RightPoint.IsNullInPlan)
        {
          FirstAccurateLeftPoint = _LeftPoint;
          FirstAccurateRightPoint = _RightPoint;

          HaveFirstAccurateGridEpochEndPoints = true;
        }
      }

      _DataTimePrevious = _DataTime;

      return true;
    }

    /// <summary>
    /// DoEpochStateEvent is called to handle epoch state events read from the  production tag files.
    /// </summary>
    /// <returns></returns>
    public virtual bool DoEpochStateEvent(EpochStateEvent eventType)
    {
      // No processing is performed in the base class.
      return true;
    }

    /// <summary>
    /// Takes copies of the arrays of positions obtained from TAG values that have been produced by transformation of the grid
    /// coordinate system they were measured in, into the coordinate system of the project the data is being processed into.
    /// </summary>
    /// <param name="convertedBladePositions"></param>
    /// <param name="convertedRearAxlePositions"></param>
    /// <param name="convertedTrackPositions"></param>
    /// <param name="convertedWheelPositions"></param>
    public void PopulateConvertedBladeAndRearTypePositions(List<UTMCoordPointPair> convertedBladePositions,
      List<UTMCoordPointPair> convertedRearAxlePositions, List<UTMCoordPointPair> convertedTrackPositions, List<UTMCoordPointPair> convertedWheelPositions)
    {
      ConvertedBladePositions.AddRange(convertedBladePositions);
      ConvertedRearAxlePositions.AddRange(convertedRearAxlePositions);
      ConvertedTrackPositions.AddRange(convertedTrackPositions);
      ConvertedWheelPositions.AddRange(convertedWheelPositions);
    }

    /// <summary>
    /// Get the latest known speed for the machine. This will come from machine reported speed values if available,
    /// otherwise the speed will be calculated from measurement epochs in the TAG value.
    /// </summary>
    /// <returns></returns>
    public double GetLatestMachineSpeed()
    {
      double result = (double)_ICMachineSpeedValues.GetLatest();
      if (result == Consts.NullDouble)
      {
        result = _CalculatedMachineSpeed;
      }

      return result;
    }
  }
}

