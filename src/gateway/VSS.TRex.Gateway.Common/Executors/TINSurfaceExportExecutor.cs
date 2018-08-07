﻿using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Exports.Surfaces.GridFabric;
using VSS.TRex.Exports.Surfaces.Requestors;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Gateway.Common.Requests;
using VSS.TRex.Gateway.Common.ResultHandling;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.Gateway.Common.Executors
{
  public class TINSurfaceExportExecutor : BaseExecutor
  {
    /// <summary>
    /// TagFileExecutor
    /// </summary>
    /// <param name="configStore"></param>
    /// <param name="logger"></param>
    /// <param name="exceptionHandler"></param>
    /// <param name="tINSurfaceExportRequestor"></param>
    public TINSurfaceExportExecutor(IConfigurationStore configStore,
      ILoggerFactory logger, IServiceExceptionHandler exceptionHandler, ITINSurfaceExportRequestor tINSurfaceExportRequestor) : base(configStore, logger, exceptionHandler, null, null)
    {
      this.tINSurfaceExportRequestor = tINSurfaceExportRequestor;
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public TINSurfaceExportExecutor()
    {
    }

    private Guid[] GetSurveyedSurfaceExclusionList(ISiteModel siteModel, bool includeSurveyedSurfaces)
    {
      return siteModel.SurveyedSurfaces == null || includeSurveyedSurfaces ? new Guid[0] : siteModel.SurveyedSurfaces.Select(x => x.ID).ToArray();
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = item as TINSurfaceExportRequest;

      var siteModel = GetSiteModel(request.ProjectUid);

      var response = tINSurfaceExportRequestor.Execute(new TINSurfaceRequestArgument
          {
            Tolerance = request.Tolerance ?? 0.0,
            ProjectID = request.ProjectUid.Value,
            Filters = new FilterSet(ConvertFilter(request.Filter, siteModel))
          }
        );

      return TINSurfaceExportResult.CreateTINResult(response.data);
    }

    /// <summary>
    /// Processes the surface request asynchronously.
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      throw new NotImplementedException();
    }
  }
}
