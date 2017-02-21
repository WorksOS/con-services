
using Newtonsoft.Json;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;

namespace VSS.Raptor.Service.WebApi.ProductionData.Controllers
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
      get { return new DesignNameRequest() { projectId = 1, DesignFilename = "MyDesign.ttm" }; }
    }
  }
}