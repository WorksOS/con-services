using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Designs.Storage;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.ResultHandling;
using VSS.TRex.Geometry;
using VSS.TRex.Rendering.GridFabric.Arguments;
using VSS.TRex.Rendering.Implementations.Core2.GridFabric.Responses;
using VSS.TRex.Rendering.Servers.Client;
using VSS.TRex.Servers;
using VSS.TRex.Servers.Client;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.TAGFiles.GridFabric.Arguments;
using VSS.TRex.TAGFiles.GridFabric.Requests;
using VSS.TRex.Types;

namespace VSS.TRex.Gateway.Common.Executors
{
  public class TagFileExecutor : RequestExecutorContainer
  {


    public TagFileExecutor(IConfigurationStore configStore,
        ILoggerFactory logger, IServiceExceptionHandler exceptionHandler, IMutableClientServer tagfileClientServer) : base(configStore, logger, exceptionHandler, null, tagfileClientServer)
    {
    }


    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public TagFileExecutor()
    {

    }


    protected override ContractExecutionResult ProcessEx<T>(T item)
    {

      ContractExecutionResult result = new ContractExecutionResult(1,"Unknown exception");
      var request = item as TagFileRequest;

      try
      {
        log.LogInformation($"#In# TagFileExecutor. Process tagfile:{request.FileName}, Project:{request.ProjectUID}, TCCOrgID:{request.TccOrgId}");

        SubmitTAGFileRequest submitRequest = new SubmitTAGFileRequest();
        SubmitTAGFileRequestArgument arg = null;

        arg = new SubmitTAGFileRequestArgument()
              {
                  ProjectID   = request.ProjectUID,
                  AssetID     = request.AssetUID,
                 // AssetID     = Guid.Parse("00000000-0000-0000-0000-000000000001"), // until TFA is ready
                  TAGFileName = request.FileName,
                  TagFileContent = request.Data,
                  TCCOrgID    = request.TccOrgId
              };

        var res = submitRequest.Execute(arg);

        if (res.Success)
          result = TagFileResult.Create(0, ContractExecutionResult.DefaultMessage);
        else
          result = TagFileResult.Create(1, res.Exception); // todo

      }
      finally
      {
        log.LogInformation($"#Out# TagFileExecutor. Process tagfile:{request.FileName}, Project:{request.ProjectUID}, Submission Code: {result.Code}, Message:{result.Message}");
    
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
