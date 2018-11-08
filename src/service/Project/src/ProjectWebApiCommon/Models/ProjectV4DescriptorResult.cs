using System.Collections.Immutable;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{

  /// <summary>
  /// List of project descriptors
  /// </summary>
  /// <seealso cref="ContractExecutionResult" />
  public class ProjectV4DescriptorsListResult : ContractExecutionResult
  {
    /// <summary>
    /// Gets or sets the project descriptors.
    /// </summary>
    /// <value>
    /// The project descriptors.
    /// </value>
    public ImmutableList<ProjectV4Descriptor> ProjectDescriptors { get; set; }
  }

  /// <summary>
  ///   Single project descriptor
  /// </summary>
  public class ProjectV4DescriptorsSingleResult : ContractExecutionResult
  {
    private ProjectV4Descriptor _projectV4Descriptor;

    public ProjectV4DescriptorsSingleResult(ProjectV4Descriptor projectV4Descriptor)
    {
      this._projectV4Descriptor = projectV4Descriptor;
    }

    /// <summary>
    /// Gets or sets the project descriptor.
    /// </summary>
    /// <value>
    /// The project descriptor.
    /// </value>
    public ProjectV4Descriptor ProjectDescriptor { get { return _projectV4Descriptor; } set { _projectV4Descriptor = value; } }
  }


  /// <summary>
  ///   Describes VL project
  /// </summary>
  public class ProjectV4Descriptor
  {
    /// <summary>
    /// Gets or sets the project uid.
    /// </summary>
    /// <value>
    /// The project uid.
    /// </value>
    public string ProjectUid { get; set; }

    /// <summary>
    /// Gets or sets the project ID from legacy VisionLink
    /// </summary>
    /// <value>
    /// The legacy project ID.
    /// </value>
    public int LegacyProjectId { get; set; }

    /// <summary>
    /// Gets or sets the type of the project.
    /// </summary>
    /// <value>
    /// The type of the project.
    /// </value>
    public ProjectType ProjectType { get; set; }

    /// <summary>
    /// Gets the name of the project type.
    /// </summary>
    /// <value>
    /// The name of the project type.
    /// </value>
    public string ProjectTypeName => this.ProjectType.ToString();

    /// <summary>
    /// Gets or sets the name of the project.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    public string Name { get; set; }
    
    /// <summary>
    /// Gets or sets the Description of the project.
    /// </summary>
    /// <value>
    /// The Description.
    /// </value>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the project time zone.
    /// </summary>
    /// <value>
    /// The project time zone.
    /// </value>
    public string ProjectTimeZone { get; set; }

    /// <summary>
    /// Gets or sets the IANA time zone.
    /// </summary>
    /// <value>
    /// The IANA project time zone.
    /// </value>
    public string IanaTimeZone { get; set; }

    /// <summary>
    /// Gets or sets the start date.
    /// </summary>
    /// <value>
    /// The start date.
    /// </value>
    public string StartDate { get; set; }

    /// <summary>
    /// Gets or sets the end date.
    /// </summary>
    /// <value>
    /// The end date.
    /// </value>
    public string EndDate { get; set; }
    
    /// <summary>
    /// Gets or sets the CustomerUID which the project is associated with
    /// </summary>
    /// <value>
    /// The Customer UID.
    /// </value>
    public string CustomerUid { get; set; }

    /// <summary>
    /// Gets or sets the customer Id from legacy VisionLink
    /// </summary>
    /// <value>
    /// The legacy Customer Id.
    /// </value>
    public string LegacyCustomerId { get; set; }

    /// <summary>
    /// Gets or sets the SubscriptionUID which the project is associated with
    /// </summary>
    /// <value>
    /// The subscription UID.
    /// </value>
    public string SubscriptionUid { get; set; }
    
    /// <summary>
    /// Gets or sets the Subscription start date.
    /// </summary>
    /// <value>
    /// The Subscription start date.
    /// </value>
    public string SubscriptionStartDate { get; set; }

    /// <summary>
    /// Gets or sets the Subscriptionend date.
    /// </summary>
    /// <value>
    /// The Subscription end date.
    /// </value>
    public string SubscriptionEndDate { get; set; }
    
    /// <summary>
    /// Gets or sets the type of the subscription.
    /// </summary>
    /// <value>
    /// The type of the subscription.
    /// </value>
    public ServiceTypeEnum ServiceType { get; set; }

    /// <summary>
    /// Gets the name of the subscription type.
    /// </summary>
    /// <value>
    /// The name of the subscription type.
    /// </value>
    public string ServiceTypeName => this.ServiceType.ToString();

    /// <summary>
    /// Gets or sets the project geofence.
    /// </summary>
    /// <value>
    /// The project geofence in WKT format.
    /// </value>
    public string ProjectGeofenceWKT { get; set; }
    
    /// <summary>
    /// Gets or sets the CoordinateSystem FileName which the project is associated with
    /// </summary>
    /// <value>
    /// The CoordinateSystem FileName.
    /// </value>
    public string CoordinateSystemFileName { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether this instance is archived.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is archived; otherwise, <c>false</c>.
    /// </value>
    public bool IsArchived { get; set; }


    public override bool Equals(object obj)
    {
      var otherProject = obj as ProjectV4Descriptor;
      if (otherProject == null) return false;
      return otherProject.ProjectUid == this.ProjectUid
             && otherProject.LegacyProjectId == this.LegacyProjectId
             && otherProject.ProjectType == this.ProjectType
             && otherProject.Name == this.Name
             && otherProject.Description == this.Description
             && otherProject.ProjectTimeZone == this.ProjectTimeZone
             && otherProject.IanaTimeZone == this.IanaTimeZone
             && otherProject.StartDate == this.StartDate
             && otherProject.EndDate == this.EndDate
             && otherProject.CustomerUid == this.CustomerUid
             && otherProject.LegacyCustomerId == this.LegacyCustomerId

             && otherProject.SubscriptionUid == this.SubscriptionUid
             && otherProject.SubscriptionStartDate == this.SubscriptionStartDate
             && otherProject.SubscriptionEndDate == this.SubscriptionEndDate
             && otherProject.ServiceType == this.ServiceType

             && otherProject.ProjectGeofenceWKT == this.ProjectGeofenceWKT
             && otherProject.CoordinateSystemFileName == this.CoordinateSystemFileName
             && otherProject.IsArchived == this.IsArchived
          ;
    }

    public override int GetHashCode() { return 0; }
  }
}