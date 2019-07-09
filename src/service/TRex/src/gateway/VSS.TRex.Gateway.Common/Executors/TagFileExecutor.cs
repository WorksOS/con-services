using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
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
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = item as CompactionTagFileRequest;

      var result = new ContractExecutionResult((int)TRexTagFileResultCode.TRexUnknownException, "TRex unknown result (TagFileExecutor.ProcessEx)");

      try
      {
        log.LogInformation($"#In# TagFileExecutor. Process tag file: {request.FileName}, Project:{request.ProjectUid}, TCCOrgID:{request.OrgId}");

        var submitRequest = new SubmitTAGFileRequest();

        var arg = new SubmitTAGFileRequestArgument
        {
          ProjectID = request.ProjectUid,
          AssetID = null, // not available via TagFileController APIs
          TAGFileName = request.FileName,
          TagFileContent = request.Data,
          TCCOrgID = request.OrgId
        };

        var res = submitRequest.Execute(arg);
        result = res.Success 
          ? TagFileResult.Create(0, ContractExecutionResult.DefaultMessage) : TagFileResult.Create(res.Code, res.Message);

      }
      finally
      {
        log.LogInformation(request != null 
          ? $"#Out# TagFileExecutor. Process tag file: {request.FileName}, Project:{request.ProjectUid}, Submission Code: {result.Code}, Message:{result.Message}" 
          : "#Out# TagFileExecutor. Invalid request");
      }
      return result;

    }


    /// <summary>
    /// Processes the tag file request asynchronously.
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      throw new NotImplementedException();
    }

  }
}
