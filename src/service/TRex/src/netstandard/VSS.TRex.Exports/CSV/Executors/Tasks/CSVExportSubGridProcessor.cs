using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.DI;
using VSS.TRex.Exports.CSV.GridFabric;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Client.Types;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.Types;

namespace VSS.TRex.Exports.CSV.Executors.Tasks
{
  public class CSVExportSubGridProcessor
  {
    private static ILogger log = Logging.Logger.CreateLogger<CSVExportSubGridProcessor>();

    private IConvertCoordinates _convertCoordinates = DIContext.Obtain<IConvertCoordinates>();

    private readonly int _maxExportRows;
    private int _totalRowCountSoFar;

    public bool RecordCountLimitReached() { return _totalRowCountSoFar >= _maxExportRows;}

    private readonly CSVExportRequestArgument _requestArgument;
    private readonly CSVExportFormatter _csvExportFormatter;
    private readonly ISiteModel _siteModel;

    private XYZ[] _llhCoords = null;
    private DateTime _runningLastPassTime = Consts.MIN_DATETIME_AS_UTC;
    private string _cellPassTimeString;
    private double _runningNorthing = Consts.NullDouble;
    private double _runningEasting = Consts.NullDouble;
    private string _coordString;
    private float _runningHeight = Consts.NullHeight;
    private string _heightString;
    private int _runningDesignNameId = Consts.kNoDesignNameID;
    private string _lastDesignNameString;
    private short _runningMachineId = -1;
    private string _lastMachineNameString;
    private ushort _runningMachineSpeed = Consts.NullMachineSpeed;
    private string _machineSpeedString;
    private GPSMode _runningGpsMode = GPSMode.NoGPS;
    private string _gpsModeString = "Not_Applicable";
    private Types.GPSAccuracy _runningGpsAccuracy = Types.GPSAccuracy.Unknown;
    private ushort _runningGpsTolerance = CellPassConsts.NullGPSTolerance;
    private string _gpsAccuracyToleranceString;
    private int _runningTargetPassCount = CellPassConsts.NullPassCountValue;
    private string _targetPassCountString;
    private short _runningLastPassValidCcv = CellPassConsts.NullCCV;
    private string _lastPassValidCcvString;
    private short _runningTargetCcv = CellPassConsts.NullCCV;
    private string _lastTargetCcvString;
    private short _runningLastPassValidMdp = CellPassConsts.NullCCV;
    private string _lastPassValidMdpString;
    private short _runningTargetMdp = CellPassConsts.NullCCV;
    private string _lastTargetMdpString;
    private short _runningValidRmv = CellPassConsts.NullCCV;
    private string _lastValidRmvString;
    private ushort _runningValidFreq = CellPassConsts.NullFrequency;
    private string _lastValidFreqString;
    private ushort _runningValidAmp = CellPassConsts.NullAmplitude;
    private string _lastValidAmpString;
    private float _runningTargetThickness = CellPassConsts.NullOverridingTargetLiftThicknessValue;
    private string _lastTargetThicknessString;
    private MachineGear _runningEventMachineGear = MachineGear.Null;
    private string _lastEventMachineGearString;
    private VibrationState _runningEventVibrationState = VibrationState.Invalid;
    private string _lastEventVibrationStateString;
    private ushort _runningLastPassValidTemperature = CellPassConsts.NullMaterialTemperatureValue;
    private string _lastPassValidTemperatureString;

    public CSVExportSubGridProcessor(CSVExportRequestArgument requestArgument)
    {
      _maxExportRows = DIContext.Obtain<IConfigurationStore>().GetValueInt("MAX_EXPORT_ROWS", Consts.DEFAULT_MAX_EXPORT_ROWS);
      _requestArgument = requestArgument;
      _csvExportFormatter = new CSVExportFormatter(requestArgument.UserPreferences, requestArgument.OutputType, requestArgument.RawDataAsDBase);
      _siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(requestArgument.ProjectID);
      _cellPassTimeString = _coordString = _heightString = _lastDesignNameString = _lastMachineNameString =
        _machineSpeedString = _gpsAccuracyToleranceString = _targetPassCountString = _lastPassValidCcvString = 
        _lastTargetCcvString = _lastPassValidMdpString = _lastTargetMdpString = _lastValidRmvString = 
        _lastValidFreqString = _lastValidAmpString = _lastTargetThicknessString = _lastEventMachineGearString = 
        _lastEventVibrationStateString = _lastPassValidTemperatureString = _csvExportFormatter.NullString;
    }

