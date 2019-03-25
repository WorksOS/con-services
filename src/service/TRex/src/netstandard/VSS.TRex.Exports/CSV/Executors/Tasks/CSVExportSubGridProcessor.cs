using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
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

    private readonly int maxExportRows;
    private int totalRowCountSoFar;

    public bool RecordCountLimitReached() { return totalRowCountSoFar >= maxExportRows;}

    private readonly CSVExportRequestArgument requestArgument;
    private readonly CSVExportFormatter csvExportFormatter;
    private readonly ISiteModel siteModel;

    private XYZ[] LLHCoords = null;
    private DateTime runningLastPassTime = DateTime.MinValue;
    private string cellPassTimeString;
    private double runningNorthing = Consts.NullDouble;
    private double runningEasting = Consts.NullDouble;
    private string coordString;
    private float runningHeight = Consts.NullHeight;
    private string heightString;
    private int runningDesignNameID = Consts.kNoDesignNameID;
    private string lastDesignNameString;
    private short runningMachineID = -1;
    private string lastMachineNameString;
    private ushort runningMachineSpeed = Consts.NullMachineSpeed;
    private string machineSpeedString;
    private GPSMode runningGPSMode = GPSMode.NoGPS;
    private string gpsModeString = "Not_Applicable";
    private Types.GPSAccuracy runningGPSAccuracy = Types.GPSAccuracy.Unknown;
    private ushort runningGPSTolerance = CellPassConsts.NullGPSTolerance;
    private string gpsAccuracyToleranceString;
    private int runningTargetPassCount = CellPassConsts.NullPassCountValue;
    private string targetPassCountString;
    private short runningLastPassValidCCV = CellPassConsts.NullCCV;
    private string lastPassValidCCVString;
    private short runningTargetCCV = CellPassConsts.NullCCV;
    private string lastTargetCCVString;
    private short runningLastPassValidMDP = CellPassConsts.NullCCV;
    private string lastPassValidMDPString;
    private short runningTargetMDP = CellPassConsts.NullCCV;
    private string lastTargetMDPString;
    private short runningValidRMV = CellPassConsts.NullCCV;
    private string lastValidRMVString;
    private ushort runningValidFreq = CellPassConsts.NullFrequency;
    private string lastValidFreqString;
    private ushort runningValidAmp = CellPassConsts.NullAmplitude;
    private string lastValidAmpString;
    private float runningTargetThickness = CellPassConsts.NullOverridingTargetLiftThicknessValue;
    private string lastTargetThicknessString;
    private MachineGear runningEventMachineGear = MachineGear.Null;
    private string lastEventMachineGearString;
    private VibrationState runningEventVibrationState = VibrationState.Invalid;
    private string lastEventVibrationStateString;
    private ushort runningLastPassValidTemperature = CellPassConsts.NullMaterialTemperatureValue;
    private string lastPassValidTemperatureString;

    public CSVExportSubGridProcessor(CSVExportRequestArgument requestArgument)
    {
      maxExportRows = DIContext.Obtain<IConfigurationStore>().GetValueInt("MAX_EXPORT_ROWS", Consts.DEFAULT_MAX_EXPORT_ROWS);
      this.requestArgument = requestArgument;
      csvExportFormatter = new CSVExportFormatter(requestArgument.UserPreferences, requestArgument.OutputType, requestArgument.RawDataAsDBase);
      siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(requestArgument.ProjectID);
      cellPassTimeString = coordString = heightString = lastDesignNameString = lastMachineNameString =
        machineSpeedString = gpsAccuracyToleranceString = targetPassCountString = lastPassValidCCVString = 
        lastTargetCCVString = lastPassValidMDPString = lastTargetMDPString = lastValidRMVString = 
        lastValidFreqString = lastValidAmpString = lastTargetThicknessString = lastEventMachineGearString = 
        lastEventVibrationStateString = lastPassValidTemperatureString = csvExportFormatter.nullString;
    }

    public List<string> ProcessSubGrid(ClientCellProfileLeafSubgrid lastPassSubGrid)
    {
      var rows = new List<string>();
      if (RecordCountLimitReached())
        return rows;

      int runningIndexLLHCoords = 0;
      if (requestArgument.CoordType == CoordType.LatLon)
        LLHCoords = SetupLLPositions(siteModel.CSIB(), lastPassSubGrid);

      lastPassSubGrid.CalculateWorldOrigin(out double subGridWorldOriginX, out double subGridWorldOriginY);

      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        if (RecordCountLimitReached())
          return;

        var cell = lastPassSubGrid.Cells[x, y];
        if (cell.PassCount == 0) // Nothing for us to do, as cell is empty
          return;

        rows.Add(FormatADataRow(cell, subGridWorldOriginX, subGridWorldOriginY, runningIndexLLHCoords));
        runningIndexLLHCoords++;
        totalRowCountSoFar++;
      });

      return rows;
    }

    public List<string> ProcessSubGrid(ClientCellProfileAllPassesLeafSubgrid allPassesSubGrid)
    {
      var rows = new List<string>();
      if (RecordCountLimitReached())
        return rows;

      int runningIndexLLHCoords = 0;
      int halfPassCount = 0;
      if (requestArgument.CoordType == CoordType.LatLon)
        LLHCoords = SetupLLPositions(siteModel.CSIB(), allPassesSubGrid);

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
            halfPassCount = 0;
          }

          rows.Add(FormatADataRow(cellPass, subGridWorldOriginX, subGridWorldOriginY, runningIndexLLHCoords));
          totalRowCountSoFar++;
        }
        runningIndexLLHCoords++;
      });

      return rows;
    }

    private string FormatADataRow(ClientCellProfileLeafSubgridRecord cell, double subGridWorldOriginX, double subGridWorldOriginY, int runningIndexLLHCoords)
    {
      var resultString = new StringBuilder();
      if (!cell.LastPassTime.Equals(runningLastPassTime))
      {
        cellPassTimeString = csvExportFormatter.FormatCellPassTime(cell.LastPassTime);
        runningLastPassTime = cell.LastPassTime;
      }
      resultString.Append($"{cellPassTimeString},");

      var northing = cell.CellYOffset + subGridWorldOriginY;
      var easting = cell.CellXOffset + subGridWorldOriginX;
      if (!(runningNorthing.Equals(northing) && runningEasting.Equals(easting)))
      {
        coordString = FormatCoordinate(northing, easting, runningIndexLLHCoords);
        runningNorthing = northing;
        runningEasting = easting;
      }
      resultString.Append($"{coordString},");

      if (!cell.Height.Equals(runningHeight))
      {
        heightString = csvExportFormatter.FormatElevation(cell.Height);
        runningHeight = cell.Height;
      }
      resultString.Append($"{heightString},");

      resultString.Append($"{cell.PassCount},");

      var lastPassValidRadioLatencyString = csvExportFormatter.FormatRadioLatency(cell.LastPassValidRadioLatency);
      resultString.Append($"{lastPassValidRadioLatencyString},");

      if (!cell.EventDesignNameID.Equals(runningDesignNameID))
      {
        lastDesignNameString = FormatDesignNameID(cell.EventDesignNameID);
        runningDesignNameID = cell.EventDesignNameID;
      }
      resultString.Append($"{lastDesignNameString},");

      if (!cell.InternalSiteModelMachineIndex.Equals(runningMachineID))
      {
        lastMachineNameString = FormatMachineName(cell.InternalSiteModelMachineIndex);
        runningMachineID = cell.InternalSiteModelMachineIndex;
      }
      resultString.Append($"{lastMachineNameString},");

      if (!cell.MachineSpeed.Equals(runningMachineSpeed))
      {
        machineSpeedString = csvExportFormatter.FormatSpeed(cell.MachineSpeed);
        runningMachineSpeed = cell.MachineSpeed;
      }
      resultString.Append($"{machineSpeedString},");

      if (!cell.LastPassValidGPSMode.Equals(runningGPSMode))
      {
        gpsModeString = csvExportFormatter.FormatGPSMode(cell.LastPassValidGPSMode);
        runningGPSMode = cell.LastPassValidGPSMode;
      }
      resultString.Append($"{gpsModeString},");

      if (!(cell.GPSAccuracy.Equals(runningGPSAccuracy) && cell.GPSTolerance.Equals(runningGPSTolerance)))
      {
        gpsAccuracyToleranceString = csvExportFormatter.FormatGPSAccuracy(cell.GPSAccuracy, cell.GPSTolerance);
        runningGPSAccuracy = cell.GPSAccuracy;
        runningGPSTolerance = cell.GPSTolerance;
      }
      resultString.Append($"{gpsAccuracyToleranceString},");

      if (!cell.TargetPassCount.Equals(runningTargetPassCount))
      {
        targetPassCountString = csvExportFormatter.FormatPassCount(cell.TargetPassCount);
        runningTargetPassCount = cell.TargetPassCount;
      }
      resultString.Append($"{targetPassCountString},");

      resultString.Append($"{cell.TotalWholePasses},");

      resultString.Append($"{cell.LayersCount},"); // for cellPasses this contains layerID

      if (!cell.LastPassValidCCV.Equals(runningLastPassValidCCV))
      {
        lastPassValidCCVString = csvExportFormatter.FormatCompactionCCVTypes(cell.LastPassValidCCV);
        runningLastPassValidCCV = cell.LastPassValidCCV;
      }
      resultString.Append($"{lastPassValidCCVString},");

      if (!cell.TargetCCV.Equals(runningTargetCCV))
      {
        lastTargetCCVString = csvExportFormatter.FormatCompactionCCVTypes(cell.TargetCCV);
        runningTargetCCV = cell.TargetCCV;
      }
      resultString.Append($"{lastTargetCCVString},");

      if (!cell.LastPassValidMDP.Equals(runningLastPassValidMDP))
      {
        lastPassValidMDPString = csvExportFormatter.FormatCompactionCCVTypes(cell.LastPassValidMDP);
        runningLastPassValidMDP = cell.LastPassValidMDP;
      }
      resultString.Append($"{lastPassValidMDPString},");

      if (!cell.TargetMDP.Equals(runningTargetMDP))
      {
        lastTargetMDPString = csvExportFormatter.FormatCompactionCCVTypes(cell.TargetMDP);
        runningTargetMDP = cell.TargetMDP;
      }
      resultString.Append($"{lastTargetMDPString},");

      if (!cell.LastPassValidRMV.Equals(runningValidRMV))
      {
        lastValidRMVString = csvExportFormatter.FormatCompactionCCVTypes(cell.LastPassValidRMV);
        runningValidRMV = cell.LastPassValidRMV;
      }
      resultString.Append($"{lastValidRMVString},");

      if (!cell.LastPassValidFreq.Equals(runningValidFreq))
      {
        lastValidFreqString = csvExportFormatter.FormatFrequency(cell.LastPassValidFreq);
        runningValidFreq = cell.LastPassValidFreq;
      }
      resultString.Append($"{lastValidFreqString},");

      if (!cell.LastPassValidAmp.Equals(runningValidAmp))
      {
        lastValidAmpString = csvExportFormatter.FormatAmplitude(cell.LastPassValidAmp);
        runningValidAmp = cell.LastPassValidAmp;
      }
      resultString.Append($"{lastValidAmpString},");

      if (!cell.TargetThickness.Equals(runningTargetThickness))
      {
        lastTargetThicknessString = csvExportFormatter.FormatTargetThickness(cell.TargetThickness);
        runningTargetThickness = cell.TargetThickness;
      }
      resultString.Append($"{lastTargetThicknessString},");

      if (!cell.EventMachineGear.Equals(runningEventMachineGear))
      {
        lastEventMachineGearString = csvExportFormatter.FormatMachineGearValue(cell.EventMachineGear);
        runningEventMachineGear = cell.EventMachineGear;
      }
      resultString.Append($"{lastEventMachineGearString},");

      if (!cell.EventVibrationState.Equals(runningEventVibrationState))
      {
        lastEventVibrationStateString = csvExportFormatter.FormatEventVibrationState(cell.EventVibrationState);
        runningEventVibrationState = cell.EventVibrationState;
      }
      resultString.Append($"{lastEventVibrationStateString},");

      if (!cell.LastPassValidTemperature.Equals(runningLastPassValidTemperature))
      {
        lastPassValidTemperatureString = csvExportFormatter.FormatLastPassValidTemperature(cell.LastPassValidTemperature);
        runningLastPassValidTemperature = cell.LastPassValidTemperature;
      }
      resultString.Append($"{lastPassValidTemperatureString}"); // no training comma

      return resultString.ToString();
    }


    // creates an array of NEE for a sub grid
    //  which is sent to Coordinate service
    //  to resolve into a list of LHH (in same order)
    // Note: only adds entry where cellPassCount exists, so may not be 1024 entries
    private XYZ[] SetupLLPositions(string csibName, IClientLeafSubGrid subGrid)
    {
      int indexIntoNEECoords = 0;
      var NEECoords = new XYZ[1024];
      var subGridp = subGrid as ClientCellProfileLeafSubgrid;

      subGrid.CalculateWorldOrigin(out double subGridWorldOriginX, out double subGridWorldOriginY);
      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        var cell = subGridp.Cells[x, y];
        if (cell.PassCount == 0) // Nothing for us to do, as cell is empty
          return;
        var northing = cell.CellYOffset + subGridWorldOriginY;
        var easting = cell.CellXOffset + subGridWorldOriginX;
        NEECoords[indexIntoNEECoords] = new XYZ(easting, northing, cell.Height);
        indexIntoNEECoords++;
      });
      var result = ConvertCoordinates.NEEToLLH(csibName, NEECoords);
      if (result.ErrorCode != RequestErrorStatus.OK)
      {
        //Log.LogInformation("Summary volume failure, could not convert bounding area from grid to WGS coordinates");
        //response.ResponseCode = SubGridRequestsResponseResult.Failure;

        log.LogError($"#Out# CSVExportExecutor. Unable to convert NEE to LLH : Project: {siteModel.ID}");
        throw new ServiceException(HttpStatusCode.InternalServerError,
         new ContractExecutionResult((int) RequestErrorStatus.ExportCoordConversionError, "Missing ProjectUID.")); 

      }
      return result.LLHCoordinates;
    }

    private string FormatCoordinate(double northing, double easting, int runningIndexLLHCoords)
    {
      return requestArgument.CoordType == CoordType.Northeast
       ? $"{csvExportFormatter.FormatCellPos(northing)},{csvExportFormatter.FormatCellPos(easting)}"
       : $"{csvExportFormatter.RadiansToLatLongString(LLHCoords[runningIndexLLHCoords - 1].Y, 8)}{csvExportFormatter.RadiansToLatLongString(LLHCoords[runningIndexLLHCoords - 1].X, 8)}";
    }

    private string FormatDesignNameID(int designNameId)
    {
      if (designNameId > Consts.kNoDesignNameID)
      {
        var design = siteModel.SiteModelMachineDesigns.Locate(designNameId);
        return design != null ? $"{design.Name}" : $"{designNameId}";
      }

      return csvExportFormatter.nullString;
    }

    private string FormatMachineName(int machineId)
    {
      CSVExportMappedMachine machine = null;

      if (machineId > -1 && requestArgument.MappedMachines != null && requestArgument.MappedMachines.Count > 0)
      {
         machine = requestArgument.MappedMachines.FirstOrDefault(m => m.InternalSiteModelMachineIndex == machineId);
      }

      return machine != null ? "\"{machine.Name}\"" : "\"Unknown\"";
    }
  }
}
