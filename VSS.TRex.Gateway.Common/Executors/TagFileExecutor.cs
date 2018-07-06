using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Designs.Storage;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.Models;
using VSS.TRex.Gateway.Common.ResultHandling;
using VSS.TRex.Geometry;
using VSS.TRex.Rendering.GridFabric.Arguments;
using VSS.TRex.Rendering.Implementations.Core2.GridFabric.Responses;
using VSS.TRex.Rendering.Servers.Client;
using VSS.TRex.Servers;
using VSS.TRex.Servers.Client;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Gateway.Common.Executors
{
  public class TagFileExecutor : RequestExecutorContainer
  {


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


    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      ContractExecutionResult result = null;
      return result;
    }
    /*
      ContractExecutionResult result = null;

   //   var request = item as TileRequest;

      try
      {
   //     return TileResult.CreateTileResult(response?.TileBitmap);
      }
      catch (Exception E)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
                $"Exception: {E.Message}"));
      }
    }
    */


    /// <summary>
    /// Processes the tagfile request asynchronously.
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      throw new NotImplementedException();
    }

  }
}