    public List<string> ProcessSubGrid(ClientCellProfileLeafSubgrid lastPassSubGrid)
    {
      var rows = new List<string>();
      if (RecordCountLimitReached())
        return rows;

      int runningIndexLLHCoords = 0;
      if (_requestArgument.CoordType == CoordType.LatLon)
        _llhCoords = SetupLLPositions(_siteModel.CSIB(), lastPassSubGrid);

      lastPassSubGrid.CalculateWorldOrigin(out double subGridWorldOriginX, out double subGridWorldOriginY);

      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        if (RecordCountLimitReached())
          return;

        var cell = lastPassSubGrid.Cells[x, y];
        if (cell.PassCount == 0) // Nothing for us to do, as cell is empty
          return;

        var easting = subGridWorldOriginX + (x + 0.5) * lastPassSubGrid.CellSize;
        var northing = subGridWorldOriginY + (y + 0.5) * lastPassSubGrid.CellSize;
        rows.Add(FormatADataRow(cell, easting, northing, runningIndexLLHCoords));
        runningIndexLLHCoords++;
        _totalRowCountSoFar++;
      });

      return rows;
    }

    public List<string> ProcessSubGrid(ClientCellProfileAllPassesLeafSubgrid allPassesSubGrid)
    {
      var rows = new List<string>();
      if (RecordCountLimitReached())
        return rows;

      var runningIndexLLHCoords = 0;
      var halfPassCount = 0;
      if (_requestArgument.CoordType == CoordType.LatLon)
        _llhCoords = SetupLLPositions(_siteModel.CSIB(), allPassesSubGrid);

      allPassesSubGrid.CalculateWorldOrigin(out double subGridWorldOriginX, out double subGridWorldOriginY);

      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        if (RecordCountLimitReached())
          return;

        var cell = allPassesSubGrid.Cells[x, y];
        foreach (var cellPass in cell.CellPasses)
        {
          if (RecordCountLimitReached())
            break;

          // we only include the 2nd part of a half pass
          if (cellPass.HalfPass)
          {
            halfPassCount++;
            if (halfPassCount < 2)
              continue;
          }
          halfPassCount = 0;
          var easting = subGridWorldOriginX + (x + 0.5) * allPassesSubGrid.CellSize;
          var northing = subGridWorldOriginY + (y + 0.5) * allPassesSubGrid.CellSize;
          rows.Add(FormatADataRow(cellPass, easting, northing, runningIndexLLHCoords));
          _totalRowCountSoFar++;
        }
        runningIndexLLHCoords++;
      });

      return rows;
    }

    private string FormatADataRow(ClientCellProfileLeafSubgridRecord cell, double easting, double northing, int runningIndexLLHCoords)
    {
      var resultString = new StringBuilder();
      if (!cell.LastPassTime.Equals(_runningLastPassTime))
      {
        _cellPassTimeString = _csvExportFormatter.FormatCellPassTime(cell.LastPassTime);
        _runningLastPassTime = cell.LastPassTime;
      }
      resultString.Append($"{_cellPassTimeString},");

      if (!(_runningNorthing.Equals(northing) && _runningEasting.Equals(easting)))
      {
        _coordString = FormatCoordinate(northing, easting, runningIndexLLHCoords);
        _runningNorthing = northing;
        _runningEasting = easting;
      }
      resultString.Append($"{_coordString},");

      if (!cell.Height.Equals(_runningHeight))
      {
        _heightString = _csvExportFormatter.FormatElevation(cell.Height);
        _runningHeight = cell.Height;
      }
      resultString.Append($"{_heightString},");

      resultString.Append($"{cell.PassCount},");

      var lastPassValidRadioLatencyString = _csvExportFormatter.FormatRadioLatency(cell.LastPassValidRadioLatency);
      resultString.Append($"{lastPassValidRadioLatencyString},");

      if (!cell.EventDesignNameID.Equals(_runningDesignNameId))
      {
        _lastDesignNameString = FormatDesignNameID(cell.EventDesignNameID);
        _runningDesignNameId = cell.EventDesignNameID;
      }
      resultString.Append($"{_lastDesignNameString},");

      if (!cell.InternalSiteModelMachineIndex.Equals(_runningMachineId))
      {
        _lastMachineNameString = FormatMachineName(cell.InternalSiteModelMachineIndex);
        _runningMachineId = cell.InternalSiteModelMachineIndex;
      }
      resultString.Append($"{_lastMachineNameString},");

      if (!cell.MachineSpeed.Equals(_runningMachineSpeed))
      {
        _machineSpeedString = _csvExportFormatter.FormatSpeed(cell.MachineSpeed);
        _runningMachineSpeed = cell.MachineSpeed;
      }
      resultString.Append($"{_machineSpeedString},");

      if (!cell.LastPassValidGPSMode.Equals(_runningGpsMode))
      {
        _gpsModeString = _csvExportFormatter.FormatGPSMode(cell.LastPassValidGPSMode);
        _runningGpsMode = cell.LastPassValidGPSMode;
      }
      resultString.Append($"{_gpsModeString},");

      if (!(cell.GPSAccuracy.Equals(_runningGpsAccuracy) && cell.GPSTolerance.Equals(_runningGpsTolerance)))
      {
        _gpsAccuracyToleranceString = _csvExportFormatter.FormatGPSAccuracy(cell.GPSAccuracy, cell.GPSTolerance);
        _runningGpsAccuracy = cell.GPSAccuracy;
        _runningGpsTolerance = cell.GPSTolerance;
      }
      resultString.Append($"{_gpsAccuracyToleranceString},");

      if (!cell.TargetPassCount.Equals(_runningTargetPassCount))
      {
        _targetPassCountString = _csvExportFormatter.FormatPassCount(cell.TargetPassCount);
        _runningTargetPassCount = cell.TargetPassCount;
      }
      resultString.Append($"{_targetPassCountString},");

      resultString.Append($"{cell.TotalWholePasses},");

      resultString.Append($"{cell.LayersCount},"); // for cellPasses this contains layerID

      if (!cell.LastPassValidCCV.Equals(_runningLastPassValidCcv))
      {
        _lastPassValidCcvString = _csvExportFormatter.FormatCompactionCCVTypes(cell.LastPassValidCCV);
        _runningLastPassValidCcv = cell.LastPassValidCCV;
      }
      resultString.Append($"{_lastPassValidCcvString},");

      if (!cell.TargetCCV.Equals(_runningTargetCcv))
      {
        _lastTargetCcvString = _csvExportFormatter.FormatCompactionCCVTypes(cell.TargetCCV);
        _runningTargetCcv = cell.TargetCCV;
      }
      resultString.Append($"{_lastTargetCcvString},");

      if (!cell.LastPassValidMDP.Equals(_runningLastPassValidMdp))
      {
        _lastPassValidMdpString = _csvExportFormatter.FormatCompactionCCVTypes(cell.LastPassValidMDP);
        _runningLastPassValidMdp = cell.LastPassValidMDP;
      }
      resultString.Append($"{_lastPassValidMdpString},");

      if (!cell.TargetMDP.Equals(_runningTargetMdp))
      {
        _lastTargetMdpString = _csvExportFormatter.FormatCompactionCCVTypes(cell.TargetMDP);
        _runningTargetMdp = cell.TargetMDP;
      }
      resultString.Append($"{_lastTargetMdpString},");

      if (!cell.LastPassValidRMV.Equals(_runningValidRmv))
      {
        _lastValidRmvString = _csvExportFormatter.FormatCompactionCCVTypes(cell.LastPassValidRMV);
        _runningValidRmv = cell.LastPassValidRMV;
      }
      resultString.Append($"{_lastValidRmvString},");

      if (!cell.LastPassValidFreq.Equals(_runningValidFreq))
      {
        _lastValidFreqString = _csvExportFormatter.FormatFrequency(cell.LastPassValidFreq);
        _runningValidFreq = cell.LastPassValidFreq;
      }
      resultString.Append($"{_lastValidFreqString},");

      if (!cell.LastPassValidAmp.Equals(_runningValidAmp))
      {
        _lastValidAmpString = _csvExportFormatter.FormatAmplitude(cell.LastPassValidAmp);
        _runningValidAmp = cell.LastPassValidAmp;
      }
      resultString.Append($"{_lastValidAmpString},");

      if (!cell.TargetThickness.Equals(_runningTargetThickness))
      {
        _lastTargetThicknessString = _csvExportFormatter.FormatTargetThickness(cell.TargetThickness);
        _runningTargetThickness = cell.TargetThickness;
      }
      resultString.Append($"{_lastTargetThicknessString},");

      if (!cell.EventMachineGear.Equals(_runningEventMachineGear))
      {
        _lastEventMachineGearString = _csvExportFormatter.FormatMachineGearValue(cell.EventMachineGear);
        _runningEventMachineGear = cell.EventMachineGear;
      }
      resultString.Append($"{_lastEventMachineGearString},");

      if (!cell.EventVibrationState.Equals(_runningEventVibrationState))
      {
        _lastEventVibrationStateString = _csvExportFormatter.FormatEventVibrationState(cell.EventVibrationState);
        _runningEventVibrationState = cell.EventVibrationState;
      }
      resultString.Append($"{_lastEventVibrationStateString},");

      if (!cell.LastPassValidTemperature.Equals(_runningLastPassValidTemperature))
      {
        _lastPassValidTemperatureString = _csvExportFormatter.FormatLastPassValidTemperature(cell.LastPassValidTemperature);
        _runningLastPassValidTemperature = cell.LastPassValidTemperature;
      }
      resultString.Append($"{_lastPassValidTemperatureString}"); // no training comma

      return resultString.ToString();
    }


    // creates an array of NEE for a sub grid
    //  which is sent to Coordinate service
    //  to resolve into a list of LHH (in same order)
    // Note: only adds entry where cellPassCount exists, so may not be 1024 entries
    private XYZ[] SetupLLPositions(string csibName, IClientLeafSubGrid subGrid)
    {
      var indexIntoNEECoords = 0;
      var NEECoords = new XYZ[1024];
      var subGridProfile = subGrid as ClientCellProfileLeafSubgrid;

      subGrid.CalculateWorldOrigin(out double subGridWorldOriginX, out double subGridWorldOriginY);
      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        var cell = subGridProfile.Cells[x, y];
        if (cell.PassCount == 0) // Nothing for us to do, as cell is empty
          return;
        var easting = subGridWorldOriginX + (x + 0.5) * subGridProfile.CellSize;
        var northing = subGridWorldOriginY + (y + 0.5) * subGridProfile.CellSize;
        NEECoords[indexIntoNEECoords] = new XYZ(easting, northing, cell.Height);
        indexIntoNEECoords++;
      });
      var result = _convertCoordinates.NEEToLLH(csibName, NEECoords);
      if (result.ErrorCode != RequestErrorStatus.OK)
      {
        log.LogError($"#Out# CSVExportExecutor. Unable to convert NEE to LLH : Project: {_siteModel.ID}");
        throw new ServiceException(HttpStatusCode.InternalServerError,
         new ContractExecutionResult((int) RequestErrorStatus.ExportCoordConversionError, "Missing ProjectUID.")); 

      }
      return result.LLHCoordinates;
    }

    private string FormatCoordinate(double northing, double easting, int runningIndexLLHCoords)
    {
      return _requestArgument.CoordType == CoordType.Northeast
       ? $"{_csvExportFormatter.FormatCellPos(northing)},{_csvExportFormatter.FormatCellPos(easting)}"
       : $"{_csvExportFormatter.RadiansToLatLongString(_llhCoords[runningIndexLLHCoords - 1].Y, 8)}{_csvExportFormatter.RadiansToLatLongString(_llhCoords[runningIndexLLHCoords - 1].X, 8)}";
    }

    private string FormatDesignNameID(int designNameId)
    {
      if (designNameId > Consts.kNoDesignNameID)
      {
        var design = _siteModel.SiteModelMachineDesigns.Locate(designNameId);
        return design != null ? $"{design.Name}" : $"{designNameId}";
      }

      return _csvExportFormatter.NullString;
    }

    private string FormatMachineName(int machineId)
    {
      CSVExportMappedMachine machine = null;

      if (machineId > -1 && _requestArgument.MappedMachines != null && _requestArgument.MappedMachines.Count > 0)
      {
         machine = _requestArgument.MappedMachines.FirstOrDefault(m => m.InternalSiteModelMachineIndex == machineId);
      }

      return machine != null ? $"\"{machine.Name}\"" : "\"Unknown\"";
    }
  }
}
