using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Interfaces;
using VSS.MasterData.Models.Utilities;
using VSS.Productivity3D.Common.Models;

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
    /// A design file unique identifier. Used as a spatial filter.
    /// </summary>
    [JsonProperty(PropertyName = "designUid", Required = Required.Default)]
    public string designUid { get; protected set; }

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
    [JsonProperty(PropertyName = "polygonUid", Required = Required.Default)]
    public string polygonUid { get; protected set; }

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

    /// <summary>
    /// layerType indicates the layer analysis method to be used for determining layers from cell passes. Some of the layer types are implemented as a 
    /// 3D spatial volume inclusion implemented via 3D spatial filtering. Only cell passes whose three dimensional location falls within the bounds
    /// 3D spatial volume are considered. If it is required to apply layer filter, lift analysis method and corresponding parameters should be specified here.
    ///  Otherwise (build lifts but do not filter, only do production data analysis) Lift Layer Analysis setting should be specified in LiftBuildSettings.
    /// </summary>
    [JsonProperty(PropertyName = "layerType", Required = Required.Default)]
    public FilterLayerMethod? layerType { get; private set; }


    public void AddBoundary(string polygonUid, string polygonName, List<WGSPoint> polygonLL)
    {
      this.polygonUid = polygonUid;
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
        string designUid,
        List<MachineDetails> contributingMachines,
        long? onMachineDesignID,
        ElevationType? elevationType,
        bool? vibeStateOn,
        List<WGSPoint> polygonLL,
        bool? forwardDirection,
        int? layerNumber,
        FilterLayerMethod? layerType,
        string polygonUid = null,
        string polygonName = null
      )
    {
      return new Filter()
      {
        startUTC = startUtc,
        endUTC = endUtc,
        designUid = designUid,
        contributingMachines = contributingMachines,
        onMachineDesignID = onMachineDesignID,
        elevationType = elevationType,
        vibeStateOn = vibeStateOn,
        polygonLL = polygonLL,
        forwardDirection = forwardDirection,
        layerNumber = layerNumber,
        layerType = layerType,
        polygonUid = polygonUid,
        polygonName = polygonName
      };
    }
    
    public string ToJsonString()
    {
      var filter = CreateFilter(startUTC, endUTC, designUid, contributingMachines, onMachineDesignID, elevationType, vibeStateOn, polygonLL, forwardDirection, layerNumber, layerType, polygonUid, polygonName);

      return JsonConvert.SerializeObject(filter);
    }

    public void Validate([FromServices] IServiceExceptionHandler serviceExceptionHandler)
    {
      //Check date range properties
      if (startUTC.HasValue || endUTC.HasValue)
      {
        if (startUTC.HasValue && endUTC.HasValue)
        {
          if (startUTC.Value > endUTC.Value)
            serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 29);
        }
        else
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 30);
      }

      Guid designUidGuid;
      if (designUid != null && Guid.TryParse(designUid, out designUidGuid) == false)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 31);

      if (contributingMachines != null)
      {
        foreach (var machine in contributingMachines)
          machine.Validate();
      }

      //Check layer filter parts
      if (layerNumber.HasValue && !layerType.HasValue)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 34);

      if (layerType.HasValue)
      {
        if (layerType.Value == FilterLayerMethod.Invalid ||
            (layerType.Value != FilterLayerMethod.None && layerType.Value != FilterLayerMethod.TagfileLayerNumber &&
             layerType.Value != FilterLayerMethod.MapReset))
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 32);

        if (layerType.Value == FilterLayerMethod.TagfileLayerNumber)
        { 
            if (!layerNumber.HasValue)
              serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 33);
        }
      }

      //Check boundary if provided
      //Raptor handles any weird boundary you give it and automatically closes it if not closed already therefore we just need to check we have at least 3 points
      if (polygonLL != null && polygonLL.Count < 3)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 35);

      if ((!string.IsNullOrEmpty(polygonUid) || !string.IsNullOrEmpty(polygonName) || polygonLL != null )
            && (string.IsNullOrEmpty(polygonUid) || string.IsNullOrEmpty(polygonName) || polygonLL == null)
        )
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 45);

    }

  }
}
