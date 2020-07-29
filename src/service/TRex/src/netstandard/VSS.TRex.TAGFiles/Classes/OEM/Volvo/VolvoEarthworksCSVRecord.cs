using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VSS.TRex.Types;

namespace VSS.TRex.TAGFiles.Classes.OEM.Volvo
{
  public class VolvoEarthworksCSVRecord
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<VolvoEarthworksCSVRecord>();

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

    public VolvoEarthworksCSVRecord()
    {
    }

    public VolvoEarthworksCSVRecord(string line, Dictionary<string, int> headerLocations)
    {
      var parts = line.Split(',');

      Time = DateTime.Parse(GetField("Time", headerLocations, parts));
      CellN_m = double.Parse(GetField("CellN_m", headerLocations, parts));
      CellE_m = double.Parse(GetField("CellE_m", headerLocations, parts));
      PassNumber = int.Parse(GetField("PassNumber", headerLocations, parts));
      DesignName = GetField("DesignName", headerLocations, parts);
      Machine = GetField("Machine", headerLocations, parts);
      Speed_mph = double.Parse(GetField("Speed_mph", headerLocations, parts));
      TargetPassCount = int.Parse(GetField("TargPassCount", headerLocations, parts));
      ValidPos = GetField("ValidPos", headerLocations, parts) == "Yes";
      Lift = int.Parse(GetField("Lift", headerLocations, parts));
      LastEDV = double.Parse(GetField("LastEDV", headerLocations, parts));
      LastFreq_Hz = int.Parse(GetField("LastFreq_Hz", headerLocations, parts));
      LastAmp_mm = double.Parse(GetField("LastAmp_mm", headerLocations, parts));
      TargThickness_FT = double.Parse(GetField("TargThickness_FT", headerLocations, parts));
      MachineGear = GetField("MachineGear", headerLocations, parts);
      VibeState = GetField("VibeState", headerLocations, parts);
      LastTemp_f = double.Parse(GetField("LastTemp_f", headerLocations, parts));
      LastCMV = double.Parse(GetField("LastCMV", headerLocations, parts));
      ICMVType = int.Parse(GetField("ICMVType", headerLocations, parts));
    }

    // Converts the machine type name from the CSV file into a revognised machine type
    public static MachineType MachineTypeFromString(string machineType)
    {
      return machineType switch
      {
        "SD115B" => TRex.Types.MachineType.SoilCompactor,
        _ => TRex.Types.MachineType.Unknown
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
