using System;
using System.Collections.Generic;
using RaptorSvcAcceptTestsCommon.Models;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class CompactionFilter
  {
    /// <summary>
    /// The 'start' time for a time based filter. Data recorded earlier to this time is not considered.
    /// Optional. If not present then there is no start time bound.
    /// </summary>
    public DateTime? startUTC { get; set; }

    /// <summary>
    /// The 'end' time for a time based filter. Data recorded after this time is not considered.
    /// Optional. If not present there is no end time bound.
    /// </summary>
    public DateTime? endUTC { get; set; }
    /// <summary>
    /// Only filter cell passes recorded when the vibratory drum was 'on'.  If set to null, returns all cell passes.  If true, returns only cell passes with the cell pass parameter and the drum was on.  If false, returns only cell passes with the cell pass parameter and the drum was off.
    /// </summary>
    public bool? vibeStateOn { get; set; }

    /// <summary>
    /// Controls the cell pass from which to determine data based on its elevation.
    /// </summary>
    public ElevationType? elevationType { get; set; }

    /// <summary>
    /// The number of the 3D spatial layer (determined through bench elevation and layer thickness or the tag file) to be used as the layer type filter. Layer 3 is then the third layer from the
    /// datum elevation where each layer has a thickness defined by the layerThickness member.
    /// </summary>
    public int? layerNumber { get; set; }

    /// <summary>
    /// A machine reported design. Cell passes recorded when a machine did not have this design loaded at the time is not considered.
    /// May be null/empty, which indicates no restriction. 
    /// </summary>
    public long? onMachineDesignID { get; set; } //PDS not VL ID

    /// <summary>
    /// Cell passes are only considered if the machines that recorded them are included in this list of machines. Use machine ID (historically VL Asset ID), or Machine Name from tagfile, not both.
    /// This may be null, which is no restriction on machines. 
    /// </summary>
    public List<MachineDetails> contributingMachines { get; set; }
  }
}
