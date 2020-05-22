using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.Executors.Design;

namespace VSS.TRex.Gateway.Common.Executors.Design
{
  public class AlignmentGeometryResponseLabel
  {
    /// <summary>
    /// Measured (as in walked) distance along the alignment from the start of the alignment, expressed in meters
    /// </summary>
    [JsonProperty(PropertyName = "stn", Required = Required.Always)]
    public double Stn { get; }

    /// <summary>
    /// Contains the WGS84 latitude (expressed decimal degrees) of the test insertion position
    /// </summary>
    [JsonProperty(PropertyName = "lat", Required = Required.Always)]
    public double Lat { get; }

    /// <summary>
    /// Contains the WGS84 longitude (expressed decimal degrees) of the test insertion position
    /// </summary>
    [JsonProperty(PropertyName = "lon", Required = Required.Always)]
    public double Lon { get; }

    /// <summary>
    /// Text rotation expressed as a survey angle (north is 0, increasing clockwise), in decimal degrees.
    /// </summary>
    [JsonProperty(PropertyName = "rot", Required = Required.Always)]
    public double Rot { get; }
    public AlignmentGeometryResponseLabel(double stn, double lat, double lon, double rot)
    {
      Stn = stn;
      Lat = lat;
      Lon = lon;
      Rot = rot;
    }
  }

  public class AlignmentGeometryResult : ContractExecutionResult
  {
    /// <summary>
    /// The array of vertices describing a poly line representation of the alignment center line
    /// Vertices is an array of arrays containing three doubles, containing the WGS84 latitude (index 0, decimal degrees),
    /// longitude (index 1, decimal degrees) and station (ISO units; meters) of each point along the alignment.
    /// </summary>
    [JsonProperty(PropertyName = "vertices", Required = Required.Always)]
    public double[][] Vertices { get; }

    /// <summary>
    /// The array of labels to be rendered along the alignment. These are generated according to the interval specified
    /// in the request and relevant features within the alignment
    /// </summary>
    [JsonProperty(PropertyName = "labels", Required = Required.Always)]
    public AlignmentGeometryResponseLabel[] Labels { get; }

    /// <summary>
    /// Constructs an alignment master geometry result from supplied vertices and labels
    /// </summary>
    /// <param name="code"></param>
    /// <param name="vertices"></param>
    /// <param name="labels"></param>
    /// <param name="message"></param>
    public AlignmentGeometryResult(int code, double[][] vertices, AlignmentGeometryResponseLabel[] labels, string message = DefaultMessage) : base(code, message)
    {
      Vertices = vertices;
      Labels = labels;
    }
  }

  /// <summary>
  /// Processes the request to get design alignment master alignment geometry from the TRex site model/project.
  /// </summary>
  /// 
  public class AlignmentMasterGeometryExecutor : BaseExecutor
  {
    public AlignmentMasterGeometryExecutor(IConfigurationStore configStore, ILoggerFactory logger, IServiceExceptionHandler exceptionHandler)
      : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public AlignmentMasterGeometryExecutor()
    {
    }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as VSS.TRex.Gateway.Common.Requests.AlignmentDesignGeometryRequest;

      if (request == null)
        ThrowRequestTypeCastException<VSS.TRex.Gateway.Common.Requests.AlignmentDesignGeometryRequest>();

      var siteModel = GetSiteModel(request.ProjectUid);

      var geometryRequest = new VSS.TRex.Designs.GridFabric.Requests.AlignmentDesignGeometryRequest();

      var geometryResponse = await geometryRequest.ExecuteAsync(new AlignmentDesignGeometryArgument
      {
        ProjectID = siteModel.ID,
       
//        ReferenceDesign = referenceDesign
      });

//      if (geometryResponse != null &&
//          geometryResponse.RequestResult != DesignProfilerRequestResult.OK)
//        return new AlignmentGeometryResult(ContractExecutionStatesEnum.ExecutedSuccessfully,
//          geometryResponse.vertices, geometryResponse.labels);

      throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
        "Failed to get requested Alignment Design geometry."));
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
