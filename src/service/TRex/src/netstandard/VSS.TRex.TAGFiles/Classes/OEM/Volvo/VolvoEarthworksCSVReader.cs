using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using VSS.TRex.TAGFiles.Classes.Processors;
using VSS.TRex.TAGFiles.Classes.Sinks;
using VSS.TRex.TAGFiles.Models;
using VSS.TRex.Types;
using VSS.TRex.Types.CellPasses;

namespace VSS.TRex.TAGFiles.Classes.OEM.Volvo
{
  public class VolvoEarthworksCSVReader
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<VolvoEarthworksCSVReader>();

    private const double VOLVO_EARTHWORKS_GRID_CELL_SIZE = 0.30;

    private Stream _stream;

    private Dictionary<string, int> _headerLocations;

    /// <summary>
    /// Volvo earthworks file reader constructor. Accepts a stream to read data from.
    /// </summary>
    public VolvoEarthworksCSVReader(Stream stream)
    {
      _stream = stream;
    }

    private IEnumerable<string> ReadLines(StreamReader reader)
    {
      string line;
      while ((line = reader.ReadLine()) != null)
      {
        yield return line;
      }
    }

    private VolvoEarthworksCSVRecord ParseLine(string line)
    {
      return new VolvoEarthworksCSVRecord(line, _headerLocations);
    }

    /// <summary>
    /// Reads the context of a Volvo earthworks CSV file using the provided sink to send data to
    /// Note: LastEDV value is not handled as it has no corresponding value in TRex
    /// </summary>
    public TAGReadResult Read(TAGValueSinkBase sink, TAGProcessor processor)
    {
      using var streamReader = new StreamReader(_stream, Encoding.ASCII);

      var headerLine = streamReader.ReadLine();
      var headerIndex = 0;
      _headerLocations = headerLine.Split(',').Select(x => new KeyValuePair<string, int>(x, headerIndex++)).ToDictionary(k => k.Key, v => v.Value);

      // Read all remaining lines into an array for easy access
      var lines = ReadLines(streamReader).ToList();

      var swather = new VolvoEarthworksCSVGridSwather(processor,
                                                      processor.MachineTargetValueChangesAggregator,
                                                      processor.SiteModel,
                                                      processor.SiteModelGridAggregator,
                                                      null)
      {
        ProcessedEpochNumber = processor.ProcessedEpochCount
      };

      var currentTime = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
      //var firstDataTime = 
      // For each line, locate the TRex cell that fall within the cell and add a cell pass for it
      lines.ForEach(line =>
      {
        var cellPass = ParseLine(line);

        if (!cellPass.lineParsedOK)
        {
          // Each cell is parsed independently, so we can short circuit processing if a line did not parse
          return;
        }

        processor.DataTime = DateTime.SpecifyKind(cellPass.Time, DateTimeKind.Utc);

        // There is no RMV in the CSV file - set on ground to be true all the time.
        processor.SetOnGround(OnGroundState.YesLegacy);

        // Default Volvo machine to MC024 sensor returning CMV values
        // TODO: understand if this is a special sensor for Volvo
        // TODO: understand meaning of the ICMVType field
        processor.ICSensorType = CompactionSensorType.MC024;

        // TODO: Understand the relationship of Volvo CMV value to internal CMV value 
        processor.SetICCCVValue((short)Math.Round(cellPass.LastCMV));

        processor.MachineType = VolvoEarthworksCSVRecord.MachineTypeFromString(cellPass.Machine);

        if (processor.MachineType == MachineType.Unknown)
        {
          _log.LogDebug($"Machine type name {cellPass.Machine} generated an unknown machine type");
        }

        // Add the events for this line
        if (currentTime < processor.DataTime)
        {
          // Fill in the machine events for this epoch
          processor.Design = cellPass.DesignName;
        }

        // Convert mph to cs/s. 44.704 is the magic mph to cm/s conversion factor
        var speedInCentimetersPerSecond = (int)Math.Round(cellPass.Speed_mph * 44.704);
        processor.SetICMachineSpeedValue(speedInCentimetersPerSecond);

        processor.ICPassTargetValue = (ushort)cellPass.TargetPassCount;
        processor.ValidPosition = cellPass.ValidPos ? (byte)1 : (byte)0;

        processor.ICLayerIDValue = (ushort)cellPass.Lift;

        processor.SetICFrequency((ushort)cellPass.LastFreq_Hz);
        processor.SetICAmplitude(cellPass.LastAmp_mm == -1000 ? CellPassConsts.NullAmplitude : (ushort)Math.Round(cellPass.LastAmp_mm * 100));

        processor.ICTargetLiftThickness = (float)(cellPass.TargThickness_FT / 3.048);
        processor.ICGear = VolvoEarthworksCSVRecord.MachineGearFromString(cellPass.MachineGear);

        if (processor.ICGear == MachineGear.Null)
        {
          _log.LogDebug($"Machine gear name {cellPass.MachineGear} generated a null machine gear");
        }

        processor.SetICTemperatureValue((ushort)cellPass.LastTemp_f);
        processor.ICMode = (byte)((cellPass.VibeState == "On" ? 1 : 0) << ICModeFlags.IC_TEMPERATURE_VIBRATION_STATE_SHIFT);

        // Add the cell pass for this line.
        // Note: Half pass is hardwired to false, and pass type is hardwired to front drum/blade
        swather.SwathSingleCell(false, PassType.Front, cellPass.CellE_m, cellPass.CellN_m, VOLVO_EARTHWORKS_GRID_CELL_SIZE, cellPass);

        processor.ProcessedEpochCount = swather.ProcessedEpochNumber;
      });

      return TAGReadResult.NoError;
    }
  }
}
