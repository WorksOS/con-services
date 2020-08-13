using System;
using System.Collections.Generic;
using Jaeger.Thrift;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.Types;
using VSS.TRex.Types.CellPasses;

namespace VSS.TRex.TAGFiles.Classes.OEM.Volvo
{
  public class VolvoEarthworksCSVRecord
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<VolvoEarthworksCSVRecord>();

    public bool lineParsedOK = false;

    public DateTime Time;
    public double CellN_m;
    public double CellE_m;
    public int PassNumber;
    public string DesignName;
    public string Machine; // ??????
    public double Speed_mph;
    public int TargetPassCount;
    public bool ValidPos;
    public int Lift;
    public double LastEDV;
    public int LastFreq_Hz;
    public double LastAmp_mm;
    public double TargThickness_FT;
    public string MachineGear;
    public string VibeState;
    public double LastTemp_f;
    public double LastCMV;
    public int ICMVType;

    private string GetField(string fieldName, Dictionary<string, int> headerLocations, string[] parts)
    {
      var partsIndex = headerLocations[fieldName];
      var hasValue = partsIndex >= 0 && partsIndex < parts.Length;

      if (!hasValue)
        _log.LogDebug($"Field {fieldName} not found in header dictionary");

      return hasValue ? parts[partsIndex] : string.Empty;
    }

    private double GetDouble(string fieldName, Dictionary<string, int> headerLocations, string[] parts, double defaultValue)
    {
      var stringValue = GetField(fieldName, headerLocations, parts);
      return string.IsNullOrEmpty(stringValue) ? defaultValue : double.Parse(stringValue);
    }

    public VolvoEarthworksCSVRecord()
    {
    }

    public VolvoEarthworksCSVRecord(string line, Dictionary<string, int> headerLocations)
    {
      try
      {
        var parts = line.Split(',');

        Time = DateTime.Parse(GetField("Time", headerLocations, parts));
        CellN_m = GetDouble("CellN_m", headerLocations, parts, Consts.NullDouble);
        CellE_m = GetDouble("CellE_m", headerLocations, parts, Consts.NullDouble);
        PassNumber = int.Parse(GetField("PassNumber", headerLocations, parts));
        DesignName = GetField("DesignName", headerLocations, parts);
        Machine = GetField("Machine", headerLocations, parts);
        Speed_mph = GetDouble("Speed_mph", headerLocations, parts, CellPassConsts.NullMachineSpeed);
        TargetPassCount = int.Parse(GetField("TargPassCount", headerLocations, parts));
        ValidPos = GetField("ValidPos", headerLocations, parts) == "Yes";
        Lift = int.Parse(GetField("Lift", headerLocations, parts));
        LastEDV = GetDouble("LastEDV", headerLocations, parts, 0.0);
        LastFreq_Hz = int.Parse(GetField("LastFreq_Hz", headerLocations, parts));
        LastAmp_mm = GetDouble("LastAmp_mm", headerLocations, parts, CellPassConsts.NullAmplitude);
        TargThickness_FT = GetDouble("TargThickness_FT", headerLocations, parts, Consts.NullDouble);
        MachineGear = GetField("MachineGear", headerLocations, parts);
        VibeState = GetField("VibeState", headerLocations, parts);
        LastTemp_f = GetDouble("LastTemp_f", headerLocations, parts, CellPassConsts.NullMaterialTemperatureValue);
        LastCMV = GetDouble("LastCMV", headerLocations, parts, CellPassConsts.NullCCV);
        ICMVType = int.Parse(GetField("ICMVType", headerLocations, parts));

        lineParsedOK = CellN_m != Consts.NullDouble && CellE_m != Consts.NullDouble;
      }
      catch (Exception e)
      {
        _log.LogError(e, $"Failed to parse line {line}");
        lineParsedOK = false;
      }
    }

    // Converts the machine type name from the CSV file into a revognised machine type
    public static MachineType MachineTypeFromString(string machineType)
    {
      return machineType switch
      {
        "SD115B" => MachineType.SoilCompactor,
        _ => MachineType.Unknown
      };
    }

    public static MachineGear MachineGearFromString(string machinegear)
    {
      return machinegear switch
      {
        "Forward" => TRex.Types.MachineGear.Forward,
        "Reverse" => TRex.Types.MachineGear.Reverse,
        _ => TRex.Types.MachineGear.Null
      };
    }
  }
}
