using System;
using System.Collections.Generic;
using System.Linq;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.CoordinateSystems;
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
    private int maxRowCountPerFile = Int32.MaxValue;
    private Formatter formatter;
    private CSVExportRequestArgument requestArgument;
    private ISiteModel siteModel;

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

    public CSVExportSubGridProcessor(Formatter formatter, 
      CSVExportRequestArgument requestArgument, ISiteModel siteModel)
    {
      this.formatter = formatter;
      this.requestArgument = requestArgument;
      this.siteModel = siteModel;
      cellPassTimeString = coordString = heightString = lastDesignNameString = lastMachineNameString =
        machineSpeedString = gpsAccuracyToleranceString = targetPassCountString = lastPassValidCCVString = 
        lastTargetCCVString = lastPassValidMDPString = lastTargetMDPString = lastValidRMVString = 
        lastValidFreqString = lastValidAmpString = lastTargetThicknessString = lastEventMachineGearString = 
        lastEventVibrationStateString = lastPassValidTemperatureString = formatter.nullString;
    }

    public List<string> ProcessSubGrid(ClientCellProfileLeafSubgrid lastPassSubGrid)
    {
      var rows = new List<string>();
      int runningIndexLLHCoords = 0;
      if (requestArgument.CoordType == CoordType.LatLon)
        LLHCoords = SetupLLPositions(siteModel.CSIB(), lastPassSubGrid);

      lastPassSubGrid.CalculateWorldOrigin(out double subGridWorldOriginX, out double subGridWorldOriginY);

      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        var cell = lastPassSubGrid.Cells[x, y];
        if (cell.PassCount == 0) // Nothing for us to do, as cell is empty
          return;

        rows.Add(FormatADataRow(cell, subGridWorldOriginX, subGridWorldOriginY, runningIndexLLHCoords));
        runningIndexLLHCoords++;

      });

      return rows;
    }

    public List<string> ProcessSubGrid(ClientCellProfileAllPassesLeafSubgrid allPassesSubGrid)
    {
      var rows = new List<string>();
      int runningIndexLLHCoords = 0;
      if (requestArgument.CoordType == CoordType.LatLon)
        LLHCoords = SetupLLPositions(siteModel.CSIB(), allPassesSubGrid);

      allPassesSubGrid.CalculateWorldOrigin(out double subGridWorldOriginX, out double subGridWorldOriginY);

      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        var cell = allPassesSubGrid.Cells[x, y];
        foreach (var cellPass in cell.CellPasses)
        {
          // todoJeannie half passes
          rows.Add(FormatADataRow(cellPass, subGridWorldOriginX, subGridWorldOriginY, runningIndexLLHCoords));
        }
        runningIndexLLHCoords++;
      });

      return rows;
    }

    //public List<string> ProcessSubGrid(IClientLeafSubGrid subGrid) 
    //{
    //  var rows = new List<string>();
    //  int runningIndexLLHCoords = 0;
    //  if (requestArgument.CoordType == CoordType.LatLon)
    //    LLHCoords = SetupLLPositions(siteModel.CSIB(), subGrid); // todoJeannie validate CSIB in executor that CSIB is loaded and avail

    //  subGrid.CalculateWorldOrigin(out double subGridWorldOriginX, out double subGridWorldOriginY);

    //  SubGridUtilities.SubGridDimensionalIterator((x, y) =>
    //  {
    //    if (subGrid is ClientCellProfileLeafSubgrid profileSubGrid)
    //    {
    //      var cell = profileSubGrid.Cells[x, y];
    //      if (cell.PassCount == 0) // Nothing for us to do, as cell is empty
    //        return;

    //      rows.Add(FormatADataRow(cell, subGridWorldOriginX, subGridWorldOriginY, runningIndexLLHCoords));
    //      runningIndexLLHCoords++;
    //    }

    //    if (subGrid is ClientCellProfileAllPassesLeafSubgrid allPassesSubGrid)
    //    {
    //      var cell = allPassesSubGrid.Cells[x, y];
    //      foreach (var cellPass in cell.CellPasses)
    //      {
    //        // todoJeannie half passes
    //        rows.Add(FormatADataRow(cellPass, subGridWorldOriginX, subGridWorldOriginY, runningIndexLLHCoords));
    //      };
    //      runningIndexLLHCoords++;
    //    }

    //  });

    //  return rows;
    //}

    private string FormatADataRow(ClientCellProfileLeafSubgridRecord cell, double subGridWorldOriginX, double subGridWorldOriginY, int runningIndexLLHCoords)
    {
      var resultString = string.Empty;
      if (!cell.LastPassTime.Equals(runningLastPassTime))
      {
        cellPassTimeString = formatter.FormatCellPassTime(cell.LastPassTime);
        runningLastPassTime = cell.LastPassTime;
      }
      resultString += $"{cellPassTimeString},";

      var northing = cell.CellYOffset + subGridWorldOriginY;
      var easting = cell.CellXOffset + subGridWorldOriginX;
      if (!(runningNorthing.Equals(northing) && runningEasting.Equals(easting)))
      {
        coordString = FormatCoordinate(northing, easting, runningIndexLLHCoords);
        runningNorthing = northing;
        runningEasting = easting;
      }
      resultString += $"{coordString},";

      if (!cell.Height.Equals(runningHeight))
      {
        heightString = formatter.FormatElevation(cell.Height);
        runningHeight = cell.Height;
      }
      resultString += $"{heightString},";

      resultString += $"{cell.PassCount.ToString()},";

      var lastPassValidRadioLatencyString = formatter.FormatRadioLatency(cell.LastPassValidRadioLatency);
      resultString += $"{lastPassValidRadioLatencyString},";

      if (!cell.EventDesignNameID.Equals(runningDesignNameID))
      {
        lastDesignNameString = FormatDesignNameID(cell.EventDesignNameID);
        runningDesignNameID = cell.EventDesignNameID;
      }
      resultString += $"{lastDesignNameString},";

      if (!cell.InternalSiteModelMachineIndex.Equals(runningMachineID))
      {
        lastMachineNameString = FormatMachineName(cell.InternalSiteModelMachineIndex);
        runningMachineID = cell.InternalSiteModelMachineIndex;
      }
      resultString += $"{lastMachineNameString},";

      if (!cell.MachineSpeed.Equals(runningMachineSpeed))
      {
        machineSpeedString = formatter.FormatSpeed(cell.MachineSpeed);
        runningMachineSpeed = cell.MachineSpeed;
      }
      resultString += $"{machineSpeedString},";

      if (!cell.LastPassValidGPSMode.Equals(runningGPSMode))
      {
        gpsModeString = formatter.FormatGPSMode(cell.LastPassValidGPSMode);
        runningGPSMode = cell.LastPassValidGPSMode;
      }
      resultString += $"{gpsModeString},";

      if (!(cell.GPSAccuracy.Equals(runningGPSAccuracy) && cell.GPSTolerance.Equals(runningGPSTolerance)))
      {
        gpsAccuracyToleranceString = formatter.FormatGPSAccuracy(cell.GPSAccuracy, cell.GPSTolerance);
        runningGPSAccuracy = cell.GPSAccuracy;
        runningGPSTolerance = cell.GPSTolerance;
      }
      resultString += $"{gpsAccuracyToleranceString},";

      if (!cell.TargetPassCount.Equals(runningTargetPassCount))
      {
        targetPassCountString = formatter.FormatPassCount(cell.TargetPassCount);
        runningTargetPassCount = cell.TargetPassCount;
      }
      resultString += $"{targetPassCountString},";

      resultString += $"{cell.TotalWholePasses},"; 

      resultString += $"{cell.LayersCount},"; // for cellPasses this contains layerID

      if (!cell.LastPassValidCCV.Equals(runningLastPassValidCCV))
      {
        lastPassValidCCVString = formatter.FormatCompactionCCVTypes(cell.LastPassValidCCV);
        runningLastPassValidCCV = cell.LastPassValidCCV;
      }
      resultString += $"{lastPassValidCCVString},";

      if (!cell.TargetCCV.Equals(runningTargetCCV))
      {
        lastTargetCCVString = formatter.FormatCompactionCCVTypes(cell.TargetCCV);
        runningTargetCCV = cell.TargetCCV;
      }
      resultString += $"{lastTargetCCVString},";

      if (!cell.LastPassValidMDP.Equals(runningLastPassValidMDP))
      {
        lastPassValidMDPString = formatter.FormatCompactionCCVTypes(cell.LastPassValidMDP);
        runningLastPassValidMDP = cell.LastPassValidMDP;
      }
      resultString += $"{lastPassValidMDPString},";

      if (!cell.TargetMDP.Equals(runningTargetMDP))
      {
        lastTargetMDPString = formatter.FormatCompactionCCVTypes(cell.TargetMDP);
        runningTargetMDP = cell.TargetMDP;
      }
      resultString += $"{lastTargetMDPString},";

      if (!cell.LastPassValidRMV.Equals(runningValidRMV))
      {
        lastValidRMVString = formatter.FormatCompactionCCVTypes(cell.LastPassValidRMV);
        runningValidRMV = cell.LastPassValidRMV;
      }
      resultString += $"{lastValidRMVString},";

      if (!cell.LastPassValidFreq.Equals(runningValidFreq))
      {
        lastValidFreqString = formatter.FormatFrequency(cell.LastPassValidFreq);
        runningValidFreq = cell.LastPassValidFreq;
      }
      resultString += $"{lastValidFreqString},";

      if (!cell.LastPassValidAmp.Equals(runningValidAmp))
      {
        lastValidAmpString = formatter.FormatAmplitude(cell.LastPassValidAmp);
        runningValidAmp = cell.LastPassValidAmp;
      }
      resultString += $"{lastValidAmpString},";

      if (!cell.TargetThickness.Equals(runningTargetThickness))
      {
        lastTargetThicknessString = formatter.FormatTargetThickness(cell.TargetThickness);
        runningTargetThickness = cell.TargetThickness;
      }
      resultString += $"{lastTargetThicknessString},";

      if (!cell.EventMachineGear.Equals(runningEventMachineGear))
      {
        lastEventMachineGearString = formatter.FormatMachineGearValue(cell.EventMachineGear);
        runningEventMachineGear = cell.EventMachineGear;
      }
      resultString += $"{lastEventMachineGearString},";

      if (!cell.EventVibrationState.Equals(runningEventVibrationState))
      {
        lastEventVibrationStateString = formatter.FormatEventVibrationState(cell.EventVibrationState);
        runningEventVibrationState = cell.EventVibrationState;
      }
      resultString += $"{lastEventVibrationStateString},";

      if (!cell.LastPassValidTemperature.Equals(runningLastPassValidTemperature))
      {
        lastPassValidTemperatureString = formatter.FormatLastPassValidTemperature(cell.LastPassValidTemperature);
        runningLastPassValidTemperature = cell.LastPassValidTemperature;
      }
      resultString += $"{lastPassValidTemperatureString}"; // no training comma

      return resultString;
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
        // todoJeannie throw exception
      }
      return result.LLHCoordinates;
    }
    private string FormatCoordinate(double northing, double easting, int runningIndexLLHCoords)
    {
      if (requestArgument.CoordType == CoordType.Northeast)
        return string.Format($"{formatter.FormatCellPos(northing)},{formatter.FormatCellPos(easting)}");

      return string.Format($"{formatter.RadiansToLatLongString(LLHCoords[runningIndexLLHCoords - 1].Y, 8)}{formatter.RadiansToLatLongString(LLHCoords[runningIndexLLHCoords - 1].X, 8)}");
    }

    private string FormatDesignNameID(int designNameId)
    {
      if (designNameId > Consts.kNoDesignNameID)
      {
        var design = siteModel.SiteModelMachineDesigns.Locate(designNameId);
        if (design != null)
          return string.Format($"{design.Name}");
        return string.Format($"{designNameId}");
      }

      return  formatter.nullString;
    }

    private string FormatMachineName(int machineId)
    {
      if (machineId > -1 && requestArgument.MappedMachines != null && requestArgument.MappedMachines.Count > 0)
      {
        var machine = requestArgument.MappedMachines.FirstOrDefault(m => m.InternalSiteModelMachineIndex == machineId);
        if (machine != null)
          return string.Format($"\"{machine.Name}\"");
      }

      return string.Format($"\"Unknown\"");
    }
  }
}
