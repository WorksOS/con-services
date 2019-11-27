using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Productivity3D.Models.Enums;
using VSS.TRex.Gateway.Common.ResultHandling;
using VSS.TRex.TAGFiles.GridFabric.Arguments;
using VSS.TRex.TAGFiles.GridFabric.Requests;

namespace VSS.TRex.Gateway.Common.Executors
{
  public class TagFileExecutor : RequestExecutorContainer
  {

    /// <summary>
    /// TagFileExecutor
    /// </summary>
    /// <param name="configStore"></param>
    /// <param name="logger"></param>
    /// <param name="exceptionHandler"></param>
    public TagFileExecutor(IConfigurationStore configStore,
        ILoggerFactory logger, IServiceExceptionHandler exceptionHandler) : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public TagFileExecutor()
    {
    }

    /// <summary>
    /// Process tagfile request
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns></returns>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as CompactionTagFileRequest;

      var result = new ContractExecutionResult((int)TRexTagFileResultCode.TRexUnknownException, "TRex unknown result (TagFileExecutor.ProcessEx)");

      try
      {
        log.LogInformation($"#In# TagFileExecutor. Process tagfile:{request.FileName}, Project:{request.ProjectUid}, TCCOrgID:{request.OrgId}");

        var submitRequest = new SubmitTAGFileRequest();

        var arg = new SubmitTAGFileRequestArgument
        {
          ProjectID = request.ProjectUid,
          AssetID = null, // not available via TagFileController APIs
          TAGFileName = request.FileName,
          TagFileContent = request.Data,
          TCCOrgID = request.OrgId
        };

        var res = await submitRequest.ExecuteAsync(arg);

        if (res.Success)
          result = TagFileResult.Create(0, ContractExecutionResult.DefaultMessage);
        else
          result = TagFileResult.Create(res.Code, res.Message);

      }
      finally
      {
        if (request != null)
          log.LogInformation($"#Out# TagFileExecutor. Process tagfile:{request.FileName}, Project:{request.ProjectUid}, Submission Code: {result.Code}, Message:{result.Message}");
        else
          log.LogInformation($"#Out# TagFileExecutor. Invalid request");

      }
      return result;
    }


    /// <summary>
    /// Processes the tile request synchronously.
    /// </summary>
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }
  }
}
