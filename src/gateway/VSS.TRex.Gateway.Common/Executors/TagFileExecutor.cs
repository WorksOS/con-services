using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Gateway.Common.ResultHandling;
using VSS.TRex.GridFabric.Interfaces;
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

      ContractExecutionResult result = new ContractExecutionResult(1, "Unknown exception");

      try
      {
        log.LogInformation($"#In# TagFileExecutor. Process tagfile:{request.FileName}, Project:{request.ProjectUid}, TCCOrgID:{request.OrgId}");

        SubmitTAGFileRequest submitRequest = new SubmitTAGFileRequest();
        SubmitTAGFileRequestArgument arg = null;

        arg = new SubmitTAGFileRequestArgument()
        {
          ProjectID = request.ProjectUid,
          AssetID = null, // not supplied by interface
          TAGFileName = request.FileName,
          TagFileContent = request.Data,
          TCCOrgID = request.OrgId
        };

        var res = submitRequest.Execute(arg);

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
    /// Processes the tagfile request asynchronously.
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      throw new NotImplementedException();
    }

  }
}
