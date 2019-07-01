#if NET_4_7 
using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Hydrology.WebApi.DXF;
using VSS.Hydrology.WebApi.DXF.Entities;
using VSS.Hydrology.WebApi.DXF.Header;
using VSS.Hydrology.WebApi.TTM;

namespace VSS.Hydrology.WebApi.Common.Utilities
{
  public class TTMtoDXFConverter
  {
    private readonly ILogger _log;
    private int ttmTriangleCount = 0;
    public int TTMTriangleCount() => ttmTriangleCount;
    private int dxfTriangleCount = 0;
    public int DXFTriangleCount() => dxfTriangleCount;


    public TTMtoDXFConverter(ILogger log)
    {
      _log = log;
    }

    public TTMtoDXFConverter(ILoggerFactory loggerFactory)
    {
      _log = loggerFactory.CreateLogger<TTMtoDXFConverter>();
    }

    public bool WriteDXFFromTTMStream(MemoryStream sourceTTMFileStream, string targetDXFFileName)
    {
      var loadedOk = false;
      _log.LogDebug($"{nameof(WriteDXFFromTTMStream)}: targetDXFFileName {targetDXFFileName}");

      try
      {
        var tin = new TrimbleTINModel();
        tin.LoadFromStream(sourceTTMFileStream);
        ttmTriangleCount = tin.Triangles.Count;
        _log.LogDebug($"{nameof(WriteDXFFromTTMStream)}: TTM triangleCount {tin.Triangles.Count}");

        var dxf = new DxfDocument();

        foreach (var triangle in tin.Triangles)
        {
          var targetTriangle = new Face3d(
            new Vector3(triangle.Vertices[0].X, triangle.Vertices[0].Y, triangle.Vertices[0].Z),
            new Vector3(triangle.Vertices[1].X, triangle.Vertices[1].Y, triangle.Vertices[1].Z),
            new Vector3(triangle.Vertices[2].X, triangle.Vertices[2].Y, triangle.Vertices[2].Z));
          dxf.AddEntity(targetTriangle);
        }

        _log.LogDebug($"{nameof(WriteDXFFromTTMStream)}: DXF triangleCount {dxf.Faces3d.Count()}");
        dxf.Save(targetDXFFileName);
        dxfTriangleCount = dxf.Faces3d.Count();
        loadedOk = true;
      }
      catch (Exception e)
      {
        _log.LogDebug(e, $"{nameof(WriteDXFFromTTMStream)}: Exception converting TTM to DXF file");
      }

      return loadedOk;
    }

    public bool WriteDXFFromTTMFile(string sourceTTMFileName, string targetDXFFileName)
    {
      var loadedOk = false;
      _log.LogDebug(
        $"{nameof(WriteDXFFromTTMStream)}: sourceFileLength {new FileInfo(sourceTTMFileName).Length} sourceFileName {sourceTTMFileName}");

      try
      {
        var tin = new TrimbleTINModel();
        tin.LoadFromFile(sourceTTMFileName);
        _log.LogDebug($"{nameof(WriteDXFFromTTMStream)}: triangleCount in source {tin.Triangles.Count}");

        var dxf = new DxfDocument();

        foreach (var triangle in tin.Triangles)
        {
          var targetTriangle = new Face3d(
            new Vector3(triangle.Vertices[0].X, triangle.Vertices[0].Y, triangle.Vertices[0].Z),
            new Vector3(triangle.Vertices[1].X, triangle.Vertices[1].Y, triangle.Vertices[1].Z),
            new Vector3(triangle.Vertices[2].X, triangle.Vertices[2].Y, triangle.Vertices[2].Z));
          dxf.AddEntity(targetTriangle);
          _log.LogDebug(
            $"{nameof(WriteDXFFromTTMStream)} source triangle {JsonConvert.SerializeObject(triangle)}, target triangle {JsonConvert.SerializeObject(targetTriangle)}");
        }

        _log.LogDebug($"{nameof(WriteDXFFromTTMStream)}: triangeCount in target {dxf.Faces3d.Count()}");
        dxf.Save(targetDXFFileName);
        loadedOk = true;
      }
      catch (Exception e)
      {
        _log.LogDebug(e, $"{nameof(WriteDXFFromTTMStream)}: Exception converting stream");
        throw e;
      }


      // this check is optional but recommended before loading a DXF file
      var dxfVersion = DxfDocument.CheckDxfFileVersion(targetDXFFileName);
      // netDxf is only compatible with AutoCad2000 and higher DXF version
      if (dxfVersion < DxfVersion.AutoCad2000)
        throw new InvalidOperationException(
          $"{nameof(WriteDXFFromTTMStream)} incorrect DXF file version: {dxfVersion}");
      // load file
      var loaded = DxfDocument.Load(targetDXFFileName);
      _log.LogDebug($"{nameof(WriteDXFFromTTMStream)}: triangleCount in re-loaded target {loaded.Faces3d.Count()}");

      return loadedOk;
    }

  }
}
#endif
