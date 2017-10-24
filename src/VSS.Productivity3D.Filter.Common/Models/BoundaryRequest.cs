﻿using System;
using System.Net;
using Newtonsoft.Json;
using VSS.MasterData.Models.Handlers;

namespace VSS.Productivity3D.Filter.Common.Models
{
  public class BoundaryRequest
  {
    /// <summary>
    /// The BoundaryUid whose boundary is to be updated, empty for create.
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public string BoundaryUid { get; set; } = string.Empty;

    /// <summary>
    /// The name of the filter boundary.
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The filter boundary geofence data in WKT format.
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public string BoundaryPolygonWKT { get; set; } = string.Empty;

    public BoundaryRequest()
    { }

    /// <summary>
    /// Returns a new instance of <see cref="BoundaryRequest"/> using the provided inputs.
    /// </summary>
    public static BoundaryRequest Create(string boundaryUid, string name, string boundaryPolygonWKT)
    {
      return new BoundaryRequest
      {
        BoundaryUid = boundaryUid,
        Name = name,
        BoundaryPolygonWKT = boundaryPolygonWKT
      };
    }

    public virtual void Validate(IServiceExceptionHandler serviceExceptionHandler)
    {
      if (!string.IsNullOrEmpty(BoundaryUid) && Guid.TryParse(BoundaryUid, out Guid boundaryUidGuid) == false)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 59);
      }

      if (string.IsNullOrEmpty(Name))
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 3);
      }

      if (string.IsNullOrEmpty(BoundaryPolygonWKT))
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 63);
      }
    }
  }
}