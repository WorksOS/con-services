using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.TRex.Rendering.Servers.Client;
using VSS.TRex.Servers.Client;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.Gateway.Common.Executors
{
  public abstract class BaseExecutor : RequestExecutorContainer
  {
    protected BaseExecutor()
    {
    }

    protected BaseExecutor(IConfigurationStore configurationStore, ILoggerFactory logger, IServiceExceptionHandler exceptionHandler, ITileRenderingServer tileRenderServer, IMutableClientServer tagfileClientServer) 
      : base(configurationStore, logger, exceptionHandler, tileRenderServer, tagfileClientServer)
    {
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }

    protected ISiteModel GetSiteModel(Guid? ID)
    {
      ISiteModel siteModel = SiteModels.SiteModels.Instance().GetSiteModel(ID ?? new Guid());

      if (siteModel == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            $"Site model {ID} is unavailable"));
      }

      return siteModel;
    }
  }
}
