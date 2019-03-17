using System;
using System.ComponentModel.DataAnnotations;

namespace VSS.Productivity3D.AssetMgmt3D.Models
{
  /// <summary>
  /// Simple Filter that will be used as a start for a full blown 3D Productivity Filter
  /// </summary>
  public class SimpleFilter
  {
    /// <summary>
    /// Project Uid for the data.
    /// </summary>
    [Required]
    public string ProjectUid { get; set; }

    /// <summary>
    /// Optional Start Date amd Time in UTC to be used as the filter Start.
    /// Defaults to Project Start.
    /// </summary>
    public DateTime? StartDateUtc { get; set; }

    /// <summary>
    /// Optional End Date and Time in UTC to be used as the filter End.
    /// Defaults to Project End.
    /// </summary>
    public DateTime? EndDateUtc { get; set; }

    /// <summary>
    /// Optional Lift number to be used to filter data.
    /// Defaults to all Lifts.
    /// </summary>
    public int? LiftNumber { get; set; }

    /// <summary>
    /// Design file to be used when filtering data.
    /// </summary>
    [Required]
    public string DesignFileUid { get; set; }

    public override string ToString()
    {
      return $"ProjectUid: {ProjectUid} " +
             $"DesignFileUid: {DesignFileUid} " +
             $"StartDateUtc: {(StartDateUtc.HasValue ? StartDateUtc.Value.ToString("O") : "<none>")}, " +
             $"EndDateUtc: {(EndDateUtc.HasValue ? EndDateUtc.Value.ToString("O") : "<none>")}, " +
             $"LiftNumber: {(LiftNumber.HasValue ? LiftNumber.Value.ToString() : "<none>")} ";
    }
  }
}