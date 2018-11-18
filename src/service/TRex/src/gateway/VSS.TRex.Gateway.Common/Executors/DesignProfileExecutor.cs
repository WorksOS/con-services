using System;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;


namespace VSS.TRex.Gateway.Common.Executors
{
  /// <summary>
  /// Processes the request to get design profile lines.
  /// </summary>
  public class DesignProfileExecutor : BaseExecutor
  {
    public DesignProfileExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler exceptionHandler)
      : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public DesignProfileExecutor()
    {
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      DesignProfileRequest request = item as DesignProfileRequest;
      
      if (request == null)
        ThrowRequestTypeCastException<DesignProfileRequest>();

      var siteModel = GetSiteModel(request.ProjectUid);

      var designProfileRequest = new VSS.TRex.Designs.GridFabric.Requests.DesignProfileRequest();

      CalculateDesignProfileResponse designProfileResponse = designProfileRequest.Execute(new CalculateDesignProfileArgument
      {
        ProjectID = siteModel.ID,
        DesignUid = request.DesignUid ?? Guid.Empty, // DesignDescriptor = new Designs.Models.DesignDescriptor { DesignID = request.DesignUid ?? Guid.Empty },
        CellSize = siteModel.Grid.CellSize,
        ProfilePath = new XYZ[] {new XYZ(request.StartX.Value, request.StartY.Value), new XYZ (request.EndX.Value, request.EndY.Value)}
      });

      if (designProfileResponse != null)
        return ConvertResult(designProfileResponse);

      throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
        "Failed to get requested Summary Volumes data"));
    }

    /// <summary>
    /// Converts CalculateDesignProfileResponse into CalculateDesignProfileResult data.
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    private DesignProfileResult ConvertResult(CalculateDesignProfileResponse result)
    {
      return DesignProfileResult.Create(result.Profile.Select(x => new Productivity3D.Models.ResultHandling.XYZS
      {
        X = x.X, Y = x.Y, Z = x.Z, Station = x.Station
      }).ToArray());
    }
  }
}
