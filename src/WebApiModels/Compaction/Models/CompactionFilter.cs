using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Utilities;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Models
{
  public class CompactionFilter : IValidatable
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
    /// Only filter cell passes recorded when the vibratory drum was 'on'.  If set to null, returns all cell passes.  
    /// If true, returns only cell passes with the cell pass parameter and the drum was on.  If false, returns only cell passes with the cell pass parameter and the drum was off.
    /// </summary>
    [JsonProperty(PropertyName = "vibeState", Required = Required.Default)]
    public bool? vibeStateOn { get; private set; }

    /// <summary>
    /// Controls the cell pass from which to determine data based on its elevation.
    /// </summary>
    [JsonProperty(PropertyName = "elevationTypeId", Required = Required.Default)]
    public ElevationType? elevationType { get; private set; }

    /// <summary>
    /// The number of the 3D spatial layer (determined through bench elevation and layer thickness or the tag file) to be used as the layer type filter. Layer 3 is then the third layer from the
    /// datum elevation where each layer has a thickness defined by the layerThickness member.
    /// </summary>
    [Range(ValidationConstants3D.MIN_LAYER_NUMBER, ValidationConstants3D.MAX_LAYER_NUMBER)]
    [JsonProperty(PropertyName = "layerNumber", Required = Required.Default)]
    public int? layerNumber { get; private set; }

    /// <summary>
    /// A machine reported design. Cell passes recorded when a machine did not have this design loaded at the time is not considered.
    /// May be null/empty, which indicates no restriction. 
    /// </summary>
    [JsonProperty(PropertyName = "onMachineDesignID", Required = Required.Default)]
    public long? onMachineDesignID { get; private set; } //PDS not VL ID

    /// <summary>
    /// Cell passes are only considered if the machines that recorded them are included in this list of machines. Use machine ID (historically VL Asset ID), or Machine Name from tagfile, not both.
    /// This may be null, which is no restriction on machines. 
    /// </summary>
    [JsonProperty(PropertyName = "machines", Required = Required.Default)]
    public List<MachineDetails> contributingMachines { get; private set; }


    /// <summary>
    /// Private constructor
    /// </summary>
    private CompactionFilter()
    {
    }

    /// <summary>
    /// Create instance of Filter
    /// </summary>
    public static CompactionFilter CreateFilter
        (
        DateTime? startUTC,
        DateTime? endUTC,
        bool? vibeStateOn,
        ElevationType? elevationType,
        int? layerNumber,
        long? onMachineDesignID,
        List<MachineDetails> contributingMachines
        )
    {
      return new CompactionFilter
      {
        startUTC = startUTC,
        endUTC = endUTC,
        vibeStateOn = vibeStateOn,
        elevationType = elevationType,
        layerNumber = layerNumber,
        onMachineDesignID = onMachineDesignID,
        contributingMachines = contributingMachines
      };
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      if (contributingMachines != null)
      {
        foreach (var machine in contributingMachines)
          machine.Validate();
      }

      //Check date range parts
      if (this.startUTC.HasValue || this.endUTC.HasValue)
      {
        if (this.startUTC.HasValue && this.endUTC.HasValue)
        {
          if (this.startUTC.Value > this.endUTC.Value)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
                new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                    "StartUTC must be earlier than EndUTC"));
          }
        }
        else
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                  "If using a date range both dates must be provided"));
        }
      }

    }
  }
}
