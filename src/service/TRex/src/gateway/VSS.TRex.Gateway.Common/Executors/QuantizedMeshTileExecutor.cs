using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using System.IO;
using System.IO.Compression;
using VSS.TRex.QuantizedMesh.GridFabric.Requests;
using VSS.TRex.QuantizedMesh.GridFabric.Arguments;
using VSS.TRex.Filters;
using VSS.TRex.Types;

namespace VSS.TRex.Gateway.Common.Executors
{
  /// <summary>
  /// QuantizedMeshTileExecutor controls execution of quantized mesh execution
  /// </summary>
  public class QuantizedMeshTileExecutor : BaseExecutor
  {

    public QuantizedMeshTileExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler exceptionHandler) : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public QuantizedMeshTileExecutor()
    {
    }

    /*
    /// <summary>
    /// todo Temporary compression here. Part one
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>

    public static byte[] Compress(byte[] data)
    {
      using (var compressedStream = new MemoryStream())
      using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
      {
        zipStream.Write(data, 0, data.Length);
        zipStream.Close();
        return compressedStream.ToArray();
      }
    }

    
    /// <summary>
    /// Temporary part one method to return dummy tiles
    /// </summary>
    /// <returns></returns>
    protected byte[] GetTempDummyTile(int x, int y, int z)
    {
      var dstr = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
      var fname = Path.Combine("Executors", "TestData", "0.terrain");

      if (z == 0)
      {
          if ( y == 0)
            fname = Path.Combine("Executors", "TestData", "0.terrain");
          else
            fname = Path.Combine("Executors", "TestData", "1.terrain"); 
      }
      else
        fname = @"Executors\TestData\x.terrain";

      var fileInfo = new FileInfo(Path.Combine(dstr, fname));
      if (fileInfo.Exists)
      {
        var buffer = new byte[fileInfo.Length];
        using (var fileStream = fileInfo.OpenRead())
        {
          fileStream.Read(buffer, 0, buffer.Length);
          Console.WriteLine("Tile {0} sent", fileInfo);
          return buffer.ToArray(); // already compressed files

         // var compressed = Compress(buffer); // gzip
          //return compressed;
        }
      }
      return null;
    }

    */

    /// <summary>
    /// Process Quantized Mesh tile request from WebAPI controller 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns></returns>
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = item as QMTileRequest;

      if (request == null)
        ThrowRequestTypeCastException<QMTileRequest>();

      var siteModel = GetSiteModel(request?.ProjectUid);

      var filter = ConvertFilter(request?.Filter, siteModel);

      var qmRequest = new QuantizedMeshRequest();
      // todo should this be asyncexec
      var response = qmRequest.Execute(new QuantizedMeshRequestArgument
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(filter),
        X = request.X,
        Y = request.Y,
        Z = request.Z
      });

     // if (response.ResultStatus != RequestErrorStatus.OK)
       // Log
       // should result status go back even further
      return new QMTileResult(response.data); 

    }


  }
}
