using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Interfaces;
using VSS.MasterData.Models.Internal;
using VSS.MasterData.Models.Utilities;

namespace VSS.MasterData.Models.Models
{
  /// <summary>
  /// Defines all the filter parameters that may be supplied and validates them.
  /// </summary>
  public class Filter : IValidatable
  {
    /// <summary>
    /// The 'start' time for a time based filter. Data recorded earlier to this time is not considered.
    /// Optional. If not present then there is no start time bound.
    /// </summary>
    [JsonProperty(PropertyName = "startUTC", Required = Required.Default)]
    public DateTime? startUTC { get; private set; }

    /// <summary>
    /// The 'end' time for a time based filter. Data recorded after this time is not considered.
    /// Optional. If not present there is no end time bound.
    /// </summary>
    [JsonProperty(PropertyName = "endUTC", Required = Required.Default)]
    public DateTime? endUTC { get; private set; }

    /// <summary>
    /// Gets the date range type for this filter, e.g. day, week, project extents.
    /// </summary>
    [JsonProperty(PropertyName = "dateRangeType", Required = Required.Default)]
    public DateRangeType? DateRangeType { get; private set; }

    /// <summary>
    /// A design file unique identifier. Used as a spatial filter.
    /// </summary>
    [JsonProperty(PropertyName = "designUID", Required = Required.Default)]
    public string designUID { get; protected set; }

    /// <summary>
    /// A comma-separated list of contributing machines.
    /// Optional as it is not used in the proper functioning of a filter.
    /// </summary>
    [JsonProperty(PropertyName = "contributingMachines", Required = Required.Default)]
    public List<MachineDetails> contributingMachines { get; private set; }

    /// <summary>
    /// A machine reported design. Cell passes recorded when a machine did not have this design loaded at the time is not considered.
    /// May be null/empty, which indicates no restriction. 
    /// </summary>
    [JsonProperty(PropertyName = "onMachineDesignID", Required = Required.Default)]
    public long? onMachineDesignID { get; private set; } //PDS not VL ID

    /// <summary>
    /// Controls the cell pass from which to determine data based on its elevation.
    /// </summary>
    [JsonProperty(PropertyName = "elevationType", Required = Required.Default)]
    public ElevationType? elevationType { get; private set; }

    /// <summary>
    /// Only filter cell passes recorded when the vibratory drum was 'on'.  If set to null, returns all cell passes.  If true, returns only cell passes with the cell pass parameter and the drum was on.  If false, returns only cell passes with the cell pass parameter and the drum was off.
    /// </summary>
    [JsonProperty(PropertyName = "vibeStateOn", Required = Required.Default)]
    public bool? vibeStateOn { get; private set; }

    /// <summary>
    /// The boundary/geofence unique identifier. Used as a spatial filter.
    /// </summary>
    [JsonProperty(PropertyName = "polygonUID", Required = Required.Default)]
    public string polygonUID { get; protected set; }

    /// <summary>
    /// name of polygonLL 
    /// </summary>
    [JsonProperty(PropertyName = "polygonName", Required = Required.Default)]
    public string polygonName { get; private set; }

    /// <summary>
    /// A polygon to be used as a spatial filter boundary. The vertices are WGS84 positions
    /// </summary>
    [JsonProperty(PropertyName = "polygonLL", Required = Required.Default)]
    public List<WGSPoint> polygonLL { get; private set; }

    /// <summary>
    /// Only use cell passes recorded when the machine was driving in the forwards direction. If true, only returns machines travelling forward, if false, returns machines travelling in reverse, if null, returns all machines.
    /// </summary>
    [JsonProperty(PropertyName = "forwardDirection", Required = Required.Default)]
    public bool? forwardDirection { get; private set; }

    /// <summary>
    /// The number of the 3D spatial layer (determined through bench elevation and layer thickness or the tag file) to be used as the layer type filter. Layer 3 is then the third layer from the
    /// datum elevation where each layer has a thickness defined by the layerThickness member.
    /// </summary>
    [Range(ValidationConstants.MIN_LAYER_NUMBER, ValidationConstants.MAX_LAYER_NUMBER)]
    [JsonProperty(PropertyName = "layerNumber", Required = Required.Default)]
    public int? layerNumber { get; private set; }

    public bool HasData() =>
      startUTC.HasValue ||
      endUTC.HasValue ||
      DateRangeType.HasValue ||
      onMachineDesignID.HasValue ||
      vibeStateOn.HasValue ||
      elevationType.HasValue ||
      layerNumber.HasValue ||
      forwardDirection.HasValue ||
      (contributingMachines != null && contributingMachines.Count > 0) ||
      (polygonLL != null && polygonLL.Count > 0);

    public void AddBoundary(string polygonUID, string polygonName, List<WGSPoint> polygonLL)
    {
      this.polygonUID = polygonUID;
      this.polygonName = polygonName;
      this.polygonLL = polygonLL;
    }

    /// <summary>
    /// Create instance of Filter
    /// </summary>
    public static Filter CreateFilter
      (
        DateTime? startUtc,
        DateTime? endUtc,
        string designUID,
        List<MachineDetails> contributingMachines,
        long? onMachineDesignID,
        ElevationType? elevationType,
        bool? vibeStateOn,
        List<WGSPoint> polygonLL,
        bool? forwardDirection,
        int? layerNumber,
        string polygonUID = null,
        string polygonName = null
      )
    {
      return new Filter
      {
        startUTC = startUtc,
        endUTC = endUtc,
        designUID = designUID,
        contributingMachines = contributingMachines,
        onMachineDesignID = onMachineDesignID,
        elevationType = elevationType,
        vibeStateOn = vibeStateOn,
        polygonLL = polygonLL,
        forwardDirection = forwardDirection,
        layerNumber = layerNumber,
        polygonUID = polygonUID,
        polygonName = polygonName
      };
    }

    public string ToJsonString()
    {
      var filter = CreateFilter(startUTC, endUTC, designUID, contributingMachines, onMachineDesignID, elevationType, vibeStateOn, polygonLL, forwardDirection, layerNumber, polygonUID, polygonName);

      return JsonConvert.SerializeObject(filter);
    }

    public void 
      Validate([FromServices] IServiceExceptionHandler serviceExceptionHandler)
    {
      //Check date range properties
      if (startUTC.HasValue || endUTC.HasValue)
      {
        if (startUTC.HasValue && endUTC.HasValue)
        {
          if (startUTC.Value > endUTC.Value)
          {
            serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 29);
          }
        }
        else
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 30);
        }
      }

      if (designUID != null && Guid.TryParse(designUID, out Guid designUIDGuid) == false)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 31);
      }

      if (contributingMachines != null)
      {
        foreach (var machine in contributingMachines)
        {
          machine.Validate();
        }
      }

      //Check boundary if provided
      //Raptor handles any weird boundary you give it and automatically closes it if not closed already therefore we just need to check we have at least 3 points
      if (polygonLL != null && polygonLL.Count < 3)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 35);
      }

      if ((!string.IsNullOrEmpty(polygonUID) || !string.IsNullOrEmpty(polygonName) || polygonLL != null)
        && (string.IsNullOrEmpty(polygonUID) || string.IsNullOrEmpty(polygonName) || polygonLL == null))
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 45);
      }
    }
  }
}