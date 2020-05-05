using System;
using System.Collections.Generic;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.TagFileForwarder.Models
{
  /// <summary>
  /// Represents a Customer
  /// </summary>
  public class CustomerDisplayModel
  {
    public CustomerDisplayModel()
    {
      Projects = new List<ProjectDisplayModel>();
    }
    
    /// <summary>
    /// Unique Identifier for the customer
    /// </summary>
    public string CustomerUid { get; set; }

    /// <summary>
    /// Customer Name
    /// </summary>
    public string CustomerName { get; set; }

    /// <summary>
    /// Projects attached to this Customer, both active or archived projects are included
    /// </summary>
    public List<ProjectDisplayModel> Projects { get; set; }
  }
}
