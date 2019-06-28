using System;
using System.IO;
using Microsoft.Extensions.Logging;
using VSS.Hydrology.WebApi.TTM;

namespace VSS.Hydrology.WebApi.Common.Utilities
{
  public class TTMtoDXFConverter
  {
    private readonly ILogger _log;

    public TTMtoDXFConverter(ILogger log)
    {
      _log = log;
    }

    public TTMtoDXFConverter(ILoggerFactory loggerFactory)
    {
      _log = loggerFactory.CreateLogger<TTMtoDXFConverter>();
    }

    protected internal bool WriteDXFFromTTMStream(string sourceTTMFileName, string targetDXFFileName)
    //protected bool WriteDXFFromTTMStream(Stream stream, string targetDXFFileName)
    {
      var loadedOk = false;
      _log.LogDebug($"{nameof(WriteDXFFromTTMStream)}: sourceFileLength {new FileInfo(sourceTTMFileName).Length} sourceFileName {sourceTTMFileName}");

      try
      {
        var tin = new TrimbleTINModel();
        tin.LoadFromFile(sourceTTMFileName);

        _log.LogDebug($"{nameof(WriteDXFFromTTMStream)}: Starting conversion...");
        DateTime startTime = DateTime.Now;

        using (FileStream stl = new FileStream(targetDXFFileName, FileMode.CreateNew))
        {
          using (StreamWriter writer = new StreamWriter(stl))
          {
            writer.WriteLine($"solid {targetDXFFileName}");

            foreach (var triangle in tin.Triangles)
            {
              writer.WriteLine("facet normal 0 0 0");
              writer.WriteLine("outer loop");
              //foreach (var ww in new[] { triangle.Vertices[0], triangle.Vertices[1], triangle.Vertices[2]})
              //  writer.WriteLine(
              //    $"Vertex {tin.Vertices.Items[vertex].X} {tin.Vertices.Items[vertex].Y} {tin.Vertices.Items[vertex].Z}");
              writer.WriteLine("endloop");
              writer.WriteLine("endfacet");
            }

            writer.WriteLine($"endsolid {targetDXFFileName}");
          }
        }

        loadedOk = true;
        _log.LogDebug($"{nameof(WriteDXFFromTTMStream)}: Conversion complete in {DateTime.Now - startTime}");
        _log.LogDebug($"{nameof(WriteDXFFromTTMStream)}: targetDXFFileName {targetDXFFileName} byteCount: {new FileInfo(targetDXFFileName).Length}");
      }
      catch (Exception e)
      {
        _log.LogDebug(e, $"{nameof(WriteDXFFromTTMStream)}: Exception converting stream");
      }

      return loadedOk;
    }
  }
}
