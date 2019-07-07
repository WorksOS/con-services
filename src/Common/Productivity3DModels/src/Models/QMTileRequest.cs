using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models.Designs;
using VSS.Productivity3D.Models.Utilities;

namespace VSS.Productivity3D.Models.Models
{
  public class QMTileRequest : RaptorHelper
  {


    /// <summary>
    /// An identifying string from the caller
    /// </summary>
    [JsonProperty(PropertyName = "callId", Required = Required.Default)]
    public Guid? CallId { get; set; }

    /// <summary>
    /// The descriptor for the design to be used for volume or cut/fill based thematic renderings.
    /// </summary>
    [JsonProperty(PropertyName = "designDescriptor", Required = Required.Default)]
    public DesignDescriptor DesignDescriptor { get; protected set; }

    /// <summary>
    /// The base or earliest filter to be used.
    /// </summary>
    [JsonProperty(PropertyName = "filter1", Required = Required.Default)]
    public FilterResult Filter1 { get; set; }

    /// <summary>
    /// The ID of the base or earliest filter to be used.
    /// </summary>
    [JsonProperty(PropertyName = "filterId1", Required = Required.Default)]
    public long FilterId1 { get; set; }

    /// <summary>
    /// The bounding box enclosing the area to be rendered. The bounding box is expressed in terms of WGS84 latitude and longitude positions, expressed in radians.
    /// Value may be null but either this or the bounding box in grid coordinates must be provided.
    /// </summary>
    [JsonProperty(PropertyName = "boundBoxLL", Required = Required.Default)]
    public BoundingBox2DLatLon BoundBoxLatLon { get; set; }

    /// <summary>
    /// The bounding box enclosing the area to be rendered. The bounding box is expressed in terms of cartesian grid coordinates in the project coordinate system, expressed in meters.
    /// Value may be null but either this or the bounding box in lat/lng coordinates must be provided.
    /// </summary>
    [JsonProperty(PropertyName = "boundBoxGrid", Required = Required.Default)]
    public BoundingBox2DGrid BoundBoxGrid { get; set; }


    /// <summary>
    /// Default public constructor.
    /// </summary>
    public QMTileRequest()
    { }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="callId"></param>
    /// <param name="designDescriptor"></param>
    /// <param name="filter1"></param>
    /// <param name="filterId1"></param>
    /// <param name="boundingBoxLatLon"></param>
    /// <param name="boundingBoxGrid"></param>
    public QMTileRequest(
      long projectId,
      Guid? projectUid,
      Guid? callId,
      DesignDescriptor designDescriptor,
      FilterResult filter1,
      long filterId1,
      BoundingBox2DLatLon boundingBoxLatLon,
      BoundingBox2DGrid boundingBoxGrid)
    {
      ProjectId = projectId;
      ProjectUid = projectUid;
      CallId = callId;
      DesignDescriptor = designDescriptor;
      Filter1 = filter1;
      FilterId1 = filterId1;
      BoundBoxLatLon = boundingBoxLatLon;
      BoundBoxGrid = boundingBoxGrid;
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();
      if (BoundBoxLatLon == null && BoundBoxGrid == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Bounding box required either in lat/lng or grid coordinates"));
      }

      if (BoundBoxLatLon != null && BoundBoxGrid != null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Only one bounding box is allowed"));
      }

    }




  }
}
