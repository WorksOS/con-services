using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using VSS.TRex.TAGFiles.Classes.Processors;
using VSS.TRex.TAGFiles.Classes.Sinks;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.TAGFiles.Classes.OEM.Volvo
{
  public class VolvoEarthworksCSVReader
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<VolvoEarthworksCSVReader>();

    private const double VOLVO_EARTHWORKS_GRID_CELL_SIZE = 0.30;

    private Stream stream;

    private Dictionary<string, int> HeaderLocations;

    /// <summary>
    /// Volvo earthworks file reader constructor. Accepts a stream to read data from.
    /// </summary>
    public VolvoEarthworksCSVReader(Stream stream)
    {
      this.stream = stream;
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
      return new VolvoEarthworksCSVRecord(line, HeaderLocations);
    }

    /// <summary>
    /// Reads the context of a Volvo earthworks CSV file using the provided sink to send data to
    /// </summary>
    public TAGReadResult Read(TAGValueSinkBase sink, TAGProcessor Processor)
    {
      using var streamReader = new StreamReader(stream, Encoding.ASCII);

      var headerLine = streamReader.ReadLine();
      var headerIndex = 0;
      HeaderLocations = headerLine.Split(',').Select(x => new KeyValuePair<string, int>(x, headerIndex++)).ToDictionary(k => k.Key, v => v.Value);

      // Read all remaining lines into an array for easy access
      var lines = ReadLines(streamReader).ToList();

      var swather = new VolvoEarthworksCSVGridSwather(Processor,
                                                      Processor.MachineTargetValueChangesAggregator,
                                                      Processor.SiteModel,
                                                      Processor.SiteModelGridAggregator,
                                                      null)
      {
        ProcessedEpochNumber = lines.Count
      };

      // For each line, locate the TRex cell that fall within the cell and add a cell pass for it
      lines.ForEach(line =>
      {
        var cellPass = ParseLine(line);

        swather.SwathSingleCell(cellPass.CellE_m, cellPass.CellN_m, VOLVO_EARTHWORKS_GRID_CELL_SIZE, cellPass);
      });

      return TAGReadResult.NoError;
    }
  }
}
