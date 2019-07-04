#if NET_4_7 
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.Hydrology.WebApi.Abstractions.ResultsHandling;
using VSS.Hydrology.WebApi.DXF;
using VSS.Hydrology.WebApi.DXF.Entities;
using VSS.Hydrology.WebApi.TTM;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Hydrology.WebApi.Common.Utilities
{
  public class TTMtoDXFConverter
  {
    private readonly ILogger _log;
    private int _ttmTriangleCount = 0;
    public int TTMTriangleCount() => _ttmTriangleCount;
    private int _dxfTriangleCount = 0;
    public int DXFTriangleCount() => _dxfTriangleCount;


    public TTMtoDXFConverter(ILogger log)
    {
      _log = log;
    }

    public TTMtoDXFConverter(ILoggerFactory loggerFactory)
    {
      _log = loggerFactory.CreateLogger<TTMtoDXFConverter>();
    }

    public bool CreateDXF(MemoryStream sourceTTMFileStream, string targetDXFFileName)
    {
      _log.LogDebug($"{nameof(CreateDXF)}: targetDXFFileName {targetDXFFileName}");

      var tin = new TrimbleTINModel();
      tin.LoadFromStream(sourceTTMFileStream);

      // can you have ponding where < 3 triangles? hydro libraries throw exception where 2 triangles anyways
      if (tin.Triangles.Count < 3)
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(2005, "Current ground design has too few TIN entities, must have at least 3."));

      _ttmTriangleCount = tin.Triangles.Count;
      _log.LogDebug($"{nameof(CreateDXF)}: TTM triangleCount {tin.Triangles.Count}");

      var dxf = new DxfDocument();

      foreach (var triangle in tin.Triangles)
      {
        var targetTriangle = new Face3d(
          new Vector3(triangle.Vertices[0].X, triangle.Vertices[0].Y, triangle.Vertices[0].Z),
          new Vector3(triangle.Vertices[1].X, triangle.Vertices[1].Y, triangle.Vertices[1].Z),
          new Vector3(triangle.Vertices[2].X, triangle.Vertices[2].Y, triangle.Vertices[2].Z));
        dxf.AddEntity(targetTriangle);
      }

      _log.LogDebug($"{nameof(CreateDXF)}: DXF triangleCount {dxf.Faces3d.Count()}");
      dxf.Save(targetDXFFileName);
      _dxfTriangleCount = dxf.Faces3d.Count();

      return _ttmTriangleCount == _dxfTriangleCount;
    }

    //public bool CreateDXF(string sourceTTMFileName, string targetDXFFileName)
    //{
    //  var loadedOk = false;
    //  _log.LogDebug(
    //    $"{nameof(CreateDXF)}: sourceFileLength {new FileInfo(sourceTTMFileName).Length} sourceFileName {sourceTTMFileName}");

    //  try
    //  {
    //    var tin = new TrimbleTINModel();
    //    tin.LoadFromFile(sourceTTMFileName);
    //    _log.LogDebug($"{nameof(CreateDXF)}: triangleCount in source {tin.Triangles.Count}");

    //    var dxf = new DxfDocument();

    //    foreach (var triangle in tin.Triangles)
    //    {
    //      var targetTriangle = new Face3d(
    //        new Vector3(triangle.Vertices[0].X, triangle.Vertices[0].Y, triangle.Vertices[0].Z),
    //        new Vector3(triangle.Vertices[1].X, triangle.Vertices[1].Y, triangle.Vertices[1].Z),
    //        new Vector3(triangle.Vertices[2].X, triangle.Vertices[2].Y, triangle.Vertices[2].Z));
    //      dxf.AddEntity(targetTriangle);
    //      _log.LogDebug(
    //        $"{nameof(CreateDXF)} source triangle {JsonConvert.SerializeObject(triangle)}, target triangle {JsonConvert.SerializeObject(targetTriangle)}");
    //    }

    //    _log.LogDebug($"{nameof(CreateDXF)}: triangeCount in target {dxf.Faces3d.Count()}");
    //    dxf.Save(targetDXFFileName);
    //    loadedOk = true;
    //  }
    //  catch (Exception e)
    //  {
    //    _log.LogDebug(e, $"{nameof(CreateDXF)}: Exception converting stream");
    //    throw e;
    //  }


    //  // this check is optional but recommended before loading a DXF file
    //  var dxfVersion = DxfDocument.CheckDxfFileVersion(targetDXFFileName);
    //  // netDxf is only compatible with AutoCad2000 and higher DXF version
    //  if (dxfVersion < DxfVersion.AutoCad2000)
    //    throw new InvalidOperationException(
    //      $"{nameof(CreateDXF)} incorrect DXF file version: {dxfVersion}");
    //  // load file
    //  var loaded = DxfDocument.Load(targetDXFFileName);
    //  _log.LogDebug($"{nameof(CreateDXF)}: triangleCount in re-loaded target {loaded.Faces3d.Count()}");

    //  return loadedOk;
    //}

  }
}
#endif
