using System;
using System.Collections.Immutable;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{

  /// <summary>
  /// List of geofences descriptors
  /// </summary>
  /// <seealso cref="ContractExecutionResult" />
  public class GeofenceV4DescriptorsListResult : ContractExecutionResult
  {
    /// <summary>
    /// Gets or sets the project descriptors.
    /// </summary>
    /// <value>
    /// The project descriptors.
    /// </value>
    public ImmutableList<GeofenceV4Descriptor> GeofenceDescriptors { get; set; }
  }

 
  /// <summary>
  ///   Describes VL geofence
  /// </summary>
  public class GeofenceV4Descriptor
  {
    /// <summary>
    /// Gets or sets the geofence uid.
    /// </summary>
    /// <value>
    /// The geofence uid.
    /// </value>
    public string GeofenceUid { get; set; }

    /// <summary>
    /// Gets or sets the name of the geofence.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the type of the geofence.
    /// </summary>
    /// <value>
    /// The type of the geofence.
    /// </value>
    public GeofenceType GeofenceType { get; set; }

    /// <summary>
    /// Gets the name of the geofence type.
    /// </summary>
    /// <value>
    /// The name of the geofence type.
    /// </value>
    public string GeofenceTypeName => this.GeofenceType.ToString();

    /// <summary>
    /// Gets or sets the geofence.
    /// </summary>
    /// <value>
    /// The geofence in WKT format.
    /// </value>
    public string GeometryWKT { get; set; }

    /// <summary>
    /// Gets the fill color for the geofence.
    /// </summary>
    /// <value>
    /// The fill color
    /// </value>
    public int? FillColor { get; set; }

    /// <summary>
    /// Indicates whether to disaplay geofence as transparent.
    /// </summary>
    /// <value>
    /// Flag
    /// </value>
    public bool? IsTransparent { get; set; }

    /// <summary>
    /// Gets or sets the Description of the geofence.
    /// </summary>
    /// <value>
    /// The Description.
    /// </value>
    public string Description { get; set; }

    /// <summary>
    /// The CustomerUID which the geofence is associated with
    /// </summary>
    /// <value>
    /// The Customer UID.
    /// </value>
    public string CustomerUid { get; set; }

    /// <summary>
    /// The UserUid who created the geofence 
    /// </summary>
    /// <value>
    /// The User UID.
    /// </value> 
    public string UserUid { get; set; }

    /// <summary>
    /// The Area, in sqm of the geofence 
    /// </summary>
    /// <value>
    /// The area
    /// </value> 
    public double AreaSqMeters { get; set; }

    public override bool Equals(object obj)
    {
      if (!(obj is GeofenceV4Descriptor otherGeofence))
      {
        return false;
      }

      return otherGeofence.GeofenceUid == GeofenceUid
             && otherGeofence.Name == Name
             && otherGeofence.GeofenceType == GeofenceType
             && otherGeofence.GeometryWKT == GeometryWKT
             && otherGeofence.FillColor == FillColor
             && otherGeofence.IsTransparent == IsTransparent
             && otherGeofence.CustomerUid == CustomerUid
             && otherGeofence.UserUid == UserUid
             && Math.Abs(otherGeofence.AreaSqMeters - AreaSqMeters) < 0.0001;
    }

    public override int GetHashCode() { return 0; }
  }
}