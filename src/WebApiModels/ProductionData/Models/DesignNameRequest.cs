using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;

namespace VSS.Productivity3D.WebApiModels.ProductionData.Models
{
  /// <summary>
  /// Request to do operations on design file in design cache
  /// </summary>
  public class DesignNameRequest : ProjectID, IValidatable
  {

    /// <summary>
    /// Description to identify a design file in DesignCache.
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "DesignFilename", Required = Required.Always)]
    [Required]
    public string DesignFilename { get; private set; }
    
    public override void Validate()
    {
      base.Validate();
    }

    /// <summary>
    /// Creates a sample instance of the DesignNameRequest class to be displayed in Help documentation.
    /// </summary>
    /// 
    public new static DesignNameRequest HelpSample
    {
      get { return new DesignNameRequest { projectId = 1, DesignFilename = "MyDesign.ttm" }; }
    }
  }
}