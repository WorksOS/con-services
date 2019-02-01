using System;
using System.Collections.Generic;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.Now3D.Models
{
  public class CustomerDisplayModel
  {
    public CustomerDisplayModel()
    {
      Projects = new List<ProjectDisplayModel>();
    }
    
    public string CustomerUid { get; set; }

    public string CustomerName { get; set; }

    public List<ProjectDisplayModel> Projects { get; set; }
  }
}