using System;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Rendering.Servers.Client;
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
      ISiteModel siteModel = ID.HasValue ? DIContext.Obtain<ISiteModels>().GetSiteModel(ID.Value) : null;

      if (siteModel == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            $"Site model {ID} is unavailable"));
      }

      return siteModel;
    }

    protected CombinedFilter ConvertFilter(FilterResult filter, ISiteModel siteModel)
    {
      if (filter == null)
        return new CombinedFilter();//TRex doesn't like null filter

      var combinedFilter = AutoMapperUtility.Automapper.Map<FilterResult, CombinedFilter>(filter);
      // TODO Map the excluded surveyed surfaces from the filter.SurveyedSurfaceExclusionList to the ones that are in the TRex database
      bool includeSurveyedSurfaces = filter.SurveyedSurfaceExclusionList == null || filter.SurveyedSurfaceExclusionList.Count == 0;
      var excludedIds = siteModel.SurveyedSurfaces == null || includeSurveyedSurfaces ? new Guid[0] : siteModel.SurveyedSurfaces.Select(x => x.ID).ToArray();
      combinedFilter.AttributeFilter.SurveyedSurfaceExclusionList = excludedIds;
      return combinedFilter;
    }
  }
}
