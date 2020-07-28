using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Amazon.S3.Model.Internal.MarshallTransformations;
using VSS.TRex.TAGFiles.Classes.Sinks;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.TAGFiles.Classes.OEM.Volvo
{
  public class VolvoEarthworksCSVReader
  {
    private Stream stream;

    /// <summary>
    /// Volvo earthworks file reader constructor. Accepts a stream to read data from.
    /// </summary>
    public VolvoEarthworksCSVReader(Stream stream)
    {
      this.stream = stream;
    }

    private IEnumerable<string> ReadLines(Stream stream,
                                     Encoding encoding)
    {
      using (var reader = new StreamReader(stream, encoding))
      {
        string line;
        while ((line = reader.ReadLine()) != null)
        {
          yield return line;
        }
      }
    }

    /// <summary>
    /// Reads the context of a Volve earthworks CSV file using the provided sink to send data to
    /// </summary>
    public TAGReadResult Read(TAGValueSinkBase sink)
    {
      // Read all lines into an arrya for easy access
      var lines = ReadLines(stream, Encoding.ASCII).ToList();

      // Read groups of cells in the CSV identified by the same time stamp and construct an approximate blade epoch for uit, then inject the 
      // required values into the sink for processing

      var indexOfGroup = 0;

//      var 
      var thisDateTime = lines[indexOfGroup];

      return TAGReadResult.NoError;
    }
  }
}
