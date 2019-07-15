using System;
using System.Collections.Generic;
using System.Linq;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Models.ResultHandling.Designs
{
  /// <summary>
  /// List of Designs
  /// </summary>
  /// <seealso cref="ContractExecutionResult" />
  public class DesignListResult : ContractExecutionResult, IMasterDataModel
  {
    /// <summary>
    /// Gets or sets the ImportedFile descriptors.
    /// </summary>
    /// <value>
    /// The ImportedFile descriptors.
    /// </value>
    public List<DesignFileDescriptor> DesignFileDescriptors { get; set; }

    public List<string> GetIdentifiers() => DesignFileDescriptors?
                                              .SelectMany(g => g.GetIdentifiers())
                                              .Distinct()
                                              .ToList()
                                            ?? new List<string>();
  }
  

  /// <summary>
  ///   Describes TRex design File 
  /// </summary>
  public class DesignFileDescriptor
  {

    /// <summary>
    /// Gets or sets the type of the file.
    ///     Only DesignSurface and SurveyedSurface are currently supported
    /// </summary>
    /// <value>
    /// The type of the file.
    /// </value>
    public ImportedFileType FileType { get; set; }
    
    /// <summary>
    /// Gets or sets the name of the file.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the design uid.
    ///    this originates from the ProjectSvc's ImportedFileUid
    /// </summary>
    /// <value>
    /// The design uid.
    /// </value>
    public string DesignUid { get; set; }

    /// <summary>
    /// Gets the bounding extents in 3d
    /// </summary>
    /// <value>
    /// The start date.
    /// </value>
    public BoundingExtents3D Extents { get; set; }

    /// <summary>
    /// Gets or sets the Surveyed Utc. 
    /// This only applies to a file type of SurveyedSurface, else null.
    /// </summary>
    /// <value>
    /// The start date.
    /// </value>
    public DateTime? SurveyedUtc { get; set; }

    /// <summary>
    /// Gets the name of the file type.
    /// </summary>
    /// <value>
    /// The name of the file type.
    /// </value>
    public string FileTypeName => FileType.ToString();

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    public override bool Equals(object obj)
    {
      var otherImportedFile = obj as DesignFileDescriptor;
      if (otherImportedFile == null) return false;
      return otherImportedFile.FileType == this.FileType
             && otherImportedFile.Name == this.Name
             && otherImportedFile.DesignUid == this.DesignUid
             && otherImportedFile.Extents == this.Extents
             && otherImportedFile.SurveyedUtc == this.SurveyedUtc
        ;
    }

    public List<string> GetIdentifiers() => new List<string>()
    {
      DesignUid.ToString()
    };
  }
}
